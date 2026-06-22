using CoreSound;
using LiteNetLib;
using ReadyM.Api.DI;
using ReadyM.Api.Idents;
using UnrealEngine.Runtime;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace WukongMp.PropHunt;

public sealed class PropHuntService : IHostedService
{
    public void OnScopeStart()
    {
        WukongApi.Events.OnPlayerPawnSpawned += OnPlayerConnected;
        WukongApi.Events.OnDisconnected += OnPlayerDisconnected;
        WukongApi.Events.OnPlayerDead += OnPlayerFound;
        WukongApi.Events.OnJoinedArea += OnAreaJoinedHanlder;
        RegisterPropHuntSounds();
    }

    private void RegisterPropHuntSounds()
    {
        SoundCore.RegisterSound("Taunt_Stick", "AkAudioEvent'/Game/00Main/Audio/SFX/Environment/BPO/ENV_Position_BPO_Stick.ENV_Position_BPO_Stick'");
        GameState.GameSounds.Add("Taunt_Stick");
    }

    private void OnAreaJoinedHanlder(AreaId id)
    {
        if (WukongApi.Sync.IsMasterClient)
        {
            WukongApi.PvP.InitializeAreaPvpState();
        }
        WukongApi.Configuration.IsSupportMultiLockEnabled = false;        
    }

    private void OnPlayerConnected(ReadyMainCharacter character)
    {
        GameState.PlayerCharacters[character.PlayerId] = character;
        if (Utils.GetCharacterActor(character) is { } actor)
        {
            GameState.PlayerActors[character.PlayerId] = actor;
        }

        GameState.HasPlayerJoinedLate = true;

        if (WukongApi.Sync.IsMasterClient)
        {
            ManagePlayerJoin(character);
        }
    }
    public void ManagePlayerJoin(ReadyMainCharacter character)
    {
        if (GameState.IsGameActive)
        {
            var snapshot = GameState.CreateSnapshop();
            Mod.Rpc?.SendLateJoinSnapshot(character.PlayerId, snapshot);
        }
    }
    private void OnPlayerFound(ReadyMainCharacter character, ReadyCharacter? nullable)
    {
        if (!WukongApi.Sync.IsMasterClient || !GameState.IsGameActive)
        {
            return;
        }

        if (GameState.Hiders.Contains(character.PlayerId))
        {
            Mod.Rpc?.SendPlayerFound(character.PlayerId);
        }
    }

    private void OnPlayerDisconnected(PlayerId id, DisconnectReason reason)
    {
        if (GameState.Seekers.Contains(id)) GameState.Seekers.Remove(id);
        else if (GameState.Hiders.Contains(id))
        { 
            GameState.Hiders.Remove(id);
            if (GameState.PlayerProps.ContainsKey(id))
            {
                GameState.PlayerProps[id].DestroyActor();
                GameState.PlayerProps.Remove(id);
            }
        }
        else if (GameState.Spectators.Contains(id)) GameState.Spectators.Remove(id);
    }

    public void Dispose()
    {
        WukongApi.Events.OnPlayerPawnSpawned -= OnPlayerConnected;
        WukongApi.Events.OnDisconnected -= OnPlayerDisconnected;
        WukongApi.Events.OnPlayerDead -= OnPlayerFound;
    }
}
