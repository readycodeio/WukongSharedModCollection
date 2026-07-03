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

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(IsGameActive);
        writer.Put(HasPreparationEnded);
        writer.Put(ElapsedRoundTime);
        writer.Put(CurrentRound);
        writer.Put(HidersScore);
        writer.Put(SeekersScore);

        writer.Put(Seekers ?? string.Empty);
        writer.Put(Hiders ?? string.Empty);
        writer.Put(Spectators ?? string.Empty);

        writer.Put((ushort)(ActiveProps?.Length ?? 0));
        if (ActiveProps != null)
        {
            for (int i = 0; i < ActiveProps.Length; i++)
            {
                ActiveProps[i].Serialize(writer);
            }
        }

        writer.Put((ushort)(ActiveDecoys?.Length ?? 0));
        if (ActiveDecoys != null)
        {
            for (int i = 0; i < ActiveDecoys.Length; i++)
            {
                ActiveDecoys[i].Serialize(writer);
            }
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        IsGameActive = reader.GetBool();
        HasPreparationEnded = reader.GetBool();
        ElapsedRoundTime = reader.GetFloat();
        CurrentRound = reader.GetInt();
        HidersScore = reader.GetInt();
        SeekersScore = reader.GetInt();

        Seekers = reader.GetString();
        Hiders = reader.GetString();
        Spectators = reader.GetString();

        ushort propsCount = reader.GetUShort();
        ActiveProps = new PlayerPropNetData[propsCount];
        for (int i = 0; i < propsCount; i++)
        {
            ActiveProps[i] = new PlayerPropNetData();
            ActiveProps[i].Deserialize(reader);
        }

        ushort decoysCount = reader.GetUShort();
        ActiveDecoys = new DecoyNetData[decoysCount];
        for (int i = 0; i < decoysCount; i++)
        {
            ActiveDecoys[i] = new DecoyNetData();
            ActiveDecoys[i].Deserialize(reader);
        }
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