using b1;
using HarmonyLib;
using ReadyM.Api.DI;
using System;
using WukongMp.Api;
using WukongMp.Sdk;
using WukongMp.Sdk.Api;
using WukongMp.Toolkit.Actors;
using WukongMp.Toolkit.Debug;
using WukongMp.Toolkit.Debug.Sync;
using WukongMp.Toolkit.Levels;

namespace WukongMp.Toolkit.Audio;

public class Mod : ModBase
{
    public override string Name => "BasicToolkit";

    private static AudioSystem? audioSystem;
    private static AudioService? audioService;
    public static AudioRpc? audioRpc;

    private static LevelsSystem? levelsSystem;
    public static LevelsRpc? levelsRpc;

    public static ActorsRpc? actorsRpc;

    protected override void Initialize(IDependencyContainer services)
    {
        services.RegisterSingleton<AudioSystem>();
        services.RegisterSingleton<AudioService>();
        services.RegisterSingleton<AudioRpc>();
        audioSystem = services.Resolve<AudioSystem>();
        audioService = services.Resolve<AudioService>();
        audioRpc = services.Resolve<AudioRpc>();

        services.RegisterSingleton<LevelsSystem>();
        services.RegisterSingleton<LevelsRpc>();
        levelsSystem = services.Resolve<LevelsSystem>();
        levelsRpc = services.Resolve<LevelsRpc>();

        services.RegisterSingleton<ActorsRpc>();
        actorsRpc = services.Resolve<ActorsRpc>();

        try
        {
            var harmony = new Harmony("WukongMp.Toolkit.Patches");
            var original = AccessTools.Method(typeof(BUI_BattleInfoCS), "InitBloodBarUI");
            var myPrefix = AccessTools.Method(typeof(Patches.PatchInitBloodBarUI), "Prefix");
            var myPostfix = AccessTools.Method(typeof(Patches.PatchInitBloodBarUI), "Postfix");
            harmony.Patch(original, prefix: new HarmonyMethod(myPrefix), postfix: new HarmonyMethod(myPostfix));
        }
        catch (Exception ex)
        {
            Logging.LogException(ex);
        }
#if DEBUG
        Debug.Test.Testing();
        Debug.Sync.DebugSync.SyncTest();
#endif
    }
}

