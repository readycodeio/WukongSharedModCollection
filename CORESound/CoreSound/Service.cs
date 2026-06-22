using ReadyM.Api.DI;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreSound;
public sealed class SoundCoreService : IHostedService
{
    public void OnScopeStart()
    {
        lock (SoundCore.LocationLoops) { SoundCore.LocationLoops.Clear(); }
        lock (SoundCore.FlatLoops) { SoundCore.FlatLoops.Clear(); }
    }

    public void Dispose()
    {
        SoundCore.Spatial.StopAllSounds();
        SoundCore.Flat.StopAllSounds();
        SoundCore.ClearCache();
    }
}