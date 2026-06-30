using b1;
using b1.Plugins.AkAudio;
using ReadyM.Api.Idents;
using System;
using System.Collections.Generic;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.WukongUtils;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace WukongMp.Toolkit.Audio;

public static class AudioCore
{
    public class ActiveSpatialLoop
    {
        public string SoundName { get; set; }
        public FVector Location { get; set; }
        public FRotator Rotation { get; set; }
        public bool IsLooping { get; set; }
        public float Interval { get; set; }
        public float TimeSinceLastPlay { get; set; }
        public float TotalElapsed { get; set; }
        public Action<float> Callback { get; set; }
        public int LastPlayingId { get; set; }
    }

    public class ActiveFlatLoop
    {
        public string SoundName { get; set; }
        public bool IsLooping { get; set; }
        public float Interval { get; set; }
        public float TimeSinceLastPlay { get; set; }
        public float TotalElapsed { get; set; }
        public Action<float> Callback { get; set; }
        public int LastPlayingId { get; set; }
        public bool IsWaiting { get; set; }
    }

    public static List<ActiveSpatialLoop> LocationLoops = [];
    public static List<ActiveFlatLoop> FlatLoops = [];

    private static readonly Dictionary<string, string> SoundRegistry = new();
    private static readonly Dictionary<string, UObject> LoadedEvents = new();

    public static void RegisterSound(string friendlyName, string assetPath)
    {
        SoundRegistry[friendlyName] = assetPath;
    }

    public static UAkAudioEvent? GetOrLoadEvent(string soundName)
    {
        if (LoadedEvents.TryGetValue(soundName, out var cachedObj))
        {
            if (cachedObj != null && cachedObj.Address != IntPtr.Zero)
                return cachedObj as UAkAudioEvent;
        }

        if (SoundRegistry.TryGetValue(soundName, out var path))
        {
            var newObj = UObject.LoadObject<UObject>(null, path);
            if (newObj != null && newObj.Address != IntPtr.Zero)
            {
                LoadedEvents[soundName] = newObj;
                return newObj as UAkAudioEvent;
            }
        }

        return null;
    }

    public static void ClearCache()
    {
        lock (LoadedEvents)
        {
            LoadedEvents.Clear();
        }
    }

    public static int PlaySound(
            string soundName,
            FVector? location = null,
            FRotator? rotation = null,
            PlayerId? targetPlayer = null,
            bool bLoop = false,
            float loopInterval = 2.0f,
            Action<float> callback = null,
            bool sync = false)
    {
        if (targetPlayer != null)
            return Spatial.PlaySoundAtPlayer(soundName, targetPlayer.Value, sync);

        if (location != null && rotation != null)
            return Spatial.PlaySoundAtLocation(soundName, location.Value, rotation.Value, bLoop, loopInterval, callback, bSync: sync);

        return Flat.Play2DSound(soundName, bLoop, loopInterval, callback, bSync: sync);
    }

    public static void StopSound(string soundName)
    {
        Spatial.StopSound(soundName);
        Flat.StopSound(soundName);
    }

    public static class Spatial
    {
        private static readonly Dictionary<string, int> SoundList = new();

        public static int PlaySoundAtLocation(
            string soundName,
            FVector location,
            FRotator? rotation = null,
            bool bLoop = false,
            float loopInterval = 2.0f,
            Action<float> callback = null,
            bool bSync= false)
        {
            try
            {
                if (location == null || string.IsNullOrEmpty(soundName)) return -1;

                var akObj = GetOrLoadEvent(soundName);
                string eventString = akObj != null ? "" : soundName;
                FRotator actualRotation = rotation ?? FRotator.ZeroRotator;

                if (bSync)
                {
                    SoundParamsSpatial paramsSpatial = new SoundParamsSpatial();
                    paramsSpatial.SoundSource = soundName;
                    paramsSpatial.IsLoop = bLoop;
                    paramsSpatial.X = location.X; paramsSpatial.Y = location.Y; paramsSpatial.Z = location.Z;
                    paramsSpatial.Pitch = actualRotation.Pitch; paramsSpatial.Yaw = actualRotation.Yaw; paramsSpatial.Roll = actualRotation.Roll;

                    Mod.audioRpc?.SendSyncSoundAtLocation(paramsSpatial);
                }

                if (!bLoop)
                {
                    int id = UAkGameplayStatics.PostEventAtLocation(akObj, location, actualRotation, eventString, GameUtils.GetWorld());
                    callback?.Invoke(0f);
                    return id;
                }

                int firstId = UAkGameplayStatics.PostEventAtLocation(akObj, location, actualRotation, eventString, GameUtils.GetWorld());
                callback?.Invoke(0f);

                lock (LocationLoops)
                {
                    LocationLoops.Add(new ActiveSpatialLoop
                    {
                        SoundName = soundName,
                        Location = location,
                        Rotation = actualRotation,
                        IsLooping = true,
                        Interval = loopInterval,
                        TimeSinceLastPlay = 0f,
                        TotalElapsed = 0f,
                        Callback = callback,
                        LastPlayingId = firstId
                    });
                }

                return firstId;
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                return -1;
            }
        }

