using LiteNetLib.Utils;
using ReadyM.Api.Multiplayer.Generators;
using System;
using System.Collections.Generic;
using System.Text;

namespace WukongMp.PropHunt;

[DeriveINetSerializable]
public partial struct GameConfig : INetSerializable
{
    public int MaxHiderHp;
    public int MaxSeekerHp;
    public int MaxHunter;
    public int MinHunter;
    public float PreparationTime;
    public float GameTime;
    public bool CustomTeams;

    public GameConfig()
    {
        MaxHiderHp = 100;
        MaxSeekerHp = 100;
        MaxHunter = 2;
        MinHunter = 1;
        PreparationTime = 5f;
        GameTime = 70f;
        CustomTeams = false;
    }

    public static GameConfig Default => new GameConfig();
}

[DeriveINetSerializable]
public partial struct GameStateSnapshot : INetSerializable
{
    public bool IsGameActive;
    public bool HasPreparationEnded;
    public float ElapsedRoundTime;
    public int CurrentRound;
    public int HidersScore;
    public int SeekersScore;

    public string Seekers;
    public string Hiders;
    public string Spectators;

    public GameStateSnapshot()
    {
        IsGameActive = false;
        HasPreparationEnded = false;
        ElapsedRoundTime = 0f;
        CurrentRound = 0;
        HidersScore = 0;
        SeekersScore = 0;

        Seekers = "";
        Hiders= "";
        Spectators = "";
    }
}