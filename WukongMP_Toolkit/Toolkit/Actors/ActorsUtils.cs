using ReadyM.Api.Multiplayer.RPC;
using System;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.Configuration;
using WukongMp.Api.WukongUtils;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace WukongMp.Toolkit.Actors;

public static class Actors
{
    public static AActor? SpawnActor(string className)
    {
        return SpawnActorAtLocation(className, FVector.ZeroVector, FRotator.ZeroRotator);
    }

    public static AActor? SpawnActorAtLocation(string className, FVector location, FRotator rotation)
    {
        try
        {
            UClass? actorClass = UClass.GetClass(className);
            if (actorClass == null)
            {
                actorClass = GetOrLoadClass(className);
            }
            if (actorClass == null)
            {
                return null;
            }
            
            //WukongApi.Chat.SendServerMessage($"Tring to spawn actor at: {location.ToCompactString()}");
            //WukongApi.Chat.SendServerMessage($"Player location: {WukongApi.Sync.LocalMainCharacter.Value.Location.ToFVector().ToCompactString()}");
            return GameUtils.GetWorld().SpawnActor(actorClass, ref location, ref rotation);
        }
        catch (Exception ex)
        {
            Logging.LogException(ex);
            return null;
        }
    }

    public static AActor? SpawnActorAtFloor(string className, FVector location, FRotator rotation)
    {
        var actor = SpawnActorAtLocation(className, location, rotation);
        actor?.SnapToFloor(10000,out _);

        return actor;
    }

    // ClassName or Path
    public static UClass? GetOrLoadClass(string className)
    {
        UClass? actorClass = UClass.GetClass(className); 
        if (actorClass != null) return actorClass;

        Logging.LogWarning($"No actor class {className} in RAM, trying to load from disk");

        try
        {
            actorClass = UClass.LoadClass<AActor>(new ObjectOuter(), className);
            if (actorClass != null) return actorClass;
        }
        catch (Exception ex)
        {
            Logging.LogError($"Failed to load class from: {className}: {ex.Message}");
        }

        return null;
    }
}
