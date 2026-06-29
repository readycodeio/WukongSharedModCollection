using b1;
using b1.ECS;
using b1.UI.Comm;
using System.Collections.Generic;
using UnrealEngine.Engine;
using WukongMp.Sdk.Api;
using WukongMp.Toolkit.Widgets;

namespace WukongMp.Toolkit.Patches;

//copied 1:1 from working prophunt, not working here lol
//manual patch in place

//[HarmonyPatch(typeof(BUI_BattleInfoCS), "InitBloodBarUI")]
//[HarmonyPatchCategory(PatchCategory.Global)]
public class PatchInitBloodBarUI
{
    public static bool Prefix(BUI_BattleInfoCS __instance, Dictionary<Entity, BUI_ProjWidget> ___EntityDic, Dictionary<AActor, DSBarInfoBind> ___BloodBarActorBindDict, Entity Entity)
    {
        if (Widgets.WidgetsHpBars.HpBars != null)
        {
            if (Widgets.WidgetsHpBars.HpBarsVisible)
                return true;
            else
                return false;
        }

        Widgets.WidgetsHpBars.HpBars = ___EntityDic;
        return true;
    }
    public static void Postfix(Dictionary<Entity, BUI_ProjWidget> ___EntityDic, Entity Entity)
    {
        if (!___EntityDic.TryGetValue(Entity, out var widget)) return;

        var actor = Entity.ToActor();
        if (actor == null) return;

        var maybePlayer = WukongApi.Sync.GetPlayerEntityByActor(actor);
        if (maybePlayer.HasValue)
        {
            var playerId = maybePlayer.Value.PlayerId;
            WidgetsHpBars.PlayerHpBars[playerId] = widget;
        }
    }
}