using b1;
using System;
using System.Collections.Generic;
using System.Linq;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.WukongUtils;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace WukongMp.Toolkit.Levels;

public static class LevelsCore
{
    private class AsyncLoadData
    {
        public byte FramesLeft;
        public string TargetActorTag;
        public UClass? TargetActorClass;
        public FVector FallbackPosition;
    }

    private static readonly Dictionary<string, AsyncLoadData> PendingAsyncLevels = new();
    private static readonly byte EventFrameDelay = 3;

    public static void TeleportToShrine(int shrineId)
    {
        try
        {
            var world = GameUtils.GetWorld();
            var events = BPS_EventCollectionCS.GetLocal(world);

            if (events == null)
            {
                return;
            }

            var teleportData = new TeleportParam_RebirthPoint { RebirthPointId = shrineId };

            events.Evt_BPS_TeleportTo.Invoke(
                ETeleportTypeV2.RebirthPointTeleportOnly,
                teleportData,
                EPlayerTeleportReason.RebirthPoint
            );
        }
        catch (Exception ex)
        {
            Logging.LogException(ex);
        }
    }

    public static void LoadLevelWithDiagnostics(string keyword)
    {
        try
        {
            UObject worldContext = GameUtils.GetWorld();

            var actorsBefore = UGameplayStatics.GetAllActorsOfClass<AActor>(worldContext);
            int countBefore = actorsBefore?.Count() ?? 0;

            var failedList = UBGUWCStreamingFuncLib.SetLevelsState(
                worldContext, keyword, EGSLevelState.LoadedVisible, -1, true, true
            );

            if (failedList != null && failedList.Count > 0)
            {
                Logging.LogError($"Map not found or failed to load: {keyword}");
                return;
            }

            var actorsAfter = UGameplayStatics.GetAllActorsOfClass<AActor>(worldContext);
            int countAfter = actorsAfter?.Count() ?? 0;
            int diff = countAfter - countBefore;

            Logging.LogInformation($"Loaded: {keyword}. Actors Before: {countBefore} | After: {countAfter} | Diff: +{diff}");

            //if (diff > 0 && actorsAfter != null && actorsBefore != null)
            //{
            //    var newActor = actorsAfter.FirstOrDefault(a => a.Address != IntPtr.Zero && !actorsBefore.Contains(a));
            //    if (newActor != null)
            //    {
            //        GameUtils.GetControlledPawn().SetActorLocation(newActor.GetActorLocation(), false, out _, true);
            //    }
            //}
        }
        catch (Exception ex)
        {
            Logging.LogError(ex.Message);
        }
    }

    public static void LoadLevel(
        string levelName,
        bool unloadOther = false,
        bool isKeyWord = false,
        bool freezeOnLoad = true,
        string targetActorTag = "",
        UClass? targetActorClass = null,
        FVector? fallbackPos = null
    )
    {
        UObject world = GameUtils.GetWorld();
        if (world == null) return;

        List<string> intendedLevels = isKeyWord
            ? (UBGUWCStreamingFuncLib.GetLevelNamesByKeyword(world, levelName) ?? [])
            : [levelName];

        if (intendedLevels.Count == 0) return;

        string levelToLoad = intendedLevels[0];
        FVector safeFallback = fallbackPos ?? new FVector(0, 0, 500f);

        if (unloadOther) UnloadLevels(GetLoadedLevels());

        var failedList = UBGUWCStreamingFuncLib.SetLevelsState(world, levelToLoad, EGSLevelState.LoadedVisible, -1, bKeywordMatch: false, freezeOnLoad);

        if (failedList != null && failedList.Count > 0) return;

        if (freezeOnLoad)
        {
            LevelEvents.InvokeOnLevelLoaded(levelToLoad);
            ExecutePostLoadTeleport(targetActorTag, targetActorClass, safeFallback);
        }
        else
        {
            lock (PendingAsyncLevels)
            {
                PendingAsyncLevels[levelToLoad] = new AsyncLoadData
                {
                    FramesLeft = EventFrameDelay,
                    TargetActorTag = targetActorTag,
                    FallbackPosition = safeFallback
                };
            }
        }
    }

