using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api.Configuration;
using WukongMp.Sdk.Api;

namespace WukongMp.PrivateComs;

// use Harmony to patch a game method, for example:
[HarmonyPatch(typeof(UGameplayStatics), nameof(UGameplayStatics.OpenLevel))]
[HarmonyPatchCategory(PatchCategory.Global)]
public static class ExamplePatch
{
    public static void Postfix(FName LevelName)
    {
        Mod.PlayerId = WukongApi.Sync.LocalPlayerId;
        if (Mod.PlayerId is null)
            return;

        WukongApi.Sync.TryGetPlayerInfoById(Mod.PlayerId.Value, out Mod.PlayerName, out Mod.PlayerTeam);

        //Logging.LogDebug("Entering level: {LevelName}", LevelName.ToString());
    }
}
