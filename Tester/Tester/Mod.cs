using CSharpModBase.Input;
using HarmonyLib;
using ReadyM.Api.Command;
using ReadyM.Api.DI;
using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Client;
using ReadyM.Api.Multiplayer.Generators;
using ReadyM.Api.Multiplayer.Protocol.Enums;
using ReadyM.Api.Multiplayer.RPC;
using ReadyM.Api.Multiplayer.Serialization;
using ReadyM.Wukong.Common.ECS.Components;
using ReadyM.Wukong.Common.ECS.Values;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Tester;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.Configuration;
using WukongMp.Sdk;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace Tester;

public class Mod : ModBase
{
    public static PlayerId? PlayerId { get; internal set; }

    public override string Name => "Tester Mod"; // TODO: CHANGE ME

    protected override void Initialize(IDependencyContainer services)
    {
        services.RegisterSingleton<TesterService>();
        PlayerTester.invoke();
        Spawner.Init();
        //WukongApi.Input.RegisterKeyBind(Key.F9, WorldTester.ReckonActors);
        WukongApi.Input.RegisterKeyBind(Key.F9, () => 
        {
            try
            {
                var location = WukongApi.Sync.LocalMainCharacter?.Location;
                var rotation = WukongApi.Sync.LocalMainCharacter?.Rotation;

                //Spawn actors next to the player
                WorldTester.SpawnActor("BPO_TreasureBox_03a_C", 
                    new FVector((double)location?.X + 300, (double)location?.Y, (double)location?.Z), 
                    new FRotator((double)rotation?.X, (double)rotation?.Y, (double)rotation?.Z));
                WorldTester.SpawnActor("BPO_TreasureBox_04a_C", 
                    new FVector((double)location?.X - 300, (double)location?.Y, (double)location?.Z), 
                    new FRotator((double)rotation?.X, (double)rotation?.Y, (double)rotation?.Z));
                WorldTester.SpawnActor("BPO_TreasureBox_06_C", 
                    new FVector((double)location?.X, (double)location?.Y + 300, (double)location?.Z), 
                    new FRotator((double)rotation?.X, (double)rotation?.Y, (double)rotation?.Z));
                WorldTester.SpawnActor("BPO_TreasureBox_08_C", 
                    new FVector((double)location?.X, (double)location?.Y - 300, (double)location?.Z), 
                    new FRotator((double)rotation?.X, (double)rotation?.Y, (double)rotation?.Z));
                WorldTester.TryChangeActors();
            }
            catch (Exception ex)
            {
                WukongApi.Chat.ShowLocalMessage("Error spawning actors: " + ex.Message, FLinearColor.Red);
            }
        });
        WukongApi.Input.RegisterKeyBind(Key.F8, () =>
        {
            if (WorldTester.TryGetMyPlayerActor() is not { } player)
            {
                WukongApi.Chat.ShowLocalMessage("Could not find player actor", FLinearColor.Red);
                return;
            }

            WukongApi.Chat.ShowLocalMessage($"Player hidden: {player.Hidden}, setting to {!player.Hidden}", FLinearColor.Yellow);
            player.SetActorHiddenInGame(!player.Hidden);
        });
    }
}