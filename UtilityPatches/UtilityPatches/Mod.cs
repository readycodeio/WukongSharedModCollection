using HarmonyLib;
using ReadyM.Api.Command;
using ReadyM.Api.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnrealEngine.Plugins.PCG;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Sdk;
using WukongMp.Sdk.Api;

namespace WukongMp.UtilityPatches
{
    public class UtilityPatchesMod : ModBase
    {
        public override string Name => "Utility Patches";

        private const string ModId = "KralPolskaUtilityPatches";

        protected override void Initialize(IDependencyContainer services)
        {
            var harmony = new Harmony(ModId);

            UtilityPatches.ApplyAll(harmony);
            WukongApi.Console.AddCommand($"{ModId}PrintRegister", ConsoleCommand.Create(ChatCommandRegistry.PrintRegister));
            ChatCommandRegistry.RegisterChatAlias($"/UPhelp", $"{ModId}PrintRegister", ModId);
        }
    }

    public static class Helpers
    {
        public static void SanitizeTriggerString(ref string trigger)
        {
            string originalTrigger = trigger.Trim();
            string cleanedTrigger = originalTrigger.Replace(ChatCommandRegistry.chatTriggerPrefix, "");
            trigger = ChatCommandRegistry.chatTriggerPrefix + cleanedTrigger;
            if (!originalTrigger.Equals(trigger, StringComparison.OrdinalIgnoreCase))
            {
                Logging.LogWarning($"[UtilityPatches] Trigger '{originalTrigger}' was faulty. Sanitized to '{trigger}'.");
            }
        }
    }
}

