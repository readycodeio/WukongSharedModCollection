using CoreSound;
using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Client;
using ReadyM.Api.Multiplayer.Generators;
using ReadyM.Api.Multiplayer.Protocol.Enums;
using ReadyM.Api.Multiplayer.RPC;
using ReadyM.Api.Multiplayer.Serialization;
using ReadyM.Wukong.Common.ECS.Values;
using System;
using System.Linq;
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
            if (!GameState.PlayerCharacters.TryGetValue(hiderId, out var character)) return;
            if (!GameState.PlayerActors.TryGetValue(hiderId, out var actor) || actor == null) return;
            try
            { 
                actor.SetActorHiddenInGame(true);

                character.Pawn?.CanBeDamaged = true;

                if (Utils.SpawnProp(propClassName, character.Location.ToFVector(), character.Rotation.ToFRotator(), actor, false) is not { } prop) return;
                if (GameState.PlayerProps.TryGetValue(hiderId, out var oldProp))
                {
                    oldProp?.DestroyActor();
                }
                GameState.PlayerProps[hiderId] = prop;
            }
            catch (Exception ex)
            {
                Logging.LogError($"OnPlayerHide: {ex.Message}");
            }
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnDecoyDeploy(PlayerId __sender, string propClassName)
    {
        RunOnMainThread(() =>
        {
            if (!GameState.PlayerCharacters.TryGetValue(__sender, out var character)) return;
            if (!GameState.PlayerActors.TryGetValue(__sender, out var actor) || actor == null) return;
            try
            {
                var decoy = Utils.SpawnActor(propClassName, actor.GetActorLocation(), actor.GetActorRotation());

                decoy?.SetActorEnableCollision(false);
                decoy?.SetActorHiddenInGame(false);
                GameState.PlayerDecoys[__sender].Add(decoy);
            }
            catch (Exception ex)
            {
                Logging.LogError($"OnDecoyDeploy: {ex.Message}");
            }
        });
    }

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
    private void OnForceHidersPlaySound(string soundName)
    {
        RunOnMainThread(() => 
        {
            if (GameState.GameSounds.Contains(soundName))
            {
                foreach (var hider in GameState.Hiders)
                {
                    if (GameState.PlayerActors.TryGetValue(hider, out var actor))
                    {
                        SoundCore.PlaySound(soundName, location: actor.GetActorLocation());
                    }
                }
            }
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnHiderPlaySound(PlayerId __sender, string soundName)
    {
        if (GameState.GameSounds.Contains(soundName))
        {
            if (GameState.Hiders.Contains(__sender))
            {
                if (GameState.PlayerActors.TryGetValue(__sender, out var character))
                {
                    SoundCore.PlaySound(soundName, location: character.GetActorLocation());
                }
            }
        }
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnForcePlayersPlaySound(string soundName)
    {
        if (GameState.GameSounds.Contains(soundName))
        {
            foreach (var playerActor in GameState.PlayerActors.Values)
            {
                if (playerActor != null)
                {
                    SoundCore.PlaySound(soundName, location: playerActor.GetActorLocation());
                }
            }
        }
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnPlayerPlaySound(PlayerId __sender, string soundName)
    {
        if (GameState.GameSounds.Contains(soundName))
        {
            var character = WukongApi.Sync.GetMainCharacterByPlayerId(__sender);
            if (character.HasValue)
            {
                SoundCore.PlaySound(soundName, location: character.Value.Location.ToFVector());
            }
        }
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnLateJoinSnapshot(PlayerId __sender, PlayerId targetPlayerId, GameStateSnapshot snapshot)
    {
        RunOnMainThread(() =>
        {
            if (!GameState.Spectators.Contains(targetPlayerId))
            {
                GameState.Spectators.Add(targetPlayerId);
            }

            if (WukongApi.Sync.LocalPlayerId == targetPlayerId)
            {
                GameState.ApplySnapshot(snapshot);

                var myCharacter = WukongApi.Sync.LocalMainCharacter;
                if (myCharacter.HasValue)
                {
                    WukongApi.Sync.EnableSpectatorMode(myCharacter.Value, SpectatorReason.Observer);
                    WukongApi.Local.ShowTip("Round already in progress, joining as a spectator!", true);
                }
            }
            else
            {
                if (!WukongApi.Sync.IsMasterClient)
                {
                    GameState.ElapsedRoundTime = snapshot.ElapsedRoundTime;
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
            GameState.ApplySnapshot(_snapshot);
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
                    }
                }
            }
            else if (GameState.Hiders.Contains(myId.Value))
            {
                Random random = new Random();
                string propName = Utils.propClassNames[random.Next(Utils.propClassNames.Length)];
                GameState.CurrentPropName = propName;
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

            if (!Core.Config.BloodBarsVisible)
            {
                try
                {
                    if (PatchInitBloodBarUI.CachedBloodBars == null)
                    {
                        return;
                    }

                    foreach (var widget in PatchInitBloodBarUI.CachedBloodBars.Values)
                    {
                        if (widget != null)
                        {
                            widget.SetVisibility(UnrealEngine.UMG.ESlateVisibility.Hidden);
                        }
                    }
                }
                catch (Exception ex)
                {
                    WukongApi.Chat.ShowLocalMessage(ex.Message, FLinearColor.Red);
                }
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
                    var seeker = WukongApi.Sync.LocalMainCharacter;
                    if (seeker.HasValue)
                    {
                        var newLocation = GameState.OriginalSeekerLocation;
                        newLocation.Z += 50;
                        seeker.Value.Teleport(newLocation.ToVector3(), seeker.Value.Rotation);
                    }
            }
            
        });
    }

    [RpcEvent(RelayMode.GlobalAll)]
    private void OnGameStop(PlayerId __sender)
    {
        
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
