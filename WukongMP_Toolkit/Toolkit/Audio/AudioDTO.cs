using LiteNetLib.Utils;
using ReadyM.Api.Multiplayer.Generators;

namespace WukongMp.Toolkit.Audio;


[DeriveINetSerializable]
public partial struct SoundParamsFlat : INetSerializable
{
    public string SoundSource;
    public bool IsLoop;
}

[DeriveINetSerializable]
public partial struct SoundParamsSpatial : INetSerializable
{
    public string SoundSource;
    public bool IsLoop;
    public float X, Y, Z;
    public float Pitch, Yaw, Roll;    
}

