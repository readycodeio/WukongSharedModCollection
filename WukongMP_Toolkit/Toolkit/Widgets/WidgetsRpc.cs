using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Client;
using ReadyM.Api.Multiplayer.Generators;
using ReadyM.Api.Multiplayer.Protocol.Enums;
using ReadyM.Api.Multiplayer.RPC;
using ReadyM.Api.Multiplayer.Serialization;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Toolkit.Levels;
using WukongMp.Toolkit.Widgets;

namespace WukongMp.Toolkit.Levels;

public partial class WidgetsRpc(IRpcClient client, IRelaySerializer serializer) : RpcClassBase(client, serializer)
{
    [RpcEvent(RelayMode.GlobalAll)]
    private void OnHpBarSync(PlayerId __sender, bool showHpBar)
    {
        if (showHpBar)
        {
            WidgetsHpBars.SetHpBarsState(UnrealEngine.UMG.ESlateVisibility.Visible);
        }
        else
        {
            WidgetsHpBars.SetHpBarsState(UnrealEngine.UMG.ESlateVisibility.Hidden);
        }
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnPlayerHpBarSync(PlayerId __sender, bool showHpBar)
    {
        foreach (var bar in WidgetsHpBars.PlayerHpBars)
        {
            if (showHpBar)
            {
                bar.Value?.SetVisibility(UnrealEngine.UMG.ESlateVisibility.Visible);
            }
            else
            {
                bar.Value?.SetVisibility(UnrealEngine.UMG.ESlateVisibility.Hidden);
            }
        }
        
    }
}