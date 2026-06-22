using CSharpModBase.Input;
using ReadyM.Api.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace WukongMp.PropHunt;

public class PropHuntGameMode : GameModeBase
{
    public readonly List<string> ConfigAutoParams = ["MaxHiderHp", "MaxSeekerHp", "MaxHunter", "MinHunter", "PreparationTime", "GameTime", "CustomTeams", "HpBarsVisible"];
    private static readonly List<int> _timeChecks = [60, 30, 10, 5, 3, 2, 1];
    public Queue<int> RemainingTimeChecks = new Queue<int>(_timeChecks);

    private static Random random = new Random();

    private float _currentRotation = 0f;
    private const float ROTATION_STEP = 5f;

    public override void Update(float deltaTime)
    {
        if (!WukongApi.Sync.IsMasterClient || !GameState.IsGameActive) return;

        GameState.ElapsedRoundTime += deltaTime;
        float remainingTime = Core.Config.GameTime - GameState.ElapsedRoundTime;

        if (!GameState.HasPreparationEnded && GameState.ElapsedRoundTime > Core.Config.PreparationTime)
        {
            GameState.HasPreparationEnded = true;
            Mod.Rpc?.SendSeekersStart();
        }

        //foreach (var player in WukongApi.Sync.AllMainCharacters.ToList())
        //{
        //    if (player.Hp <= 0f)
        //    {
        //        if (GameState.Hiders.Contains(player.PlayerId))
        //        {
        //            GameState.Hiders.Remove(player.PlayerId);
        //            Mod.Rpc?.SendPlayerFound(player.PlayerId);
        //        }
        //    }
        //}

        if (GameState.Hiders.Count == 0)
        {
            GameState.IsGameActive = false;
            Mod.Rpc?.SendGameEnd(Core.Team.Seeker);
        }

        if (GameState.ElapsedRoundTime >= Core.Config.GameTime)
        {
            GameState.IsGameActive = false;
            Mod.Rpc?.SendGameEnd(Core.Team.Hider);
        }

        if (RemainingTimeChecks.Count > 0)
        {
            if (remainingTime <= RemainingTimeChecks.Peek())
            {
                WukongApi.Chat.SendServerMessage($"Remaining time: {RemainingTimeChecks.Dequeue()}!");
                Mod.Rpc?.SendForceHidersPlaySound("Taunt_Stick");
            }
        }
    }

    public override void ClientUpdate(float deltaTime)
    {
        if (!GameState.IsGameActive || WukongApi.Sync.LocalPlayerId is not { } myId) return;

        GameState.ElapsedTauntTime += deltaTime;
        GameState.ElapsedTranformTime += deltaTime;
        GameState.ElapsedDecoyTime += deltaTime;

        if (GameState.ElapsedTranformTime >= GameState.TransformDownTime)
        {
            GameState.CanPerformTransform = true;
        }
        else
        {
            GameState.CanPerformTransform = false;
        }

        if (!GameState.HasPreparationEnded && GameState.Seekers.Contains(myId))
        {
            if (GameState.PlayerActors.TryGetValue(myId, out var actor))
            {
                var targetLocation = GameState.OriginalSeekerLocation;
                targetLocation.X += 50_000;

                actor.Teleport(targetLocation, FRotator.ZeroRotator);
            }
        }
    }

    public override void OnStart()
    {
        if (!WukongApi.Sync.IsMasterClient) return;

        RemainingTimeChecks = new Queue<int>(_timeChecks);

        int playerCount = WukongApi.Sync.AllMainCharacters.Count();
        if (playerCount < 2)
        {
            WukongApi.Chat.ShowLocalMessage($"Unable to start the game: Not enough players! Min:2, Current: {playerCount}", FLinearColor.Red);
            return;
        }

        GameState.Reset();
        RollTeams(Core.Config.CustomTeams);
        GameState.IsGameActive = true;

        var snapshot = GameState.CreateSnapshop();
        var config = Core.Config;

        Mod.Rpc?.SendGameStart(snapshot, config);
    }

    public override void OnEnd(Core.Team winnerTeam)
    {
        switch (winnerTeam)
        {
            case Core.Team.Hider:
                GameState.HidersScore++;
                break;
            case Core.Team.Seeker:
                GameState.SeekersScore++;
                break;
            default:
                break;
        }
        WukongApi.Widgets.ShowInfoMessage($"Seekers: {GameState.SeekersScore} | Hiders: {GameState.HidersScore}");
        GameState.IsGameActive = false;

        GameState.RebirthPlayers();
    }

    public void ManagePlayerJoin(ReadyMainCharacter character)
    {
        if (!WukongApi.Sync.IsMasterClient) return;
        if (!GameState.IsGameActive) return;
        
        GameState.Spectators.Add(character.PlayerId);
    }

