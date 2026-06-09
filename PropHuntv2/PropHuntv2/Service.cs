using ReadyM.Api.DI;
using ReadyM.Api.Idents;
using System;
using System.Collections.Generic;
using System.Text;
using ReadyM;
using WukongMp;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;
using UnrealEngine.Engine;
using WukongMp.Api.WukongUtils;
using ReadyM.Wukong.Common.ECS.Values;
using UnrealEngine.Plugins.ControlRig;
using LiteNetLib;
using UnrealEngine.Runtime;
using b1;

namespace WukongMp.PropHunt;

public sealed class PropHuntService : IHostedService
{
    public void OnScopeStart()
    {
        WukongApi.Events.OnPlayerPawnSpawned += OnPlayerConnected;
        WukongApi.Events.OnDisconnected += OnPlayerDisconnected;
        WukongApi.Events.OnPlayerDead += OnPlayerFound;
        WukongApi.Events.OnJoinedArea += OnAreaJoinedHanlder;
    }

    private void OnAreaJoinedHanlder(AreaId id)
    {
        if (WukongApi.Sync.IsMasterClient)
        {
            WukongApi.PvP.InitializeAreaPvpState();
        }
        WukongApi.Configuration.IsSupportMultiLockEnabled = false;        
    }

    //private void OnPlayerConnected(ReadyMainCharacter character)
    //{
    //    PlayerId id = character.PlayerId;
    //    GameState.PlayerCharacters[id] = character;
    //    var actor = Utils.GetCharacterActor(character);
    //    if (actor != null)
    //    {
    //        GameState.PlayerActors[id] = actor;
    //    }

    //    if (Core.CurrentGameMode is PropHuntGameMode gameMode)
    //    {
    //        gameMode.ManagePlayerJoin(character);
    //    }

    //    if (WukongApi.Sync.IsMasterClient)
    //    {
    //        Mod.Rpc?.SendPlayerJoined(character.PlayerId);
    //    }
    //}

    private void OnPlayerConnected(ReadyMainCharacter character)
    {
        GameState.PlayerCharacters[character.PlayerId] = character;
        if (Utils.GetCharacterActor(character) is { } actor)
        {
            GameState.PlayerActors[character.PlayerId] = actor;
        }
        
        GameState.HasPlayerJoinedLate = true;

        if (Core.CurrentGameMode is PropHuntGameMode gameMode)
        {
            gameMode.ManagePlayerJoin(character);
        }

        if (WukongApi.Sync.IsMasterClient)
        {
            Mod.Rpc?.SendPlayerJoined(character.PlayerId);
        }
    }

    private void OnPlayerFound(ReadyMainCharacter character, ReadyCharacter? nullable)
    {
        WukongApi.Chat.ShowLocalMessage("event strzelił", FLinearColor.Yellow);

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
