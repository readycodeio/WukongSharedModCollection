using HarmonyLib;
using ReadyM.Api.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Sdk.Api;

namespace WukongMp.UtilityPatches
{
    public static class UtilityPatches
    {
        public static string BannedChars = new string(System.IO.Path.GetInvalidFileNameChars()) + new string(System.IO.Path.GetInvalidPathChars());
        public static void ApplyAll(Harmony harmony)
        {
            try
            {
                Type chatApiType = WukongApi.Chat.GetType();

                FieldInfo chatterField = chatApiType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                    .FirstOrDefault(f => f.FieldType.Name == "WukongChatter");

                if (chatterField != null)
                {
                    Type chatterType = chatterField.FieldType;

                    MethodInfo originalSendMessage = chatterType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                    .FirstOrDefault(m => m.Name == "SendChatMessage" && m.GetParameters().Length == 3);
                    if (originalSendMessage != null)
                    {
                        MethodInfo prefixMatch = typeof(ChatInputPatch).GetMethod(nameof(ChatInputPatch.Prefix));
                        harmony.Patch(originalSendMessage, prefix: new HarmonyMethod(prefixMatch));

                        Logging.LogWarning("[UtilityPatches]: Patch connected");
                    }
                    else
                    {
                        Logging.LogError("[UtilityPatches]: SendChatMessage function not found");
                    }
                }
                else
                {
                    Logging.LogError("[UtilityPatches]: WukongChatter object not found in WukongChatApi");
                }
            }
            catch (Exception ex)
            {
                Logging.LogError($"[UtilityPatches]: Harmony error in ApplyAll: {ex}");
            }
        }

        public static void AddCommand(string commandName, ConsoleCommand command, IEnumerable<string>? avaiableFirstParams = null)
        {
            if (string.IsNullOrWhiteSpace(commandName))
            {
                Logging.LogWarning($"[UtilityPatches] Attempt to register a console command with a null or whitespace name was terminated.");
                return;
            }

            string sanitizedName = new string(commandName.Where(c => char.IsLetter(c)).ToArray());

            if (sanitizedName != commandName)
            {
                Logging.LogWarning($"[UtilityPatches] The command name '{commandName}' contained invalid characters. Changed to: '{sanitizedName}'.");
            }

            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                Logging.LogError($"[UtilityPatches] No valid characters remain in {sanitizedName}. The command will not be registered.");
                return;
            }

            WukongApi.Console.AddCommand(sanitizedName, command, avaiableFirstParams);
        }

        public static class ChatInputPatch
        {
            public static bool Prefix(string message)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(message)) return true;

                    if (message.StartsWith("/"))
                    {


                        string[] parts = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        string trigger = parts[0];

                        if (ChatCommandRegistry.TryGetAlias(trigger, out var config))
                        {
                            if (!WukongApi.Console.HasCommand(config.ConsoleCmd))
                            {
                                WukongApi.Chat.ShowLocalMessage($"[UtilityPatches] Missing command for {trigger}", FLinearColor.DarkRed);
                                return false;
                            }


                            string finalCommand = config.ConsoleCmd;

                            if (parts.Length > 1)
                            {
                                if (config.QuoteIndex != -1 && parts.Length > config.QuoteIndex)
                                {
                                    string prefixArgs = string.Join(" ", parts, 1, config.QuoteIndex - 1);
                                    string quotedSentence = string.Join(" ", parts, config.QuoteIndex, parts.Length - config.QuoteIndex);

                                    finalCommand = string.IsNullOrWhiteSpace(prefixArgs)
                                        ? $"{finalCommand} \"{quotedSentence}\""
                                        : $"{finalCommand} {prefixArgs} \"{quotedSentence}\"";
                                }
                                else
                                {
                                    string allArgs = string.Join(" ", parts, 1, parts.Length - 1);
                                    finalCommand = $"{finalCommand} {allArgs}";
                                }
                            }

                            object consoleApiInstance = WukongApi.Console;
                            Type consoleApiType = consoleApiInstance.GetType();

                            FieldInfo coreConsoleField = consoleApiType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                                .FirstOrDefault(f => f.FieldType.Name == "WukongCommandConsole");

                            if (coreConsoleField != null)
                            {
                                object coreConsoleInstance = coreConsoleField.GetValue(consoleApiInstance);
                                MethodInfo processCommandMethod = coreConsoleField.FieldType.GetMethod("ProcessCommand", BindingFlags.Public | BindingFlags.Instance);

                                if (processCommandMethod != null)
                                {
                                    processCommandMethod.Invoke(coreConsoleInstance, new object[] { finalCommand });
                                }

                            }

                            return false;
                        }
                        else
                        {
                            WukongApi.Chat.ShowLocalMessage($"[UtilityPatches] Unknown chat command: {trigger}", FLinearColor.DarkRed);
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.LogError($"[UtilityPatches] FATAL ERROR: {ex}");
                }

                return true;
            }
        }
    }
}