    public void RegisterBinds()
    {
        WukongApi.Input.RegisterKeyBind(Key.RIGHT, BindRotatePropRight);
        WukongApi.Input.RegisterKeyBind(Key.LEFT, BindRotatePropLeft);
        WukongApi.Input.RegisterKeyBind(ModifierKeys.Shift, Key.F2, BindRerollProp);
        WukongApi.Input.RegisterKeyBind(ModifierKeys.Shift, Key.F3, BindTaunt);
        WukongApi.Input.RegisterKeyBind(ModifierKeys.Shift, Key.F4, BindPlaceDecoy);
    }

    private void BindPlaceDecoy()
    {
        if (!WukongApi.Input.CanApplyInput()) return;
        if (!GameState.IsGameActive) return;
        if (GameState.ElapsedDecoyTime < GameState.DecoyDownTime)
        {
            WukongApi.Local.ShowTip($"Wait {Math.Round(GameState.DecoyDownTime - GameState.ElapsedDecoyTime,2)} before next decoy", true);
            return;
        }

        if (GameState.PlacedDecoys >= GameState.DecoyLimit)
        {
            WukongApi.Local.ShowTip("Decoy limit reached!", true);
            return;
        }
        string propName = string.Empty;
        propName = Utils.propClassNames[random.Next(Utils.propClassNames.Length)];

        GameState.ElapsedDecoyTime = 0f;
        GameState.PlacedDecoys++;
        WukongApi.Local.ShowTip($"Decoys in place: {GameState.PlacedDecoys}/{GameState.DecoyLimit}", true);
        Mod.Rpc?.SendDecoyDeploy(propName);
    }

    private void BindTaunt()
    {
        if (!WukongApi.Input.CanApplyInput()) return;
        if (!GameState.IsGameActive) return;

        if (GameState.ElapsedTauntTime >= GameState.TauntDownTime)
        {
            GameState.ElapsedTauntTime = 0f;
            Random random = new Random();
            var sound = GameState.GameSounds[random.Next(GameState.GameSounds.Count)];

            Mod.Rpc?.SendHiderPlaySound(sound);
        }
        else
        {
            WukongApi.Local.ShowTip($"Wait {Math.Round(GameState.TauntDownTime - GameState.ElapsedTauntTime,2)} before next taunt!", true);
        }
    }

    private void BindRerollProp()
    {
        if (!GameState.IsGameActive) return;
        if (!WukongApi.Input.CanApplyInput()) return;

        if (GameState.CanPerformTransform)
        {
            GameState.ElapsedTranformTime = 0f;
            string newPropName;

            int currentIndex = Array.IndexOf(Utils.propClassNames, GameState.CurrentPropName);

            if (currentIndex == -1)
            {
                newPropName = Utils.propClassNames[random.Next(Utils.propClassNames.Length)];
            }
            else
            {
                int randomIndex = random.Next(Utils.propClassNames.Length - 1);

                if (randomIndex >= currentIndex)
                {
                    randomIndex++;
                }

                newPropName = Utils.propClassNames[randomIndex];
            }

            GameState.CurrentPropName = newPropName;
            Mod.Rpc?.SendPlayerHide(WukongApi.Sync.LocalPlayerId.Value, newPropName);
            WukongApi.Local.ShowTip($"You are: {newPropName}", true);
        }
        else
        {
            WukongApi.Local.ShowTip($"Wait {Math.Round(GameState.TransformDownTime - GameState.ElapsedTranformTime)} before next transformation", true);
        }
    }

    private void BindRotatePropLeft()
    {
        if (!WukongApi.Input.CanApplyInput()) return;

        RotateProp(-ROTATION_STEP);
    }

    private void BindRotatePropRight()
    {
        if (!WukongApi.Input.CanApplyInput()) return; 
        
        RotateProp(ROTATION_STEP);
    }

    private void RotateProp(float angleDelta)
    {
        if (!GameState.Hiders.Contains(WukongApi.Sync.LocalPlayerId.Value)) return;

        if (GameState.PlayerProps.TryGetValue(WukongApi.Sync.LocalPlayerId.Value, out var myProp) && myProp != null)
        {
            _currentRotation += angleDelta;

            if (_currentRotation >= 360f) _currentRotation -= 360f;
            if (_currentRotation < 0f) _currentRotation += 360f;

            FRotator newRotation = new FRotator(0f, _currentRotation, 0f);

            myProp.SetActorRelativeRotation(newRotation, false, out _, false);

            Mod.Rpc?.SendPropRotate(_currentRotation);
        }
    }
    public void RegisterCommands()
    {
        WukongApi.Console.AddCommand("prop", ConsoleCommand.Create(CommandManageGame), ["Start", /*"Pause",*/ "End"]);
        WukongApi.Console.AddCommand("propConfig", ConsoleCommand.Create(CommandConfigGame), ConfigAutoParams);
    }

