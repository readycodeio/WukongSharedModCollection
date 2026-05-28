using System;
using System.Collections.Generic;
using System.Text;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Sdk.Api;

namespace WukongMp.UtilityPatches
{
    public static class ChatCommandRegistry
    {
        private static readonly Dictionary<string, (string ConsoleCommand, int QuoteIndex)> _chatAliases
            = new(StringComparer.OrdinalIgnoreCase);

        public static readonly string chatTriggerPrefix = "/";

        public static void RegisterChatAlias(string chatTrigger, string consoleCommand, string modNameFallback, int quoteFromIndex = -1)
        {
            if (!chatTrigger.StartsWith(chatTriggerPrefix))
            {
                chatTrigger = chatTrigger.Insert(0, chatTriggerPrefix);
                Logging.LogWarning($"[UtilityPatches] Chat trigger '{chatTrigger}' did not start with '{chatTriggerPrefix}', automatically corrected.");
            }

            if (_chatAliases.ContainsKey(chatTrigger))
            {
                string cleanFallback = modNameFallback.Replace(".", "").Replace(" ", "");

                string fallbackTrigger = chatTrigger.Insert(1, $"{cleanFallback}:");

                if (_chatAliases.ContainsKey(fallbackTrigger))
                {
                    Logging.LogError($"[UtilityPatches] Both the original trigger '{chatTrigger}' and the fallback trigger '{fallbackTrigger}' are already taken. Alias registration for '{chatTrigger}' failed.");
                    return;
                }

                _chatAliases[fallbackTrigger] = (consoleCommand, quoteFromIndex);

                Logging.LogWarning($"[UtilityPatches] Alias '{chatTrigger}' is already taken, new alias generated: '{fallbackTrigger}'");
            }
            else
            {
                _chatAliases[chatTrigger] = (consoleCommand, quoteFromIndex);
            }
        }

        internal static bool TryGetAlias(string trigger, out (string ConsoleCmd, int QuoteIndex) config)
        {
            return _chatAliases.TryGetValue(trigger, out config);
        }

        public static void PrintRegister()
        {
            foreach (var kvp in _chatAliases)
            {
                WukongApi.Chat.ShowLocalMessage($"[UtilityPatches]'{kvp.Key}' -> '{kvp.Value.ConsoleCommand}'", FLinearColor.Gray);
            }
        }
    }

}
