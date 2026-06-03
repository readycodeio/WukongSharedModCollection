using CSharpModBase.Input;
using ReadyM.Api.Command;
using ReadyM.Api.Idents;
using ReadyM.Wukong.Common.ECS.Components;
using ReadyM.Wukong.Common.ECS.Values;
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
    public readonly List<string> ConfigAutoParams = ["MaxHidersHp", "MaxSeekerHp", "MaxHunter", "MinHunter", "PreparationTime", "GameTime", "CustomTeams"];
    private static readonly List<int> _timeChecks = [60, 30, 10, 5, 3, 2, 1];
    public Queue<int> RemainingTimeChecks = new Queue<int>(_timeChecks);


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
            }
        }
    }

    public override void ClientUpdate(float deltaTime)
    {
        if (!GameState.IsGameActive || WukongApi.Sync.LocalPlayerId is not { } myId) return;

        if (!GameState.HasPreparationEnded && GameState.Seekers.Contains(myId))
        {
            if (GameState.PlayerActors.TryGetValue(myId, out var actor))
            {
                var targetLocation = GameState.OriginalSeekerLocation;
                targetLocation.Z += 5_000;

                var rotation = actor.GetActorRotation();
                actor.Teleport(targetLocation, FRotator.ZeroRotator);
            }
        }
    }

    public override void OnStart()
    {
        if (!WukongApi.Sync.IsMasterClient) return;

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
        WukongApi.Local.ShowInfoMessage($"Seekers: {GameState.SeekersScore} | Hiders: {GameState.HidersScore}");
        GameState.IsGameActive = false;
        GameState.RebirthPlayers();
    }

    public void ManagePlayerJoin(ReadyMainCharacter character)
    {
        if (!WukongApi.Sync.IsMasterClient) return;

        if (GameState.IsGameActive)
        {
            GameState.Spectators.Add(character.PlayerId);
        }
    }

    public void RegisterCommands()
    {
        WukongApi.Console.AddCommand("prop", ConsoleCommand.Create(ManageGame), ["Start", "Pause", "End"]);
        WukongApi.Console.AddCommand("propConfig", ConsoleCommand.Create(CommandConfigGame), ConfigAutoParams);
        WukongApi.Console.AddCommand("test", ConsoleCommand.Create(SpawnTestingProp));
    }

    private void ManageGame(string command)
    {
        string option = command.ToLower();

        switch (option) 
        {
            case "start":
                CommandStartGame();
                break;
            case "pause":
                CommandPauseGame();
                break;
            case "end":
                CommandEndGame();
                break;
            default:
                WukongApi.Console.LogMessage("No such argument as"+option);
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

        int minSeekers = Math.Min(Core.Config.MinHunter, maxAllowedSeekers);
        int maxSeekers = Math.Min(Core.Config.MaxHunter, maxAllowedSeekers);

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
        OnEnd(Core.Team.Spectator);
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
                Core.Config.MaxHiderHp = value;
                break;
            case "maxseekerhp":
                Core.Config.MaxSeekerHp = value;
                break;
            case "maxhunter":
                Core.Config.MaxHunter = value;
                break;
            case "minhunter":
                Core.Config.MinHunter = value;
                break;
            case "preparationtime":
                Core.Config.PreparationTime = value;
                break;
            case "gametime":
                Core.Config.GameTime = value;
                break;
            case "customteams":
                if (value > 0) Core.Config.CustomTeams = true;
                else Core.Config.CustomTeams = false;
                break;
            default:
                WukongApi.Console.LogMessage("Invalid argument " + option);
                return;
        }
    }

    public static void SpawnTestingProp(int value)
    {
        var myActor = Utils.GetMyPlayer();
        if (myActor == null)
        {
            return;
        }

        var propName = Utils.propClassNames[value];

        FVector location = myActor.GetActorLocation();
        FRotator rotation = myActor.GetActorRotation();

        var prop = Utils.SpawnProp(propName, location, rotation, myActor, false);
        //var prop = TestProp(propName, location, rotation, myActor, false);
        //if (prop != null)
        //{
        //    WukongApi.Chat.ShowLocalMessage($"Zespawnowano prop: {propName}", FLinearColor.Green);
        //}
    }

    public static AActor? TestProp(string propClassName, FVector location, FRotator rotation, AActor actorToIgnore, bool enableCollision = false)
    {
        AActor? prop = Utils.SpawnActor(propClassName, location, rotation);
        if (prop == null) return null;

        prop.GetActorBounds(false, out FVector pOrigin, out FVector pExtent, false);

        float relativeZ = -122.3f - 90.0f + pExtent.Z;

        prop.AttachToActor(actorToIgnore, new FName(""), EAttachmentRule.SnapToTarget, EAttachmentRule.SnapToTarget, EAttachmentRule.KeepWorld, false);

        prop.SetActorRelativeLocation(new FVector(0, 0, relativeZ), false, out _, false);

        prop.SetActorHiddenInGame(false);
        prop.SetActorEnableCollision(enableCollision);

        return prop;
    }
}