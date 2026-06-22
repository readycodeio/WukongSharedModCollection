using b1.Plugins.AkAudio;
using System;
using System.Collections.Generic;
using System.Text;
using UnrealEngine.Engine;
using WukongMp.Api;
using WukongMp.Api.WukongUtils;
using WukongMp.Sdk;

namespace CoreSound;

public class SoundCoreSystem : ModSystemBase
{
    protected override void OnUpdate(UpdateTick tick)
    {
        try
        {
            float deltaTime = tick.deltaTime;

            lock (SoundCore.LocationLoops)
            {
                for (int i = SoundCore.LocationLoops.Count - 1; i >= 0; i--)
                {
                    var loop = SoundCore.LocationLoops[i];
                    loop.TimeSinceLastPlay += deltaTime;
                    loop.TotalElapsed += deltaTime;

                    if (loop.TimeSinceLastPlay >= loop.Interval)
                    {
                        if (GameUtils.GetWorld() == null)
                        {
                            SoundCore.LocationLoops.RemoveAt(i);
                            continue;
                        }

                        var akObj = SoundCore.GetOrLoadEvent(loop.SoundName);
                        string eventStr = akObj != null ? "" : loop.SoundName;

                        loop.LastPlayingId = UAkGameplayStatics.PostEventAtLocation(akObj, loop.Location, loop.Rotation, eventStr, GameUtils.GetWorld());
                        loop.TimeSinceLastPlay = 0f;
                        loop.Callback?.Invoke(loop.TotalElapsed);
                    }
                }
            }

            lock (SoundCore.FlatLoops)
            {
                for (int i = SoundCore.FlatLoops.Count - 1; i >= 0; i--)
                {
                    var loop = SoundCore.FlatLoops[i];
                    loop.TotalElapsed += deltaTime;

                    if (!loop.IsWaiting) continue;

                    loop.TimeSinceLastPlay += deltaTime;

                    if (loop.TimeSinceLastPlay >= loop.Interval)
                    {
                        var playerPawn = GameUtils.GetControlledPawn();
                        if (playerPawn == null || playerPawn.Address == IntPtr.Zero)
                        {
                            SoundCore.FlatLoops.RemoveAt(i);
                            continue;
                        }

                        FOnAkPostEventCallback retriggerCallback = new FOnAkPostEventCallback();
                        string currentSoundName = loop.SoundName;

                        retriggerCallback.Bind(new FOnAkPostEventCallback.Signature((type, info) =>
                        {
                            lock (SoundCore.FlatLoops)
                            {
                                var currentLoop = SoundCore.FlatLoops.Find(l => l.SoundName == currentSoundName);
                                if (currentLoop != null)
                                {
                                    currentLoop.IsWaiting = true;
                                    currentLoop.TimeSinceLastPlay = 0f;
                                }
                            }
                        }));

                        var akObj = SoundCore.GetOrLoadEvent(loop.SoundName);
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