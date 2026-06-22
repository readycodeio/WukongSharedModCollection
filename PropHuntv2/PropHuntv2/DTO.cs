using LiteNetLib.Utils;
using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Generators;

namespace WukongMp.PropHunt;

[DeriveINetSerializable]
public partial struct GameConfig : INetSerializable
{
    public int MaxHiderHp;
    public int MaxSeekerHp;

    public int MaxHunterCount;
    public int MinHunterCount;

    public int MaxHiderDecoys;
    public int MaxHiderPermaDecoys;

    public float PreparationTime;
    public float GameTime;

    public bool CustomTeams;
    public bool DestroyTamers;
    public bool BloodBarsVisible;


    public GameConfig()
    {
        this.MaxHiderHp = 100;
        this.MaxSeekerHp = 100;

        this.MaxHunterCount = 2;
        this.MinHunterCount = 1;

        this.MaxHiderDecoys = 3;
        this.MaxHiderPermaDecoys = 1;
       
        this.PreparationTime = 5f;
        
        this.GameTime = 70f;
        
        this.CustomTeams = false;
        this.DestroyTamers = true;
        this.BloodBarsVisible = false;
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

    public PlayerPropNetData[] ActiveProps;
    public DecoyNetData[] ActiveDecoys;

    public GameStateSnapshot()
    {
        this.IsGameActive = false;
        this.HasPreparationEnded = false;
        this.ElapsedRoundTime = 0f;
        this.CurrentRound = 0;
        this.HidersScore = 0;
        this.SeekersScore = 0;

        this.Seekers = "";
        this.Hiders = "";
        this.Spectators = "";

        this.ActiveProps = [];
        this.ActiveDecoys = [];
    }
}

[DeriveINetSerializable]
public partial struct DecoyNetData : INetSerializable
{
    public PlayerId OwnerId;
    public string PropName;
    public float X, Y, Z;
    public float Pitch, Yaw, Roll;
}

[DeriveINetSerializable]
public partial struct PlayerPropNetData : INetSerializable
{
    public PlayerId OwnerId;
    public string PropName;
}