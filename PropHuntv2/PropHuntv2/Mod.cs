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

namespace WukongMp.PropHunt;

public class Mod : ModBase
{
    public override string Name => "PropHunt Mod";

    public static PropHuntRpc? Rpc { get; private set; } = null;
    public static PropHuntSystem? System { get; private set; } = null;

    public static UWorld? World { get; set; } = null;

    protected override void Initialize(IDependencyContainer services)
    {
        // register and resolve your services here, for example:
        services.RegisterSingleton<PropHuntRpc>();
        services.RegisterSingleton<PropHuntSystem>();
        Rpc = services.Resolve<PropHuntRpc>();
        System = services.Resolve<PropHuntSystem>();

        Core.InitializeDefaultMode();
        if (Core.CurrentGameMode is PropHuntGameMode gameMode)
        {
            gameMode.RegisterCommands();
        }
    }
}

