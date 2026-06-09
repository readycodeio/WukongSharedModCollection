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
using b1;
using System;
using BtlShare;

namespace WukongMp.PropHunt;

public class Mod : ModBase
{
    // DO TESTOWANIA
    // SHIFT+O - Reroll propa
    // Czy hider może atakować lub robić cokolwiek
    // Czy seeker może namierzać

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
            gameMode.RegisterBinds();
            WukongApi.Configuration.SetIsSkillEnabledQuery(QuerySkillAllowed);
        }
    }

    public static bool QuerySkillAllowed(int skillId)
    {
        try
        {
            if (!WukongApi.Sync.LocalPlayerId.HasValue)
            {
                //Logging.LogWarning("false "+ skillId);
                return false;
            }

            bool res = GameState.Seekers.Contains(WukongApi.Sync.LocalPlayerId.Value);
            if (res)
            {
                //Logging.LogWarning("true "+ skillId);
                return true;
            }
            else
            {
                //Logging.LogWarning("false" + skillId);
                return false;
            }

            //return GameState.Seekers.Contains(WukongApi.Sync.LocalPlayerId.Value);
        }
        catch (Exception ex)
        {
            Logging.LogError(ex.Message);
            return true;
        }
    }
}


[HarmonyPatch(typeof(BUS_PlayerInputActionComp), "DoAttackLogic")]
[HarmonyPatchCategory(PatchCategory.Global)]
public class BlockPropAttacksPatch
{
    public static bool Prefix(object __instance, EInputActionType actionType, bool isRelease, int descID)
    {
        try
        {
            if (!WukongApi.Sync.LocalPlayerId.HasValue) return true;
            var myId = WukongApi.Sync.LocalPlayerId.Value;
            if (GameState.IsGameActive && GameState.Hiders.Contains(myId))
            {
                switch (actionType)
                {
                    case EInputActionType.Dodge:
                    case EInputActionType.Jump:
                    case EInputActionType.Move:
                        return true;
                    default:
                        return false;
                }
            }
            else if (GameState.IsGameActive && GameState.Seekers.Contains(myId))
            {
                switch (actionType)
                {
                    case EInputActionType.CameraLock:
                    case EInputActionType.CameraLockPointHide:
                    case EInputActionType.CameraModeSwitch:
                        return false;
                    default:
                        return true;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            return true;
        }
    }
}

[HarmonyPatch(typeof(BUS_PlayerInputActionComp), "OnCameraLockTarget")]
[HarmonyPatchCategory(PatchCategory.Global)]
public class BlockCameraLockPatch
{
    public static bool Prefix(UnitLockTargetInfo TargetInfo)
    {
        return false;
    }
}