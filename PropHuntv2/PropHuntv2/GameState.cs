using ReadyM.Api.Idents;
using System.Collections.Generic;
using System.Linq;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace WukongMp.PropHunt;

public static class GameState
{
    public static bool IsGameActive { get; set; } = false;
    public static bool HasPreparationEnded { get; set; } = false;
    public static bool HasPlayerJoinedLate { get; set; } = false;
    public static bool CanPerformTransform { get; set; } = true;
    public static float ElapsedRoundTime { get; set; } = 0f;
    public static float ElapsedTranformTime { get; set; } = 0f;
    public static readonly float TransformDownTime = 30f;

    public static float ElapsedTauntTime { get; set; } = 0f;
    public static readonly float TauntDownTime = 5f;

    public static float ElapsedDecoyTime { get; set; } = 0f;
    public static readonly float DecoyDownTime = 5f;
    public static readonly byte DecoyLimit = 3;
    public static byte PlacedDecoys { get; set; } = 0;

    public static int CurrentRound { get; set; } = 0;

    public static int HidersScore { get; set; } = 0;
    public static int SeekersScore { get; set; } = 0;

    public static HashSet<PlayerId> Seekers { get; set; } = [];
    public static HashSet<PlayerId> Hiders { get; set; } = [];
    public static HashSet<PlayerId> Spectators { get; set; } = [];

    public static Dictionary<PlayerId, AActor> PlayerProps { get; set; } = [];
    public static Dictionary<PlayerId, List<AActor>> PlayerDecoys { get; set; } = [];
    public static Dictionary<PlayerId, ReadyMainCharacter> PlayerCharacters { get; set; } = [];
    public static Dictionary<PlayerId, AActor> PlayerActors { get; set; } = [];

    public static List<string> GameSounds { get; set; } = [];

    public static FVector OriginalSeekerLocation = FVector.ZeroVector;
    public static string CurrentPropName = string.Empty;

    public static void Reset()
    {
        IsGameActive = false;
        HasPreparationEnded = false;

        ElapsedRoundTime = 0f;
        ElapsedTauntTime = 0f;
        ElapsedTranformTime = 0f;
        CanPerformTransform = false;
        ElapsedDecoyTime = 0f;
        PlacedDecoys = 0;

        Seekers.Clear();
        Hiders.Clear();
        Spectators.Clear();

        foreach (var prop in PlayerProps.Values)
        {
            prop?.DestroyActor();
        }
        foreach (var decoy_list in PlayerDecoys.Values)
        {
            foreach (var decoy in decoy_list)
            {
                decoy?.DestroyActor();
            }
        }

        PlayerProps.Clear();
        PlayerDecoys.Clear();
        PlayerCharacters.Clear();
        PlayerActors.Clear();
    }

    public static void RebirthPlayers()
    {
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
        var propsList = new List<PlayerPropNetData>();
        foreach (var prop in PlayerProps)
        {
            if (prop.Value != null)
            {
                propsList.Add(new PlayerPropNetData
                {
                    OwnerId = prop.Key,
                    PropName = prop.Value.GetClass().GetName()
                });
            }
        }

        var decoysList = new List<DecoyNetData>();
        foreach (var decoy_list in PlayerDecoys)
        {
            foreach (var decoyActor in decoy_list.Value)
            {
                if (decoyActor != null)
                {
                    var loc = decoyActor.GetActorLocation();
                    var rot = decoyActor.GetActorRotation();

                    decoysList.Add(new DecoyNetData
                    {
                        OwnerId = decoy_list.Key,
                        PropName = decoyActor.GetClass().GetName(),
                        X = loc.X,
                        Y = loc.Y,
                        Z = loc.Z,
                        Pitch = rot.Pitch,
                        Yaw = rot.Yaw,
                        Roll = rot.Roll
                    });
                }
            }
        }

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
            Spectators = string.Join(",", Spectators.Select(x => x.ToString())),

            ActiveProps = propsList.ToArray(),
            ActiveDecoys = decoysList.ToArray()
        };
    }

    public static void ApplySnapshot(GameStateSnapshot snapshot)
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

        foreach (var propData in snapshot.ActiveProps)
        {
            if (!GameState.PlayerCharacters.TryGetValue(propData.OwnerId, out var character)) continue;
            if (!GameState.PlayerActors.TryGetValue(propData.OwnerId, out var actor)) continue;

            if (Utils.SpawnProp(propData.PropName, character.Location.ToFVector(), character.Rotation.ToFRotator(), actor, false) is not { } prop) continue;
            GameState.PlayerProps[propData.OwnerId] = prop;

        }

        foreach (var decoyData in snapshot.ActiveDecoys)
        {
            FVector location = new FVector(decoyData.X, decoyData.Y, decoyData.Z);
            FRotator rotation = new FRotator(decoyData.Pitch, decoyData.Yaw, decoyData.Roll);

            AActor spawnedDecoy = Utils.SpawnActor(decoyData.PropName, location, rotation);

            if (spawnedDecoy != null)
            {
                var ownerId = decoyData.OwnerId;

                if (!PlayerDecoys.ContainsKey(ownerId))
                {
                    PlayerDecoys[ownerId] = new List<AActor>();
                }
                PlayerDecoys[ownerId].Add(spawnedDecoy);
            }
        }
    }
}