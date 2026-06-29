using b1;
using b1.ECS;
using b1.UI.Comm;
using ReadyM.Api.Idents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnrealEngine.Runtime;
using WukongMp.Sdk.Api;

namespace WukongMp.Toolkit.Widgets;

public static class WidgetsHpBars
{
    public static Dictionary<Entity, BUI_ProjWidget>? HpBars = null;
    public static Dictionary<PlayerId, BUI_ProjWidget> PlayerHpBars = new();
    public static bool HpBarsVisible { get; private set; } = true;

    public static bool HideHpBars()
    {
        SetHpBarsState(UnrealEngine.UMG.ESlateVisibility.Hidden);
        HpBarsVisible = false;
        return true;
    }
    
    public static bool ShowHpBars()
    {
        SetHpBarsState(UnrealEngine.UMG.ESlateVisibility.Visible);
        HpBarsVisible = true;
        return true;
    }

    public static void SetHpBarsState(UnrealEngine.UMG.ESlateVisibility state)
    {
        if (HpBars == null) return;

        foreach (var bar in HpBars)
        {
            bar.Value.SetVisibility(state);
        }
    }

    public static bool ToggleHpBars()
    {
        if (HpBarsVisible) SetHpBarsState(UnrealEngine.UMG.ESlateVisibility.Hidden);
        else SetHpBarsState(UnrealEngine.UMG.ESlateVisibility.Visible);

        HpBarsVisible = !HpBarsVisible;
        return HpBarsVisible;
    }
}