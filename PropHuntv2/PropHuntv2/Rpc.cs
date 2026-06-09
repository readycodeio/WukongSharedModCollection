using LiteNetLib;
using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Client;
using ReadyM.Api.Multiplayer.Generators;
using ReadyM.Api.Multiplayer.Protocol.Enums;
using ReadyM.Api.Multiplayer.RPC;
using ReadyM.Api.Multiplayer.Serialization;
using ReadyM.Wukong.Common.ECS.Components;
using ReadyM.Wukong.Common.ECS.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.WukongUtils;
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

            if (!GameState.PlayerCharacters.TryGetValue(hiderId, out var character)) return;
            if (!GameState.PlayerActors.TryGetValue(hiderId, out var actor) || actor == null) return;

            actor.SetActorHiddenInGame(true);

            //if ((bool)!WukongApi.Sync.LocalMainCharacter?.AreTeamsEqual(character.PlayerId))
            //{
            //    character.HideMarker();
            //}

            character.Pawn?.CanBeDamaged = true;

            if (Utils.SpawnProp(propClassName, character.Location.ToFVector(), character.Rotation.ToFRotator(), actor, false) is not { } prop) return;
            if (GameState.PlayerProps.TryGetValue(hiderId, out var oldProp))
            {
                oldProp?.DestroyActor();
            }
        });
    }

    //[RpcEvent(RelayMode.GlobalAll)]
    //private void OnDecoyDeploy(PlayerId __sender, PlayerId hiderId, string propClassName)
    //{
    //    RunOnMainThread(() =>
    //    {

    //    });
    //}

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnPropRotate(PlayerId __sender, float newRotation)
    {
        if (__sender == WukongApi.Sync.LocalPlayerId) return;

        RunOnMainThread(() =>
        {
            if (GameState.PlayerProps.TryGetValue(__sender, out var remoteProp) && remoteProp != null)
            {
                remoteProp.SetActorRelativeRotation(new FRotator(0f, newRotation, 0f), false, out _, false);
            }
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
            //WukongApi.Chat.ShowLocalMessage("1",FLinearColor.Yellow);
            GameState.Hiders.Remove(playerFound);
            GameState.Spectators.Add(playerFound);
            //WukongApi.Chat.ShowLocalMessage("2",FLinearColor.Yellow);

            if (GameState.PlayerProps.TryGetValue(playerFound, out var prop))
            {
                prop?.DestroyActor();
                GameState.PlayerProps.Remove(playerFound);
                //WukongApi.Chat.ShowLocalMessage("3",FLinearColor.Yellow);
            }

            if (playerFound == WukongApi.Sync.LocalPlayerId)
            {
                var myCharacter = WukongApi.Sync.LocalMainCharacter;
                if (myCharacter.HasValue)
                {
                    WukongApi.Sync.EnableSpectatorMode(myCharacter.Value, SpectatorReason.Death);
                }
                //WukongApi.Chat.ShowLocalMessage("4",FLinearColor.Yellow);
            }

            if (WukongApi.Sync.IsMasterClient)
            {
                WukongApi.Sync.TryGetPlayerInfoById(playerFound, out var nickname, out _);
                WukongApi.Chat.SendServerMessage($"Hider {nickname} was found! Hiders remaining: {GameState.Hiders.Count}");
                //WukongApi.Chat.ShowLocalMessage("5",FLinearColor.Yellow);
            }
            //WukongApi.Chat.ShowLocalMessage("6",FLinearColor.Yellow);
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

            if (Core.Config.DestroyTamers)
            {
                if (WukongApi.Sync.IsMasterClient)
                {
                    WukongApi.Chat.SendServerMessage("Tamers destroyed");
                }

                var tamers = WukongApi.Sync.AllTamers;
                foreach (var tamer in tamers)
                {
                    tamer.Pawn?.DestroyActor();
                }
            }

            GameState.Reset();
            GameState.ApplySnapshop(_snapshot);
            Core.Config = _config;

            GameState.InitPlayerCharacters();
            GameState.InitPlayerActors();

            PlayerId? myId = WukongApi.Sync.LocalPlayerId;
            if (myId == null) return;

            var character = WukongApi.Sync.LocalMainCharacter.Value;

            character.EnableInteraction(false);

            if (GameState.Seekers.Contains(myId.Value))
            {
                WukongApi.Local.ShowTip("You are a seeker!", true);
                GameState.OriginalSeekerLocation = WukongApi.Sync.LocalMainCharacter.Value.Location.ToFVector();

                character.Pawn?.SetTeamID((int)Core.Team.Seeker);   
                character.TeamId = (int)Core.Team.Seeker;
                //character.EnableInteraction(true);

                foreach (var hiderId in GameState.Hiders)
                {
                    if (GameState.PlayerCharacters.TryGetValue(hiderId, out var hiderCharacter))
                    {
                        hiderCharacter.HideMarker();
                        //hiderCharacter.HideHpBar();
                    }
                }
            }
            else if (GameState.Hiders.Contains(myId.Value))
            {
                Random random = new Random();
                string propName = Utils.propClassNames[random.Next(Utils.propClassNames.Length)];
                WukongApi.Local.ShowTip("You are a hiding as " + propName, true);
                Mod.Rpc?.SendPlayerHide(myId.Value, propName);

                character.Pawn?.SetTeamID((int)Core.Team.Hider);
                character.Pawn?.CanBeDamaged = true;
                //character.EnableInteraction(true);

                character.TeamId = (int)Core.Team.Hider;
            }
            else if (GameState.Spectators.Contains(myId.Value))
            {
                character.TeamId = (int)Core.Team.Spectator;
                WukongApi.Sync.EnableSpectatorMode(WukongApi.Sync.LocalMainCharacter.Value, SpectatorReason.Observer);
            }

            if (!WukongApi.PvP.OwnsPvpState) WukongApi.PvP.InitializeAreaPvpState();
            WukongApi.PvP.InPvP = true;
            WukongApi.PvP.InPvpTournament = false;
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
            }

            foreach (var player in WukongApi.Sync.AllPlayers)
            {
                if (GameState.PlayerProps.ContainsKey(player))
                {
                    GameState.PlayerProps[player].DisableInput(GameUtils.GetPlayerController());
                }
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
                var character = myCharacter.Value;
                character.TeamId = (int)Core.Team.Spectator;

                if (myCharacter.Value.IsSpectator)
                {
                    WukongApi.Sync.DisableSpectatorMode(myCharacter.Value);
                    int shrineId = myCharacter.Value.RebirthPointId;
                    myCharacter.Value.RebirthAtShrine(shrineId);
                }
            }
        });
    }
}