    private static void ExecutePostLoadTeleport(string targetTag, UClass? targetClass, FVector fallbackPosition)
    {
        var localPlayer = WukongApi.Sync.LocalMainCharacter;
        if (!localPlayer.HasValue || localPlayer.Value.Pawn == null) return;

        FVector finalPosition = fallbackPosition;
        bool foundTarget = false;

        bool hasTag = !string.IsNullOrEmpty(targetTag);
        bool hasClass = targetClass != null;

        if (hasTag || hasClass)
        {
            UClass searchClass = targetClass ?? UClass.GetClass("Actor");
            var targetActors = UGameplayStatics.GetAllActorsOfClass(GameUtils.GetWorld(), searchClass);

            if (targetActors != null)
            {
                foreach (var actor in targetActors)
                {
                    if (actor.Address == IntPtr.Zero) continue;

                    if (!hasTag || actor.ActorHasTag(new FName(targetTag)))
                    {
                        finalPosition = actor.GetActorLocation();
                        foundTarget = true;
                        break;
                    }
                }
            }
        }

        if (!foundTarget)
        {
            var startActors = UGameplayStatics.GetAllActorsOfClass(GameUtils.GetWorld(), UClass.GetClass("PlayerStart"));
            if (startActors != null && startActors.Any() && startActors.First().Address != IntPtr.Zero)
            {
                finalPosition = startActors.First().GetActorLocation();
            }
        }

        localPlayer.Value.Pawn.SetActorLocation(finalPosition, false, out _, true);
    }

    internal static void Update(float deltaTime)
    {
        if (PendingAsyncLevels.Count == 0) return;

        var visibleLevels = GetVisibleLevels();

        lock (PendingAsyncLevels)
        {
            List<string> readyLevels = [];

            foreach (var lvl in PendingAsyncLevels.Keys.ToList())
            {
                if (visibleLevels.Contains(lvl))
                {
                    PendingAsyncLevels[lvl].FramesLeft--;
                    if (PendingAsyncLevels[lvl].FramesLeft <= 0)
                    {
                        ExecutePostLoadTeleport(PendingAsyncLevels[lvl].TargetActorTag, PendingAsyncLevels[lvl].TargetActorClass, PendingAsyncLevels[lvl].FallbackPosition);

                        PendingAsyncLevels.Remove(lvl);
                        readyLevels.Add(lvl);
                    }
                }
            }

            foreach (var lvl in readyLevels) LevelEvents.InvokeOnLevelLoaded(lvl);
        }
    }
    private static List<string> GetLevelsByState(EGSLevelState targetState)
    {
        UObject world = GameUtils.GetWorld();
        if (world == null) return [];

        UBGUWCStreamingFuncLib.GetAllLevelCurrentState(world, out var levelStates, true);
        if (levelStates == null) return [];

        List<string> list = [];
        foreach (var levelState in levelStates)
        {
            if (levelState.Value.Equals((byte)targetState))
            {
                list.Add(levelState.Key);
            }
        }
        return list;
    }
    public static List<string> GetVisibleLevels()
    {
        return GetLevelsByState(EGSLevelState.LoadedVisible);
    }

    public static List<string> GetLoadedLevels()
    {
        UObject world = GameUtils.GetWorld();
        if (world == null) return [];

        UBGUWCStreamingFuncLib.GetAllLevelCurrentState(world, out var levelStates, true);
        if (levelStates == null) return [];

        List<string> list = [];
        foreach (var levelState in levelStates)
        {
            byte state = levelState.Value;
            if (state == (byte)EGSLevelState.LoadedVisible || state == (byte)EGSLevelState.LoadedInvisible)
            {
                list.Add(levelState.Key);
            }
        }
        return list;
    }
    public static void UnloadLevels(List<string>? levelsToUnload)
    {
        if (levelsToUnload == null || levelsToUnload.Count <= 0) return;

        foreach (var lvl in levelsToUnload)
        {
            UnloadLevel(lvl);
        }
    }

    public static void UnloadAllLevelsExcept(List<string>? exceptedLevels)
    {
        if (exceptedLevels == null || exceptedLevels.Count <= 0)
        {
            Logging.LogError($"Request to unload all levels with no exception given has been terminated");
            return;
        }
        var loadedLevels = GetLoadedLevels();

        foreach (var level in loadedLevels)
        {
            if (exceptedLevels.Contains(level))
                continue;
            UnloadLevel(level);
        }
    }
    public static void UnloadLevel(string? levelToUnload)
    {
        if (string.IsNullOrEmpty(levelToUnload)) return;

        UObject? world = GameUtils.GetWorld();
        if (world == null) return;

        UBGUWCStreamingFuncLib.SetLevelsState(world, levelToUnload, EGSLevelState.Unloaded, -1, bKeywordMatch: false, bBlockOnLoad: false);

        Logging.LogInformation($"Unloaded level: {levelToUnload}");
        LevelEvents.InvokeLevelUnloaded(levelToUnload!);
    }
}