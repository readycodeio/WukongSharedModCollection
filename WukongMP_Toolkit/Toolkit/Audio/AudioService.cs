using ReadyM.Api.DI;

namespace WukongMp.Toolkit.Audio;

public sealed class AudioService : IHostedService
{
    public void OnScopeStart()
    {
        lock (AudioCore.LocationLoops) { AudioCore.LocationLoops.Clear(); }
        lock (AudioCore.FlatLoops) { AudioCore.FlatLoops.Clear(); }
    }

    public void Dispose()
    {
        AudioCore.Spatial.StopAllSounds();
        AudioCore.Flat.StopAllSounds();
        AudioCore.ClearCache();
    }
}