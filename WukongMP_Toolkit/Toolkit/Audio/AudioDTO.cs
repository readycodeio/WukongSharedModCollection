using LiteNetLib.Utils;
using ReadyM.Api.Multiplayer.Generators;
using UnrealEngine.Runtime;

namespace WukongMp.Toolkit.Audio;


[DeriveINetSerializable]
public partial struct SoundParamsFlat : INetSerializable
{
    public string SoundSource;
    public bool IsLoop;

    public SoundParamsFlat()
    {
        this.SoundSource = string.Empty;
        this.IsLoop = false;
    }

    public SoundParamsFlat(string SoundSource, bool IsLoop = false)
    {
        this.SoundSource = SoundSource;
        this.IsLoop = IsLoop;
    }
}

[DeriveINetSerializable]
public partial struct SoundParamsSpatial : INetSerializable
{
    public string SoundSource;
    public bool IsLoop;
    public float X, Y, Z;
    public float Pitch, Yaw, Roll;

    public SoundParamsSpatial()
    {
        this.SoundSource = string.Empty;
        this.IsLoop = false;
        this.X = 0; this.Y = 0; this.Z = 0;
        this.Pitch = 0; this.Yaw = 0; this.Roll = 0;
    }

    public SoundParamsSpatial(string SoundSource, bool IsLoop = false)
    {
        this.SoundSource = SoundSource;
        this.IsLoop = IsLoop;
        this.X = 0; this.Y = 0; this.Z = 0;
        this.Pitch = 0; this.Yaw = 0; this.Roll = 0;
    }
    
    public void FromFVector(FVector vec)
    {
        this.X = vec.X; 
        this.Y = vec.Y; 
        this.Z = vec.Z;
    }

    public void FromFRotator(FRotator r) 
    { 
        this.Pitch = r.Pitch;
        this.Yaw = r.Yaw;
        this.Roll = r.Roll;
    }
}

