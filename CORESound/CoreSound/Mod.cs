using CSharpModBase.Input;
using ReadyM.Api.Command;
using ReadyM.Api.DI;
using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Client;
using ReadyM.Api.Multiplayer.Generators;
using ReadyM.Api.Multiplayer.Protocol.Enums;
using ReadyM.Api.Multiplayer.RPC;
using ReadyM.Api.Multiplayer.Serialization;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.Configuration;
using WukongMp.Sdk;
using WukongMp.Sdk.Api;
using HarmonyLib;
using CoreSound;

namespace ExampleMod;

public class Mod : ModBase
{
    public override string Name => "BasicSoundCore";

    private static SoundCoreSystem sSystem;
    private static SoundCoreService sService;

    protected override void Initialize(IDependencyContainer services)
    {
        services.RegisterSingleton<SoundCoreSystem>();
        services.RegisterSingleton<SoundCoreService>();

        sSystem = services.Resolve<SoundCoreSystem>();
        sService = services.Resolve<SoundCoreService>();

        // register and resolve your services here, for example:

        // use the WukongApi class to interact with the SDK, for example:

        // register input bindings, for example:
    }
}