        public static int PlaySoundAtPlayer(string soundName, PlayerId playerId, bool bSync = false)
        {
            if (string.IsNullOrEmpty(soundName)) return -1;

            if (bSync)
            {
                SoundParamsFlat soundParams = new SoundParamsFlat();
                soundParams.SoundSource = soundName;
                soundParams.IsLoop = false;

                Mod.audioRpc?.SendSyncSoundAtPlayer(playerId, soundParams);
            }

            var character = WukongApi.Sync.GetMainCharacterByPlayerId(playerId);
            
            if (character.HasValue && character.Value.Pawn != null && character.Value.Pawn.Address != IntPtr.Zero)
            {
                var akObj = GetOrLoadEvent(soundName);
                string eventString = akObj != null ? "" : soundName;

                int id = UAkGameplayStatics.PostEvent(akObj, character.Value.Pawn, 0, null, false, eventString);
                if (id > 0)
                {
                    SoundList[soundName] = id;
                }
                return id;
            }

            return -1;
        }

        public static void StopSound(string soundName)
        {
            try
            {
                if (soundName == null) return;

                lock (LocationLoops)
                {
                    LocationLoops.RemoveAll(l => l.SoundName == soundName);
                }

                if (SoundList.TryGetValue(soundName, out int id))
                {
                    UBGUFunctionLibAK.BGUAKStopPlayingID(id, 200, 4);
                    SoundList.Remove(soundName);
                }
            }
            catch (Exception ex) { Logging.LogError(ex.Message); }
        }

        public static int StopAllSounds()
        {
            int count = 0;
            try
            {
                lock (LocationLoops) { count += LocationLoops.Count; LocationLoops.Clear(); }

                foreach (var kvp in SoundList)
                {
                    UBGUFunctionLibAK.BGUAKStopPlayingID(kvp.Value, 200, 4);
                    count++;
                }
                SoundList.Clear();
            }
            catch (Exception ex) { Logging.LogException(ex); }
            return count;
        }
    }

    public static class Flat
    {
        private static readonly Dictionary<string, int> SoundList = new();
        private const int AK_EndOfEvent = 1;

        public static int Play2DSound(
            string soundName,
            bool bLoop = false,
            float loopInterval = 2.0f,
            Action<float> callback = null,
            bool bSync = false)
        {
            try
            {
                if (string.IsNullOrEmpty(soundName)) return -1;

                var akObj = GetOrLoadEvent(soundName);
                string eventString = akObj != null ? "" : soundName;
                var pawn = GameUtils.GetControlledPawn();

                if (bSync)
                {
                    SoundParamsFlat soundParams = new();
                    soundParams.SoundSource = soundName;
                    soundParams.IsLoop = bLoop;

                    Mod.audioRpc?.SendSyncSoundFlat(soundParams);
                }

                if (!bLoop)
                {
                    int id = akObj != null
                        ? UAkGameplayStatics.PostEvent(akObj, pawn, 0, null, false, "")
                        : UBGUFunctionLibAK.PostAkEventOnDummyActor(soundName, null);

                    if (id > 0) SoundList[soundName] = id;
                    callback?.Invoke(0f);
                    return id;
                }

                FOnAkPostEventCallback eventCallback = new FOnAkPostEventCallback();
                eventCallback.Bind(new FOnAkPostEventCallback.Signature((type, info) =>
                {
                    lock (FlatLoops)
                    {
                        var loop = FlatLoops.Find(l => l.SoundName == soundName);
                        if (loop != null)
                        {
                            loop.IsWaiting = true;
                            loop.TimeSinceLastPlay = 0f;
                        }
                    }
                }));

                int firstId = UAkGameplayStatics.PostEvent(akObj, pawn, AK_EndOfEvent, eventCallback, false, eventString);
                callback?.Invoke(0f);

                lock (FlatLoops)
                {
                    FlatLoops.Add(new ActiveFlatLoop
                    {
                        SoundName = soundName,
                        IsLooping = true,
                        Interval = loopInterval,
                        TimeSinceLastPlay = 0f,
                        TotalElapsed = 0f,
                        Callback = callback,
                        LastPlayingId = firstId,
                        IsWaiting = false
                    });
                }

                return firstId;
            }
            catch (Exception ex) { Logging.LogError(ex.Message); return -1; }
        }

        public static void StopSound(string soundName)
        {
            try
            {
                if (string.IsNullOrEmpty(soundName)) return;

                lock (FlatLoops) { FlatLoops.RemoveAll(l => l.SoundName == soundName); }

                if (SoundList.TryGetValue(soundName, out int id))
                {
                    UBGUFunctionLibAK.BGUAKStopPlayingID(id, 150, 4);
                    SoundList.Remove(soundName);
                }
            }
            catch (Exception ex) { Logging.LogError(ex.Message); }
        }

        public static int StopAllSounds()
        {
            int count = 0;
            try
            {
                lock (FlatLoops) { count += FlatLoops.Count; FlatLoops.Clear(); }

                foreach (var kvp in SoundList)
                {
                    UBGUFunctionLibAK.BGUAKStopPlayingID(kvp.Value, 150, 4);
                    count++;
                }
                SoundList.Clear();
            }
            catch (Exception ex) { Logging.LogException(ex); }
            return count;
        }
    }
}