using ReadyM.Api.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.WukongUtils;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;
using WukongMp.Toolkit.Actors;
using WukongMp.Toolkit.Widgets;

namespace WukongMp.Toolkit.Actors;

public static class ActorsExtensions
{
    public static bool SnapToFloor(this AActor actor, float downwardRange, out FVector newLocation)
    {
        if (downwardRange > 0)
            downwardRange = -downwardRange;


        bool bhit = GameUtils.GetWorld().LineTraceSingle(
            actor.GetActorLocation().Add_VectorVector(new(0, 0, 100)),
            actor.GetActorLocation().Add_VectorVector(new(0, 0, downwardRange)),
            ETraceTypeQuery.TraceTypeQuery1,
            false,
            new List<AActor>() { actor },
            EDrawDebugTrace.None,
            out FHitResult result,
            true,
            FLinearColor.Transparent,
            FLinearColor.Transparent,
            0f
            );

        if (bhit)
        {
            newLocation = new(result.Location.X, result.Location.Y, result.Location.Z);
            actor.SetActorLocation(newLocation, false, out _, true);
        }
        else
        {
            actor.GetActorLocation();
            newLocation = FVector.ZeroVector;
            return false;
        }

        return true;
    }

    public static float RotateByAngle(this AActor actor, float angle)
    {
        var rotation = actor.GetActorRotation();

        rotation.mYaw = (rotation.mYaw + angle) % 360f;

        if (rotation.mYaw < 0)
        {
            rotation.mYaw += 360f;
        }

        actor.SetActorRotation(rotation, false);

        return (float)rotation.mYaw;
    }

    public static UActorComponent? GetWukongComponent(this AActor actor, string componentClassName)
    {
        if (actor == null || actor.Address == IntPtr.Zero)
            return null;

        UClass compClass = UClass.GetClass(componentClassName);
        if (compClass == null || compClass.Address == IntPtr.Zero)
        {
            return null;
        }
        return actor.GetComponentByClass(compClass);
    }
}