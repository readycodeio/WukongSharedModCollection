using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Client;
using ReadyM.Api.Multiplayer.Generators;
using ReadyM.Api.Multiplayer.Protocol.Enums;
using ReadyM.Api.Multiplayer.RPC;
using ReadyM.Api.Multiplayer.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using WukongMp.Sdk.Api;

namespace WukongMp.PrivateComs;

// define your RPC methods, for example:
public partial class PrivateComsRpc(IRpcClient client, IRelaySerializer serializer) : RpcClassBase(client, serializer)
{
    [RpcEvent(RelayMode.GlobalOthers)] //TODO change to GlobalOthers, for testing its GlobalAll
    private void OnPrivateMessage(PlayerId __sender, string __receiver, string message)
    {
        //Goes along with GlobalAll for testing to self
        //if (__sender == Mod.PlayerId) 
        //    return; //this should never run

        WukongApi.Sync.TryGetPlayerInfoById(__sender, out string? playerName, out _);

        if (playerName is null)
            return;

        if (!__receiver.Equals(Mod.PlayerName, System.StringComparison.OrdinalIgnoreCase))
            return;

        Mod.RecentMessenger = __sender;
        WukongApi.Chat.ShowLocalMessage($"[MSG] {playerName}: {message}", Utils.PrivateMessageColor);
    }

    [RpcEvent(RelayMode.GlobalOthers)]
    private void OnTeamMessage(PlayerId __sender, int teamId, string message)
    {
        //Goes along with GlobalAll for testing to self
        //if (__sender == Mod.PlayerId)
        //    return; //this should never run

        WukongApi.Sync.TryGetPlayerInfoById(__sender, out string? playerName, out _);

        if (playerName is null)
            return;

        if (Mod.PlayerTeam is null || teamId != Mod.PlayerTeam.Value)
            return;

        Mod.RecentTeamMessenger = __sender;
        WukongApi.Chat.ShowLocalMessage($"[TEAM] {playerName}: {message}", Utils.TeamMessageColor);
    }
}
