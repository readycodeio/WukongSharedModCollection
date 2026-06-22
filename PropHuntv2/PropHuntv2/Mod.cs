using b1;
using b1.ECS;
using b1.UI.Comm;
using BtlShare;
using CoreSound;
using HarmonyLib;
using ReadyM.Api.DI;
using System;
using System.Collections.Generic;
using UnrealEngine.Engine;
using WukongMp.Api;
using WukongMp.Api.Configuration;
using WukongMp.Sdk;
using WukongMp.Sdk.Api;

namespace WukongMp.PropHunt;

public class Mod : ModBase
{
    public override string Name => "PropHunt Mod";

    public static PropHuntRpc? Rpc { get; private set; } = null;
    public static PropHuntSystem? System { get; private set; } = null;
    public static PropHuntService? Service { get; private set; } = null;

    public static SoundCoreService? SService { get; private set; } = null;
    public static SoundCoreSystem? SSystem { get; private set; } = null;

    public static UWorld? World { get; set; } = null;

    protected override void Initialize(IDependencyContainer services)
    {
        // register and resolve your services here, for example:
        services.RegisterSingleton<PropHuntRpc>();
        services.RegisterSingleton<PropHuntSystem>();
        services.RegisterSingleton<PropHuntService>();

        Rpc = services.Resolve<PropHuntRpc>();
        System = services.Resolve<PropHuntSystem>();
        Service = services.Resolve<PropHuntService>();
        
        services.RegisterSingleton<SoundCoreService>();
        services.RegisterSingleton<SoundCoreSystem>();
        
        SService = services.Resolve<SoundCoreService>();
        SSystem = services.Resolve<SoundCoreSystem>();

        Core.InitializeDefaultMode();

        if (Core.CurrentGameMode is PropHuntGameMode gameMode)
        {
            gameMode.RegisterCommands();
            gameMode.RegisterBinds();
            WukongApi.Configuration.SetIsSkillEnabledQuery(QuerySkillAllowed);
        }

        //var haramony = new Harmony("WukongMp.PropHunt");
        //var originalBloodBarUI = AccessTools.Method(typeof(BUI_BattleInfoCS), "InitBloodBarUI");
        //var patchInfo = Harmony.GetPatchInfo(originalBloodBarUI);
        //var myPrefix = AccessTools.Method(typeof(PatchInitBloodBarUI), "Prefix");

        //if (patchInfo != null)
        //{
        //    foreach (var prefix in patchInfo.Prefixes)
        //    {
        //        if (prefix.owner != haramony.Id)
        //        {
        //            haramony.Unpatch(originalBloodBarUI, HarmonyPatchType.Prefix, prefix.owner);
        //        }
        //    }
        //}
        //haramony.Patch(originalBloodBarUI, prefix: new HarmonyMethod(myPrefix));
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

[HarmonyPatch(typeof(BUS_PlayerInputActionComp), "DoAttackLogic", typeof(EInputActionType), typeof(bool), typeof(int))]
[HarmonyPatchCategory(PatchCategory.Global)]
public static class BlockPropAttacksPatch
{
    public static bool Prefix(EInputActionType InputActionType, bool IsRelease, int DescID)
    {
        try
        {
            if (!WukongApi.Sync.LocalPlayerId.HasValue) return true;
            var myId = WukongApi.Sync.LocalPlayerId.Value;

            if (GameState.IsGameActive && GameState.Hiders.Contains(myId))
            {
                switch (InputActionType)
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
                switch (InputActionType)
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
            Logging.LogError(ex.Message);
            return true;
        }
    }
}

[HarmonyPatch(typeof(BUS_PlayerInputActionComp), "OnCameraLockTarget")]
[HarmonyPatchCategory(PatchCategory.Global)]
public static class BlockCameraLockPatch
{
    public static bool Prefix()
    {
        return false;
    }
}

[HarmonyPatch(typeof(BUI_BattleInfoCS), "InitBloodBarUI")]
[HarmonyPatchCategory(PatchCategory.Global)]
public class PatchInitBloodBarUI
{
    public static Dictionary<Entity, BUI_ProjWidget> CachedBloodBars;

    public static bool Prefix(BUI_BattleInfoCS __instance, Dictionary<Entity, BUI_ProjWidget> ___EntityDic, Dictionary<AActor, DSBarInfoBind> ___BloodBarActorBindDict, Entity Entity)
    {
        if (CachedBloodBars == null)
        {
            CachedBloodBars = ___EntityDic;
        }

        if (!Core.Config.BloodBarsVisible)
        {
            return false;
        }
        
        if (___EntityDic.ContainsKey(Entity))
            return false;
        var actor = Entity.ToActor();
        var ownerUnit = actor as BGUCharacterCS;
        if (ownerUnit == null)
            return false;
        var unitCommDesc = BGW_GameDB.GetUnitCommDesc(ownerUnit.GetResID());
        if (unitCommDesc == null)
            return false;
        var battleInfoExtendDesc = BGW_GameDB.GetUnitBattleInfoExtendDesc(ownerUnit.GetFinalBattleInfoExtendID());
        if (battleInfoExtendDesc == null)
            return false;

        var maybePlayer = WukongApi.Sync.GetPlayerEntityByActor(actor);
        var isPlayer = maybePlayer.HasValue;
        var bloodBarShowType = isPlayer ? EBGUBloodBarShowType.Always : EBGUBloodBarShowType.Change;

        var isInPlayerTeam = !isPlayer && BGU_DataUtil.GetIsInPlayerTeam(actor);

        if (battleInfoExtendDesc.BloodBarType == EBGUBloodBarType.None || isInPlayerTeam)
            return false;

        var bloodBarPoolWidget = __instance.GetTopBarPoolWidget(ownerUnit, true) as BUI_MBarBase;
        bloodBarPoolWidget?.InitBloodBar(battleInfoExtendDesc.BloodBarType, unitCommDesc.HPBarHeightOffset);

        if (bloodBarPoolWidget != null)
        {
            if (bloodBarShowType == EBGUBloodBarShowType.Always)
            {
                bloodBarPoolWidget.SetAlwaysShowSetting(AlwaysShowSetting.Always, true);
            }

            ___EntityDic.Add(Entity, bloodBarPoolWidget);
        }

        if (!___EntityDic.ContainsKey(Entity) || !___BloodBarActorBindDict.TryGetValue(actor, out var dsBarInfoBind))
            return false;

        dsBarInfoBind.ReInit();
        return false;
    }
}