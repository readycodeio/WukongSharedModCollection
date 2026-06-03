using LiteNetLib;
using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Client;
using ReadyM.Api.Multiplayer.Generators;
using ReadyM.Api.Multiplayer.Protocol.Enums;
using ReadyM.Api.Multiplayer.RPC;
using ReadyM.Api.Multiplayer.Serialization;
using ReadyM.Wukong.Common.ECS.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace WukongMp.PropHunt;

public partial class PropHuntRpc(IRpcClient client, IRelaySerializer serializer) : RpcClassBase(client, serializer)
{
    [RpcEvent(RelayMode.GlobalAll)]
    private void OnPlayerHide(PlayerId __sender, PlayerId hiderId, string propClassName)
    {
        RunOnMainThread(() =>
        {
            Core.ExecutionQueue.Enqueue(() =>
            {
                if (!GameState.PlayerCharacters.TryGetValue(hiderId, out var character)) return;
                if (!GameState.PlayerActors.TryGetValue(hiderId, out var actor) || actor == null) return;
                actor.SetActorHiddenInGame(true);
                if (Utils.SpawnProp(propClassName, character.Location.ToFVector(), character.Rotation.ToFRotator(), actor, false) is not { } prop) return;
                if (GameState.PlayerProps.TryGetValue(hiderId, out var oldProp))
                {
                    oldProp?.DestroyActor();
                }
                //GameState.PlayerProps.Add(hiderId, prop);
                GameState.PlayerProps[hiderId] = prop;
            });
        }); 
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnPlayerJoined(PlayerId __sender, PlayerId playerId)
    {
        RunOnMainThread(() =>
        {
            if (GameState.IsGameActive)
            {
                GameState.Spectators.Add(playerId);

                if (playerId == WukongApi.Sync.LocalPlayerId)
                {
                    var myCharacter = WukongApi.Sync.LocalMainCharacter;
                    if (myCharacter.HasValue)
                    {
                        WukongApi.Sync.EnableSpectatorMode(myCharacter.Value, SpectatorReason.Observer);
                    }
                }
            }
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnPlayerFound(PlayerId __sender, PlayerId playerFound)
    {
        RunOnMainThread(() =>
        {
            GameState.Hiders.Remove(playerFound);
            GameState.Spectators.Add(playerFound);

            if (GameState.PlayerProps.TryGetValue(playerFound, out var prop))
            {
                prop?.DestroyActor();
                GameState.PlayerProps.Remove(playerFound);
            }

            if (playerFound == WukongApi.Sync.LocalPlayerId)
            {
                var myCharacter = WukongApi.Sync.LocalMainCharacter;
                if (myCharacter.HasValue)
                {
                    WukongApi.Sync.EnableSpectatorMode(myCharacter.Value, SpectatorReason.Death);
                }
            }

            if (WukongApi.Sync.IsMasterClient)
            {
                WukongApi.Sync.TryGetPlayerInfoById(playerFound, out var nickname, out _);
                WukongApi.Chat.SendServerMessage($"Hider {nickname} was found! Hiders remaining: {GameState.Hiders.Count}");
            }
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnRebirthPlayers()
    {
        RunOnMainThread(() => 
        {
            GameState.RebirthPlayer();
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnGameStart(PlayerId __sender, GameStateSnapshot _snapshot, GameConfig _config)
    {
        RunOnMainThread(() =>
        {
            GameState.Reset();
            GameState.ApplySnapshop(_snapshot);
            Core.Config = _config;

            GameState.InitPlayerCharacters();
            GameState.InitPlayerActors();

            PlayerId? myId = WukongApi.Sync.LocalPlayerId;
            if (myId == null) return;

            if (GameState.Seekers.Contains(myId.Value))
            {
                WukongApi.Local.ShowTip("You are a seeker!", true);
                GameState.OriginalSeekerLocation = WukongApi.Sync.LocalMainCharacter.Value.Location.ToFVector();

                if (!WukongApi.PvP.OwnsPvpState) WukongApi.PvP.InitializeAreaPvpState();
                WukongApi.PvP.InPvP = true;
                WukongApi.PvP.InPvpTournament = false;
            }
            else if (GameState.Hiders.Contains(myId.Value))
            {
                if (!WukongApi.PvP.OwnsPvpState) WukongApi.PvP.InitializeAreaPvpState();
                WukongApi.PvP.InPvP = true;
                WukongApi.PvP.InPvpTournament = false;

                Random random = new Random();
                string propName = Utils.propClassNames[random.Next(Utils.propClassNames.Length)];
                WukongApi.Local.ShowTip("You are a hiding as " + propName, true);
                Mod.Rpc?.SendPlayerHide(myId.Value, propName);
            }
            else if (GameState.Spectators.Contains(myId.Value))
            {
                WukongApi.Sync.EnableSpectatorMode(WukongApi.Sync.LocalMainCharacter.Value, SpectatorReason.Observer);
            }
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnSeekersStart(PlayerId __sender) 
    {
        RunOnMainThread(() => 
        { 
            GameState.HasPreparationEnded = true;
            WukongApi.Chat.ShowLocalMessage("Seekers are on the hunt!", FLinearColor.Yellow);
            var myId = WukongApi.Sync.LocalPlayerId;
            if (myId != null && GameState.Seekers.Contains(myId.Value))
            {
                Core.ExecutionQueue.Enqueue(() =>
                {
                    /*if (GameState.PlayerActors.TryGetValue(myId.Value, out var actor))
                    {
                        //actor.Teleport(GameState.OriginalSeekerLocation, actor.GetActorRotation());
                        //actor.SetActorLocation(GameState.OriginalSeekerLocation, false, out _, true);
                    }*/
                    var seeker = WukongApi.Sync.LocalMainCharacter;
                    if (seeker.HasValue)
                    {
                        var newLocation = GameState.OriginalSeekerLocation;
                        newLocation.Z += 50;
                        seeker.Value.Teleport(newLocation.ToVector3(), seeker.Value.Rotation);
                    }
                });
            }
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnGameStop(PlayerId __sender)
    {
        //Game force stop by command prop stop
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnGameEnd(PlayerId __sender, Core.Team winnerTeam)
    {
        RunOnMainThread(() =>
        {
            Core.ExecutionQueue.Enqueue(() =>
            {
                if (Core.CurrentGameMode is PropHuntGameMode gameMode)
                {
                    gameMode.OnEnd(winnerTeam);
                }

                foreach (var prop in GameState.PlayerProps.Values)
                {
                    prop?.DestroyActor();
                }
                GameState.PlayerProps.Clear();

                foreach (var actor in GameState.PlayerActors.Values)
                {
                    if (actor != null)
                    {
                        actor.SetActorHiddenInGame(false);
                        actor.SetActorEnableCollision(true);
                    }
                }

                var myCharacter = WukongApi.Sync.LocalMainCharacter;
                if (myCharacter.HasValue)
                {
                    WukongApi.Sync.DisableSpectatorMode(myCharacter.Value);
                    int shrineId = myCharacter.Value.RebirthPointId;
                    myCharacter.Value.RebirthAtShrine(shrineId);
                }
            });
        });
    }
}
