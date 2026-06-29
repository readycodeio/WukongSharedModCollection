using LiteNetLib.Utils;
using ReadyM.Api.Multiplayer.Generators;
using System;
using System.Collections.Generic;
using System.Text;

namespace WukongMp.Toolkit.Levels;

[DeriveINetSerializable]
public partial struct Position : INetSerializable
{
    public float X;
    public float Y;
    public float Z;

    public Position(float x, float y, float z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    public static Position ZeroPosition => new Position(0.0f, 0.0f, 0.0f);
}
