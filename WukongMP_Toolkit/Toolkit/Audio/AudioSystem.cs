using b1.Plugins.AkAudio;
using System;
using WukongMp.Api;
using WukongMp.Api.WukongUtils;
using WukongMp.Sdk;

namespace WukongMp.Toolkit.Audio;

public class AudioSystem : ModSystemBase
{
    protected override void OnUpdate(UpdateTick tick)
    {
        try
        {
            float deltaTime = tick.deltaTime;

            lock (AudioCore.LocationLoops)
            {
                for (int i = AudioCore.LocationLoops.Count - 1; i >= 0; i--)
                {
                    var loop = AudioCore.LocationLoops[i];
                    loop.TimeSinceLastPlay += deltaTime;
                    loop.TotalElapsed += deltaTime;

                    if (loop.TimeSinceLastPlay >= loop.Interval)
                    {
                        if (GameUtils.GetWorld() == null)
                        {
                            AudioCore.LocationLoops.RemoveAt(i);
                            continue;
                        }

                        var akObj = AudioCore.GetOrLoadEvent(loop.SoundName);
                        string eventStr = akObj != null ? "" : loop.SoundName;

                        loop.LastPlayingId = UAkGameplayStatics.PostEventAtLocation(akObj, loop.Location, loop.Rotation, eventStr, GameUtils.GetWorld());
                        loop.TimeSinceLastPlay = 0f;
                        loop.Callback?.Invoke(loop.TotalElapsed);
                    }
                }
            }

            lock (AudioCore.FlatLoops)
            {
                for (int i = AudioCore.FlatLoops.Count - 1; i >= 0; i--)
                {
                    var loop = AudioCore.FlatLoops[i];
                    loop.TotalElapsed += deltaTime;

                    if (!loop.IsWaiting) continue;

                    loop.TimeSinceLastPlay += deltaTime;

                    if (loop.TimeSinceLastPlay >= loop.Interval)
                    {
                        var playerPawn = GameUtils.GetControlledPawn();
                        if (playerPawn == null || playerPawn.Address == IntPtr.Zero)
                        {
                            AudioCore.FlatLoops.RemoveAt(i);
                            continue;
                        }

                        FOnAkPostEventCallback retriggerCallback = new FOnAkPostEventCallback();
                        string currentSoundName = loop.SoundName;

                        retriggerCallback.Bind(new FOnAkPostEventCallback.Signature((type, info) =>
                        {
                            lock (AudioCore.FlatLoops)
                            {
                                var currentLoop = AudioCore.FlatLoops.Find(l => l.SoundName == currentSoundName);
                                if (currentLoop != null)
                                {
                                    currentLoop.IsWaiting = true;
                                    currentLoop.TimeSinceLastPlay = 0f;
                                }
                            }
                        }));

                        var akObj = AudioCore.GetOrLoadEvent(loop.SoundName);
                        string eventStr = akObj != null ? "" : loop.SoundName;

                        loop.LastPlayingId = UAkGameplayStatics.PostEvent(akObj, playerPawn, 1, retriggerCallback, false, eventStr);
                        loop.IsWaiting = false;
                        loop.Callback?.Invoke(loop.TotalElapsed);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logging.LogException(ex);
        }
    }
}