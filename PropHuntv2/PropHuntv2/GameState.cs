using ReadyM.Api.Idents;
using System.Collections.Generic;
using System.Linq;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace WukongMp.PropHunt;

public static class GameState
{
    public static bool IsGameActive { get; set; } = false;
    public static bool HasPreparationEnded { get; set; } = false;
    public static bool HasPlayerJoinedLate { get; set; } = false;
    public static float ElapsedRoundTime { get; set; } = 0f;
    public static int CurrentRound { get; set; } = 0;

    public static int HidersScore { get; set; } = 0;
    public static int SeekersScore { get; set; } = 0;

    public static HashSet<PlayerId> Seekers { get; set; } = [];
    public static HashSet<PlayerId> Hiders { get; set; } = [];
    public static HashSet<PlayerId> Spectators { get; set; } = [];

    public static Dictionary<PlayerId, AActor> PlayerProps { get; set; } = [];
    public static Dictionary<PlayerId, ReadyMainCharacter> PlayerCharacters { get; set; } = [];
    public static Dictionary<PlayerId, AActor> PlayerActors { get; set; } = [];

    public static FVector OriginalSeekerLocation = FVector.ZeroVector;


    public static void Reset()
    {
        IsGameActive = false;
        HasPreparationEnded = false;
        ElapsedRoundTime = 0f;

        Seekers.Clear();
        Hiders.Clear();
        Spectators.Clear();

        foreach (var prop in PlayerProps.Values)
        {
            prop?.DestroyActor();
        }
        PlayerProps.Clear();
        PlayerCharacters.Clear();
        PlayerActors.Clear();
    }

    public static void RebirthPlayers()
    {
        //lock (PlayerCharacters)
        //{
        //    foreach (var player in PlayerCharacters.Values)
        //    {
        //        player.RebirthInPlace();
        //    }
        //}
        Mod.Rpc?.SendRebirthPlayers();
    }
    public static void RebirthPlayer()
    {
        WukongApi.Sync.LocalMainCharacter?.RebirthAtShrine(WukongApi.Sync.LocalMainCharacter.Value.RebirthPointId);
        WukongApi.Sync.LocalMainCharacter?.RebirthInPlace();
    }

    public static void InitPlayerCharacters()
    {
        var allCharacters = WukongApi.Sync.AllMainCharacters;
        foreach (var character in allCharacters)
        {
            PlayerCharacters[character.PlayerId] = character;
        }
    }

    public static void InitPlayerActors()
    {
        foreach (var character in PlayerCharacters.Values)
        {
            if (Utils.GetCharacterActor(character) is { } actor)
            {
                PlayerActors[character.PlayerId] = actor;
            }
        }
    }

    public static GameStateSnapshot CreateSnapshop()
    {
        return new GameStateSnapshot 
        {
            IsGameActive = IsGameActive,
            HasPreparationEnded = HasPreparationEnded,
            ElapsedRoundTime = ElapsedRoundTime,
            CurrentRound = CurrentRound,
            HidersScore = HidersScore,
            SeekersScore = SeekersScore,

            Seekers = string.Join(",", Seekers.Select(x => x.ToString())),
            Hiders = string.Join(",", Hiders.Select(x => x.ToString())),
            Spectators = string.Join(",", Spectators.Select(x => x.ToString()))
        };
    }

    public static void ApplySnapshop(GameStateSnapshot snapshot)
    {
        IsGameActive = snapshot.IsGameActive;
        HasPreparationEnded = snapshot.HasPreparationEnded;
        ElapsedRoundTime = snapshot.ElapsedRoundTime;
        CurrentRound = snapshot.CurrentRound;
        HidersScore = snapshot.HidersScore;
        SeekersScore = snapshot.SeekersScore;

        string[] seekersArr = string.IsNullOrEmpty(snapshot.Seekers) ? [] : snapshot.Seekers.Split(',');
        string[] hidersArr = string.IsNullOrEmpty(snapshot.Hiders) ? [] : snapshot.Hiders.Split(',');
        string[] spectatorsArr = string.IsNullOrEmpty(snapshot.Spectators) ? [] : snapshot.Spectators.Split(',');

        Seekers = new HashSet<PlayerId>(WukongApi.Sync.AllPlayers.Where(p => seekersArr.Contains(p.ToString())));
        Hiders = new HashSet<PlayerId>(WukongApi.Sync.AllPlayers.Where(p => hidersArr.Contains(p.ToString())));
        Spectators = new HashSet<PlayerId>(WukongApi.Sync.AllPlayers.Where(p => spectatorsArr.Contains(p.ToString())));
    }
}