using LiteNetLib.Utils;
using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Generators;
using System;
using System.Collections.Generic;
using System.Text;
using UnrealEngine.AnimGraphRuntime;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;

namespace WukongMp. Toolkit.Actors;

[DeriveINetSerializable]
public partial struct ActorSpawnParams : INetSerializable
{
    public string ActorClass;
    public int ActorId;
    public float X, Y, Z;
    public float Pitch, Yaw, Roll;

    public ActorSpawnParams(string actorClass, int actorId, float x, float y, float z, float pitch, float yaw, float roll)
    {
        this.ActorClass = actorClass;
        this.ActorId = actorId;

        this.X = x; 
        this.Y = y; 
        this.Z = z;
        
        this.Pitch = pitch; 
        this.Yaw = yaw; 
        this.Roll = roll;
    }

    public ActorSpawnParams(string actorClass, int actorId, FVector location, FRotator? rotation = null)
    {
        this.ActorClass = actorClass;
        this.ActorId = actorId;

        this.X = location.X;
        this.Y = location.Y;
        this.Z = location.Z;

        this.Yaw = rotation?.Yaw ?? 0;
        this.Yaw = rotation?.Yaw ?? 0;
        this.Roll = rotation?.Roll ?? 0;
    }

    public FVector ToFVector()
    {
        FVector vec = new FVector();
        vec.X = X;
        vec.Y = Y;
        vec.Z = Z;

        return vec;
    }

    public void FromFVector(FVector location)
    {
        this.X = location.X;
        this.Y = location.Y;
        this.Z = location.Z;
    }

    public FRotator ToFRotator()
    {
        FRotator rot = new FRotator();
        rot.Pitch = this.Pitch;
        rot.Yaw = this.Yaw;
        rot.Roll = this.Roll;

        return rot;
    }

    public void FromFRotator(FRotator rotation)
    {
        this.Pitch = rotation.Pitch;
        this.Yaw = rotation.Yaw;
        this.Roll = rotation.Roll;

        var actor = new AActor();
    }
}

[DeriveINetSerializable]
public partial struct ActorIdParams : INetSerializable
{
    public PlayerId OwnerId;
    public int ActorId;

    public ActorIdParams(PlayerId ownerId, int actorId)
    {
        this.OwnerId = ownerId;
        this.ActorId = actorId;
    }
}

//[DeriveINetSerializable]
//public partial struct ActorSyncParams : INetSerializable
//{
//    ActorIdParams Identification;
//    public FVector Location;
//    public FRotator Rotation;
//    public FVector Scale;
//    public bool IsHidden;
//    public bool IsInteractionEnabled;
    
//    public ActorSyncParams()
//    {
//        Location = new FVector();
//        Rotation = new FRotator();
//        Scale = new FVector();
//        IsHidden = false;
//        IsInteractionEnabled = true;
//    }

//    public ActorSyncParams(PlayerId ownerId, int actorId, FVector location, FRotator rotation, FVector scale, bool isHidden, bool isInteractionEnabled)
//    {
//        Identification.OwnerId = ownerId;
//        Identification.ActorId = actorId;
//        Location = location;
//        Rotation = rotation;
//        Scale = scale;
//        IsHidden = isHidden;
//        IsInteractionEnabled = isInteractionEnabled;
//    }

//    public ActorSyncParams WithLocation(FVector location)
//    {
//        this.Location = location;
//        return this;
//    }
//    public ActorSyncParams WithRotation(FRotator rotation)
//    {
//        this.Rotation = rotation;
//        return this;
//    }
//    public ActorSyncParams WithScale(FVector scale)
//    {
//        this.Scale = scale;
//        return this;
//    }
//    public ActorSyncParams WithIsHidden(bool isHidden)
//    {
//        this.IsHidden = isHidden;
//        return this;
//    }
//    public ActorSyncParams WithInteraction(bool interaction)
//    {
//        this.IsInteractionEnabled = interaction;
//        return this;
//    }
//}

[DeriveINetSerializable]
public partial struct ActorAttachmentSyncParams : INetSerializable
{
    public ActorIdParams Identification;
    public EAttachmentRule LocationAttachmentRule;
    public EAttachmentRule RotationAttachmentRule;
    public EAttachmentRule ScaleAttachmentRule;

    public ActorAttachmentSyncParams(ActorIdParams identification, EAttachmentRule locationRule, EAttachmentRule rotationRule, EAttachmentRule scaleRule)
    {
        this.Identification = identification;
        this.LocationAttachmentRule = locationRule;
        this.RotationAttachmentRule = rotationRule;
        this.ScaleAttachmentRule = scaleRule;
    }
    public ActorAttachmentSyncParams(ActorIdParams identification, EAttachmentRule rule)
    {
        this.Identification = identification;
        this.LocationAttachmentRule = rule;
        this.RotationAttachmentRule = rule;
        this.ScaleAttachmentRule = rule;
    }
}
