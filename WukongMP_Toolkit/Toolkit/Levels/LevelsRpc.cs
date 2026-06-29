using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Client;
using ReadyM.Api.Multiplayer.Generators;
using ReadyM.Api.Multiplayer.Protocol.Enums;
using ReadyM.Api.Multiplayer.RPC;
using ReadyM.Api.Multiplayer.Serialization;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Toolkit.Levels;

namespace WukongMp.Toolkit.Levels;

public partial class LevelsRpc(IRpcClient client, IRelaySerializer serializer) : RpcClassBase(client, serializer)
{
    [RpcEvent(RelayMode.GlobalAll)]
    internal void OnLevelLoadRequest(string levelName, bool inFreezeOnLoad, string targetActorTag)
    {
        RunOnMainThread(() =>
        {
            LevelsCore.LoadLevel(
                levelName: levelName,
                unloadOther: true,
                isKeyWord: true,
                freezeOnLoad: inFreezeOnLoad,
                targetActorTag: targetActorTag
            );
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    internal void OnLevelUnloadRequest(string? levelName)
    {
        RunOnMainThread(() =>
        {
            if (levelName != null)
                LevelsCore.UnloadLevel(levelName);
            else
                LevelsCore.UnloadLevels(LevelsCore.GetLoadedLevels());
        });
    }
}