    private void CommandManageGame(string command)
    {
        string option = command.ToLower();

        switch (option) 
        {
            case "start":
                CommandStartGame();
                break;
            //case "pause":
            //    CommandPauseGame();
            //    break;
            case "end":
                CommandEndGame();
                break;
            default:
                WukongApi.Console.LogMessage("No such argument as "+option);
                break;
        }
    }

    private void CommandStartGame()
    {
        if (!WukongApi.Sync.IsMasterClient)
        {
            WukongApi.Console.LogMessage("Only host can start the game!");
            return;
        }

        if (GameState.IsGameActive)
        {
            WukongApi.Console.LogMessage("Cannot start new game during active round!");
            return;
        }

        OnStart();
    }

    public void RollTeams(bool customTeams)
    {
        GameState.Reset();
        var allPlayers = WukongApi.Sync.AllPlayers.ToList();
        if (customTeams || allPlayers.Count == 0) return;

        Random rand = new Random();

        int maxAllowedSeekers = Math.Max(1, allPlayers.Count / 2);

        int minSeekers = Math.Min(Core.Config.MinHunterCount, maxAllowedSeekers);
        int maxSeekers = Math.Min(Core.Config.MaxHunterCount, maxAllowedSeekers);

        int targetSeekersCount = rand.Next(minSeekers, maxSeekers + 1);

        while (GameState.Seekers.Count < targetSeekersCount && allPlayers.Count > 0)
        {
            var randomPlayer = allPlayers[rand.Next(allPlayers.Count)];
            GameState.Seekers.Add(randomPlayer);
        }

        foreach (var player in WukongApi.Sync.AllPlayers)
        {
            if (!GameState.Seekers.Contains(player))
            {
                GameState.Hiders.Add(player);
            }
        }
    }

    private void CommandPauseGame()
    {

    }

    private void CommandEndGame()
    {
        if (!WukongApi.Sync.IsMasterClient)
        {
            WukongApi.Console.LogMessage("Only host can use this command!");
            return;
        }

        WukongApi.Chat.SendServerMessage("Game forcibly ended by host!");
        OnEnd(Core.Team.None);
    }

    private void CommandConfigGame(string option, int value)
    {
        if (!WukongApi.Sync.IsMasterClient)
        {
            WukongApi.Console.LogMessage("Only host can configure game settings!");
            return;
        }

        if (GameState.IsGameActive)
        {
            WukongApi.Console.LogMessage("Cannot configure game settings during active round!");
            return;
        }

        option = option.ToLower();

        switch (option)
        {
            case "maxhiderhp":
                WukongApi.Console.LogMessage($"Maximum hider hp set to: {value}");
                Core.Config.MaxHiderHp = value;
                break;
            case "maxseekerhp":
                WukongApi.Console.LogMessage($"Maximum seeker hp set to: {value}");
                Core.Config.MaxSeekerHp = value;
                break;
            case "maxhunter":
                WukongApi.Console.LogMessage($"Maximum hunter count set to: {value}");
                Core.Config.MaxHunterCount = value;
                break;
            case "minhunter":
                WukongApi.Console.LogMessage($"Minimum hunter count set to: {value}");
                Core.Config.MinHunterCount = value;
                break;
            case "preparationtime":
                WukongApi.Console.LogMessage($"Preparation phase time set to: {value}s");
                Core.Config.PreparationTime = value;
                break;
            case "gametime":
                WukongApi.Console.LogMessage($"Game round time limit set to: {value}s");
                Core.Config.GameTime = value;
                break;
            case "customteams":
                if (value > 0) Core.Config.CustomTeams = true;
                else Core.Config.CustomTeams = false;
                WukongApi.Console.LogMessage($"Custom teams set to {Core.Config.CustomTeams}");
                break;
            case "destroytamers":
                if (value > 0) Core.Config.DestroyTamers = true;
                else Core.Config.DestroyTamers = false;
                WukongApi.Console.LogMessage($"Destroy tamers (enemies) on game start set to {Core.Config.DestroyTamers}");
                break;
            case "hpbarsvisible":
                if (value > 0) Core.Config.BloodBarsVisible = true;
                else Core.Config.BloodBarsVisible = false;
                WukongApi.Console.LogMessage($"Hp bars visibility set to {Core.Config.BloodBarsVisible}");
                break;
            default:
                WukongApi.Console.LogMessage("Invalid argument " + option);
                return;
        }
    }
}
