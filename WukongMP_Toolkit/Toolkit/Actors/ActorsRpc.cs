using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Client;
using ReadyM.Api.Multiplayer.Generators;
using ReadyM.Api.Multiplayer.Protocol.Enums;
using ReadyM.Api.Multiplayer.RPC;
using ReadyM.Api.Multiplayer.Serialization;
using System;
using System.Collections.Generic;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace WukongMp.Toolkit.Actors;

public partial class ActorsRpc(IRpcClient client, IRelaySerializer serializer) : RpcClassBase(client, serializer)
{
    [RpcEvent(RelayMode.GlobalAll)]
    private void OnActorSpawn(PlayerId __sender, ActorSpawnParams spawnParams)
    {
        RunOnMainThread(() =>
        {
            var actor = Actors.SpawnActor(spawnParams.ActorClass);
            if (actor == null) return;

            if (!ActorsSync.ActorDict.ContainsKey(__sender))
            {
                ActorsSync.ActorDict[__sender] = new Dictionary<int, AActor>();
            }
            ActorsSync.ActorDict[__sender][spawnParams.ActorId] = actor;
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnActorDespawn(PlayerId __sender, int index)
    {
        RunOnMainThread(() => 
        { 
            if (ActorsSync.ActorDict.TryGetValue(__sender, out var playerActors))
            {
                if (playerActors.TryGetValue(index, out var actor))
                {
                    actor.DestroyActor();
                    playerActors.Remove(index);
                }
            }
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnActorSpawnAtLocation(PlayerId __sender, ActorSpawnParams spawnParams)
    {
        RunOnMainThread(() =>
        {
            var actor = Actors.SpawnActorAtLocation(spawnParams.ActorClass, spawnParams.ToFVector(), spawnParams.ToFRotator());
            if (actor == null) return;

            if (!ActorsSync.ActorDict.ContainsKey(__sender))
            {
                ActorsSync.ActorDict[__sender] = new Dictionary<int, AActor>();
            }
            ActorsSync.ActorDict[__sender][spawnParams.ActorId] = actor;
        });
    }

    [RpcEvent(RelayMode.GlobalOthers)]
    private void OnActorDictSyncRequest(PlayerId __sender)
    {
        RunOnMainThread(() =>
        {
            if (__sender == WukongApi.Sync.LocalPlayerId) return;

            PlayerId myId = WukongApi.Sync.LocalPlayerId.Value;

            if (!ActorsSync.ActorDict.TryGetValue(myId, out var myActors)) return;

            foreach (var record in myActors)
            {
                int netId = record.Key;
                AActor actor = record.Value;

                if (actor == null || actor.Address == IntPtr.Zero) continue;

                var location = actor.GetActorLocation();
                var rotation = actor.GetActorRotation();
                string className = actor.GetClass().GetName();

                var spawnParams = new ActorSpawnParams(className, netId, location, rotation);
                SendActorDictSyncResponse(__sender, spawnParams);
            }
        });
    }

    [RpcEvent(RelayMode.GlobalOthers)]
    private void OnActorDictSyncResponse(PlayerId __sender, PlayerId targetPlayer, ActorSpawnParams spawnParams)
    {
        RunOnMainThread(() =>
        {
            if (targetPlayer != WukongApi.Sync.LocalPlayerId.Value) return;
            
            if (!ActorsSync.ActorDict.ContainsKey(__sender)) return;
            if (ActorsSync.ActorDict[__sender].ContainsKey(spawnParams.ActorId)) return;

            var actor = Actors.SpawnActorAtLocation(spawnParams.ActorClass, spawnParams.ToFVector(), spawnParams.ToFRotator());
            if (actor == null) return;

            ActorsSync.ActorDict[__sender][spawnParams.ActorId] = actor;

        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnActorAttachToPlayer(PlayerId __sender, ActorAttachmentSyncParams attachParams)
    {
        RunOnMainThread(() =>
        {
            if (!ActorsSync.ActorDict.TryGetValue(attachParams.Identification.OwnerId, out var playerActors)) return;
            if (!playerActors.TryGetValue(attachParams.Identification.ActorId, out var actor)) return;

            var player = WukongApi.Sync.GetMainCharacterByPlayerId(attachParams.Identification.OwnerId);
            if (player == null) return;

            actor.AttachToActor(player.Value.Pawn, new FName(""), attachParams.LocationAttachmentRule, attachParams.RotationAttachmentRule, attachParams.ScaleAttachmentRule, false);
        });
    }
}