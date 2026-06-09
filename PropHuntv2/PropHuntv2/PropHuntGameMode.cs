using b1;
using CSharpModBase.Input;
using ReadyM.Api.Command;
using ReadyM.Api.Idents;
using ReadyM.Wukong.Common.ECS.Components;
using ReadyM.Wukong.Common.ECS.Values;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using UnrealEngine.AssetRegistry;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.WukongUtils;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace WukongMp.PropHunt;

public class PropHuntGameMode : GameModeBase
{
    public readonly List<string> ConfigAutoParams = ["MaxHidersHp", "MaxSeekerHp", "MaxHunter", "MinHunter", "PreparationTime", "GameTime", "CustomTeams"];
    private static readonly List<int> _timeChecks = [60, 30, 10, 5, 3, 2, 1];
    public Queue<int> RemainingTimeChecks = new Queue<int>(_timeChecks);

    private float _currentRotation = 0f;
    private const float ROTATION_STEP = 5f;

    //public List<FAssetData> _testSoundAssets = new List<FAssetData>();
    //public int _currentSoundIndex = 0;
    //public float _soundTimer = 0f;
    //public bool _isSoundTestActive = false;

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

        foreach (var player in WukongApi.Sync.AllMainCharacters)
        {
            if (player.Hp <= 0f)
            {
                if (GameState.Hiders.Contains(player.PlayerId))
                {
                    GameState.Hiders.Remove(player.PlayerId);
                    Mod.Rpc?.SendPlayerFound(player.PlayerId);
                }
            }
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
        //if (_isSoundTestActive && _testSoundAssets.Count > 0)
        //{
        //    _soundTimer += deltaTime;

        //    if (_soundTimer >= 1.0f)
        //    {
        //        _soundTimer = 0f;
        //        PlayNextTestSound();
        //    }
        //}
        if (!GameState.IsGameActive || WukongApi.Sync.LocalPlayerId is not { } myId) return;

        GameState.ElapsedTranformTime += deltaTime;

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
                targetLocation.Z += 5_000;

                var rotation = actor.GetActorRotation();
                actor.Teleport(targetLocation, FRotator.ZeroRotator);
            }
        }
    }
    //private void PlayNextTestSound()
    //{
    //    if (_currentSoundIndex >= _testSoundAssets.Count)
    //    {
    //        _isSoundTestActive = false;
    //        WukongApi.Chat.ShowLocalMessage("Stopped playing", FLinearColor.Yellow);
    //        return;
    //    }

    //    var assetData = _testSoundAssets[_currentSoundIndex];
    //    string assetName = UAssetRegistryHelpers.GetFullName(assetData);
    //    _currentSoundIndex++;

    //    try
    //    {
    //        UObject loadedObject = UAssetRegistryHelpers.GetAsset(assetData);

    //        if (loadedObject is USoundBase mySound)
    //        {
    //            WukongApi.Chat.ShowLocalMessage($"Playing [{_currentSoundIndex}/{_testSoundAssets.Count}] {assetName}",FLinearColor.AliceBlue);

    //            AActor? myActor = Utils.GetMyPlayer();
    //            if (myActor != null)
    //            {
    //                UGameplayStatics.PlaySoundAtLocation(
    //                    GameUtils.GetWorld(),
    //                    mySound,
    //                    myActor.GetActorLocation(),
    //                    FRotator.ZeroRotator,
    //                    5.0f, 1.0f, 0.0f,
    //                    null, null, myActor, null
    //                );
    //            }
    //            //else
    //            //{
    //            //    WukongApi.Chat.ShowLocalMessage("No actor found",FLinearColor.Red);
    //            //}
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        WukongApi.Console.LogMessage($"Error {assetName} {ex.Message}");
    //    }
    //}

    public override void OnStart()
    {
        if (!WukongApi.Sync.IsMasterClient) return;

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

        if (GameState.IsGameActive)
        {
            GameState.Spectators.Add(character.PlayerId);
        }
    }

    public void RegisterBinds()
    {
        WukongApi.Input.RegisterKeyBind(Key.RIGHT, BindRotatePropRight);
        WukongApi.Input.RegisterKeyBind(Key.LEFT, BindRotatePropLeft);
        WukongApi.Input.RegisterKeyBind(ModifierKeys.Shift, Key.O, BindRerollProp);
    }

    private void BindRerollProp()
    {
        if (GameState.CanPerformTransform)
        {
            GameState.ElapsedTranformTime = 0f;
            Random random = new Random();
            string newPropName = Utils.propClassNames[random.Next(Utils.propClassNames.Length)];
            Mod.Rpc?.SendPlayerHide(WukongApi.Sync.LocalPlayerId.Value, newPropName);
            WukongApi.Local.ShowTip($"You are: {newPropName}", true);
        }
        else
        {
            WukongApi.Local.ShowTip($"Time until next transform: {GameState.TransformDownTime - GameState.ElapsedTranformTime}", true);
        }
    }

    private void BindRotatePropLeft()
    {
        if (WukongApi.Input.CanApplyInput())
        {
            RotateProp(-ROTATION_STEP);
        }
    }

    private void BindRotatePropRight()
    {
        if (WukongApi.Input.CanApplyInput())
        {
            RotateProp(ROTATION_STEP);
        }
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
        WukongApi.Console.AddCommand("prop", ConsoleCommand.Create(CommandManageGame), ["Start", "Pause", "End"]);
        WukongApi.Console.AddCommand("propConfig", ConsoleCommand.Create(CommandConfigGame), ConfigAutoParams);
        WukongApi.Console.AddCommand("test", ConsoleCommand.Create(SpawnTestingProp));
        //WukongApi.Console.AddCommand("testt", ConsoleCommand.Create(() =>
        //{
        //    try
        //    {
        //var objects = UGameplayStatics.GetObjects<USoundWave>(EObjectFlags.NoFlags, true, EInternalObjectFlags.None);

        //USoundBase? o = null;

        //foreach (var obj in objects)
        //{
        //    if (obj != null && !obj.PathName.Contains("Default__") && !obj.PathName.StartsWith("/Script/"))
        //    {
        //        o = obj;
        //        break;
        //    }
        //}
        //USoundWave? o = null;
        //while (objects.MoveNext())
        //{
        //    var obj = objects.Current;
        //    if (obj == null)
        //        continue;
        //    if (obj.PathName.Contains("Default__"))
        //        continue;
        //    if (obj.PathName.StartsWith("/scripts/"))
        //        continue;

        //    o = obj;
        //    break;
        //}

        //int count = 0;
        //while (objects.MoveNext() && count < 10)
        //{
        //    var obj = objects.Current;
        //    if (obj != null)
        //    {
        //        WukongApi.Chat.ShowLocalMessage($"{obj.PathName}", FLinearColor.Yellow);
        //        count++;
        //    }
        //}

        //if (o == null)
        //{
        //    WukongApi.Chat.ShowLocalMessage("Not found", FLinearColor.Red);
        //    return;
        //}

        //WukongApi.Chat.ShowLocalMessage($"{o?.GetType().Name}", FLinearColor.AliceBlue);
        //WukongApi.Chat.ShowLocalMessage($"{o?.GetType().FullName}", FLinearColor.AliceBlue);
        //WukongApi.Chat.ShowLocalMessage($"{o?.PathName}", FLinearColor.AliceBlue);
        //IAssetRegistry assetRegistry = UAssetRegistryHelpers.GetAssetRegistry();

        //List<FAssetData> assetDataList = new List<FAssetData>();

        //assetRegistry.GetAssetsByClass(new FName("SoundBase"), out assetDataList, true);

        //int znalezione = 0;

        //foreach (var assetData in assetDataList)
        //{
        //    string assetName = UAssetRegistryHelpers.GetFullName(assetData);

        //    if (!assetName.Contains("Default__") && !assetName.StartsWith("/Script/"))
        //    {
        //        WukongApi.Console.LogMessage($"Znaleziono SoundBase: {assetName}");
        //        znalezione++;

        //        if (znalezione >= 10)
        //        {
        //            break;
        //        }
        //    }
        //}

        //if (znalezione == 0)
        //{
        //    WukongApi.Chat.ShowLocalMessage("Empty", FLinearColor.Rdd);
        //}
        //foreach (var assetData in assetDataList)
        //{
        //    string assetName = UAssetRegistryHelpers.GetFullName(assetData);

        //    if (assetName.ToLower().Contains("gong") && !assetName.Contains("Default__"))
        //    {
        //        UObject loadedObject = UAssetRegistryHelpers.GetAsset(assetData);

        //        if (loadedObject is USoundBase mySound)
        //        {
        //            WukongApi.Chat.ShowLocalMessage($"{assetName}", FLinearColor.Green);

        //UGameplayStatics.PlaySoundAtLocation(
        //        GameUtils.GetWorld(),
        //        mySound,
        //        WukongApi.Sync.LocalMainCharacter.Value.Location.ToFVector(),
        //        FRotator.ZeroRotator,
        //        1.0f,
        //        1.0f,
        //        0.0f,
        //        null,
        //        null,
        //        Utils.GetCharacterActor(WukongApi.Sync.LocalMainCharacter.Value),
        //        null
        //    );
        //            break;
        //        }
        //    }
        //}

        //        IAssetRegistry assetRegistry = UAssetRegistryHelpers.GetAssetRegistry();
        //        List<FAssetData> assetDataList = new List<FAssetData>();

        //        assetRegistry.GetAssetsByClass(new FName("SoundWave"), out assetDataList, true);

        //        USoundBase? soundToPlay = null;

        //        foreach (var assetData in assetDataList)
        //        {
        //            string assetName = UAssetRegistryHelpers.GetFullName(assetData);

        //            if (assetName.ToLower().Contains("ui") || assetName.ToLower().Contains("beep") || assetName.ToLower().Contains("warn"))
        //            {
        //                UObject loadedObject = UAssetRegistryHelpers.GetAsset(assetData);

        //                if (loadedObject is USoundBase mySound)
        //                {
        //                    soundToPlay = mySound;
        //                    WukongApi.Console.LogMessage($"Zaladowano dzwiek odliczania: {assetName}");
        //                    break;
        //                }
        //            }
        //        }

        //        if (soundToPlay != null)
        //        {
        //            AActor? myActor = Utils.GetMyPlayer();
        //            if (myActor != null)
        //            {
        //                UGameplayStatics.PlaySoundAtLocation(
        //                    GameUtils.GetWorld(),
        //                    soundToPlay,
        //                    myActor.GetActorLocation(),
        //                    FRotator.ZeroRotator,
        //                    1.0f, 1.0f, 0.0f,
        //                    null, null, myActor, null
        //                );
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        WukongApi.Chat.ShowLocalMessage(ex.Message, FLinearColor.Red);
        //    }
        //}));
        //WukongApi.Console.AddCommand("tsound", ConsoleCommand.Create(() =>
        //{
        //    if (_isSoundTestActive)
        //    {
        //        _isSoundTestActive = false;
        //        WukongApi.Chat.ShowLocalMessage("Test stopped", FLinearColor.Yellow);
        //        return;
        //    }

        //    _testSoundAssets.Clear();

        //    IAssetRegistry assetRegistry = UAssetRegistryHelpers.GetAssetRegistry();
        //    List<FAssetData> allSounds = new List<FAssetData>();

        //    assetRegistry.GetAssetsByClass(new FName("SoundWave"), out allSounds, true);

        //    int added = 0;
        //foreach (var asset in allSounds)
        //{
        //    string name = UAssetRegistryHelpers.GetFullName(asset).ToLower();

        //    if (!name.Contains("default__") && !name.StartsWith("/script/"))
        //    {
        //        _testSoundAssets.Add(asset);
        //        added++;

        //        if (added >= 10) break;
        //    }
        //}

        //    if (_testSoundAssets.Count > 0)
        //    {
        //        _currentSoundIndex = 0;
        //        _soundTimer = 0f;
        //        _isSoundTestActive = true;
        //        WukongApi.Chat.ShowLocalMessage($"{_testSoundAssets.Count}", FLinearColor.Green);
        //    }
        //    else
        //    {
        //        WukongApi.Chat.ShowLocalMessage("Brak", FLinearColor.Red);
        //    }
        //}));
    }

    private void CommandManageGame(string command)
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
                Core.Config.MaxHiderHp = value;
                break;
            case "maxseekerhp":
                Core.Config.MaxSeekerHp = value;
                break;
            case "maxhunter":
                Core.Config.MaxHunterCount = value;
                break;
            case "minhunter":
                Core.Config.MinHunterCount = value;
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
            case "destroytamers":
                if (value > 0) Core.Config.DestroyTamers = true;
                else Core.Config.DestroyTamers = false;
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