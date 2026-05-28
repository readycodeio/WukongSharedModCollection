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
using System;
using System.Linq;
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
    }
}
