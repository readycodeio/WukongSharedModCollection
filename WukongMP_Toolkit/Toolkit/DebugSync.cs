using ReadyM.Api.Command;
using UnrealEngine.Engine;
using WukongMp.Api;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;
using WukongMp.Toolkit.Actors;
using WukongMp.Toolkit.Audio;

namespace WukongMp.Toolkit.Debug.Sync;

public static class DebugSync
{
    public static void SyncTest()
    {
        WukongApi.Console.AddCommand("sync_spawn", ConsoleCommand.Create(() => 
        {
            var location = WukongApi.Sync.LocalMainCharacter.Value.Location.ToFVector();
            location.Add_VectorVector(new UnrealEngine.Runtime.FVector(300, 0, 0));

            ActorsSync.SyncSpawnActorAtLocation(Actors.Assets.Destructible.HFS_Destructible_DengKan, location, WukongApi.Sync.LocalMainCharacter.Value.Rotation.ToFRotator());
        }));

        WukongApi.Console.AddCommand("sync_audio_player", ConsoleCommand.Create(() =>
        {
            //AkAudioEvent'/Game/00Main/Audio/SFX/Environment/BPO/ENV_Position_BPO_Stick.ENV_Position_BPO_Stick'
            AudioCore.RegisterSound("Stick", "AkAudioEvent'/Game/00Main/Audio/SFX/Environment/BPO/ENV_Position_BPO_Stick.ENV_Position_BPO_Stick'");
            //AudioCore.PlaySound("Stick", location: WukongApi.Sync.LocalMainCharacter.Value.Location.ToFVector());
            var param = new SoundParamsFlat();
            param.SoundSource = "Stick";

            Mod.audioRpc?.SendSyncSoundAtPlayer(WukongApi.Sync.LocalPlayerId.Value, param);
        }));
        WukongApi.Console.AddCommand("sync_audio_location", ConsoleCommand.Create((float offsetX, float offsetY = 0, float offsetZ = 0, float rX = 0, float rY = 0, float rZ = 0) =>
        {
            //AkAudioEvent'/Game/00Main/Audio/SFX/Environment/BPO/ENV_Position_BPO_Stick.ENV_Position_BPO_Stick'
            AudioCore.RegisterSound("Stick", "AkAudioEvent'/Game/00Main/Audio/SFX/Environment/BPO/ENV_Position_BPO_Stick.ENV_Position_BPO_Stick'");
            //AudioCore.PlaySound("Stick", location: WukongApi.Sync.LocalMainCharacter.Value.Location.ToFVector());
            var param = new SoundParamsSpatial();
            param.SoundSource = "Stick";
            param.FromFVector(WukongApi.Sync.LocalMainCharacter.Value.Location.ToFVector().Add_VectorVector(new UnrealEngine.Runtime.FVector(offsetX, offsetY, offsetZ)));
            param.FromFRotator(WukongApi.Sync.LocalMainCharacter.Value.Rotation.ToFRotator());
            param.Yaw += rZ;

            Mod.audioRpc?.SendSyncSoundAtLocation(param);
        }));
    }
}
