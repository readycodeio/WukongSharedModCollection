using b1;
using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Client;
using ReadyM.Api.Multiplayer.Generators;
using ReadyM.Api.Multiplayer.Protocol.Enums;
using ReadyM.Api.Multiplayer.RPC;
using ReadyM.Api.Multiplayer.Serialization;
using UnrealEngine.Runtime;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace WukongMp.Toolkit.Audio;


public partial class AudioRpc(IRpcClient client, IRelaySerializer serializer) : RpcClassBase(client, serializer)
{
    [RpcEvent(RelayMode.GlobalAll)]
    private void OnSyncSoundAtLocation(PlayerId __sender, SoundParamsSpatial parameters)
    {
        RunOnMainThread(() =>
        {
            FVector loc = new FVector(parameters.X, parameters.Y, parameters.Z);
            FRotator rot = new FRotator(parameters.Pitch, parameters.Yaw, parameters.Roll);

            AudioCore.PlaySound(parameters.SoundSource, location: loc, rotation: rot);
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnSyncSoundAtPlayer(PlayerId __sender, PlayerId targetPlayer, SoundParamsFlat parameters)
    {
        RunOnMainThread(() =>
        {
            var character = WukongApi.Sync.GetMainCharacterByPlayerId(targetPlayer);
            if (character.HasValue && character.Value.Pawn != null)
            {
                AudioCore.PlaySound(parameters.SoundSource, targetPlayer: targetPlayer);
            }
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnSyncSoundFlat(PlayerId __sender, SoundParamsFlat parameters)
    {
        RunOnMainThread(() =>
        {
            AudioCore.PlaySound(parameters.SoundSource);
        });
    }
}
