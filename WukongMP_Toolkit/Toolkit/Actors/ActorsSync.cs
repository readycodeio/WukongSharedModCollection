using ReadyM.Api.Idents;
using System;
using System.Collections.Generic;
using System.Text;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.WukongUtils;
using WukongMp.Sdk.Api;
using WukongMp.Toolkit.Audio;

namespace WukongMp.Toolkit.Actors;

public static class ActorsSync
{
    public static Dictionary<PlayerId, Dictionary<int, AActor>> ActorDict = new();
    private static int localActorId = 0;

    public static AActor? SyncSpawnActor(string className)
    {
        var actor = SyncSpawnActorAtLocation(className, FVector.ZeroVector, FRotator.ZeroRotator);

        return actor;
    }

    public static AActor? SyncSpawnActorAtLocation(string className, FVector location, FRotator rotation)
    {
        int netId = ++localActorId;
        PlayerId myId = WukongApi.Sync.LocalPlayerId.Value;

        var actor = Actors.SpawnActorAtLocation(className, location, rotation);
        if (actor == null) return null;

        if (!ActorDict.ContainsKey(myId))
        {
            ActorDict[myId] = new Dictionary<int, AActor>();
        }
        ActorDict[myId][netId] = actor;

        ActorSpawnParams spawnParams = new ActorSpawnParams(className, netId, location, rotation);

        Mod.actorsRpc?.SendActorSpawnAtLocation(spawnParams);

        return actor;
    }
}

