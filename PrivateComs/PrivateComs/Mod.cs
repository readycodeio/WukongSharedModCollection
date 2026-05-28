using CSharpModBase.Input;
using ReadyM.Api.Command;
using ReadyM.Api.DI;
using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Client;
using ReadyM.Api.Multiplayer.Generators;
using ReadyM.Api.Multiplayer.Protocol.Enums;
using ReadyM.Api.Multiplayer.RPC;
using ReadyM.Api.Multiplayer.Serialization;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.Configuration;
using WukongMp.Sdk;
using WukongMp.Sdk.Api;
using HarmonyLib;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Linq;
using System;
using UnrealEngine.UMG;
using WukongMp.Sdk.Entities;
using UnrealEngine.GeometryFramework;

namespace WukongMp.PrivateComs;

public class Mod : ModBase
{
    public override string Name => "PrivateComs";
    //public override string Version => "1.0.0";


    public static string CommandFallbackPrefix = $"PrivateComs".Trim();

    public static PrivateComsRpc? RpcInstance = null;

    public static PlayerId? PlayerId;
    public static string? PlayerName;
    public static int? PlayerTeam;

    public static PlayerId? RecentMessenger;
    public static PlayerId? RecentTeamMessenger;

    protected override void Initialize(IDependencyContainer services)
    {
        // register and resolve your services here, for example:
        services.RegisterSingleton<PrivateComsRpc>();
        var rpc = services.Resolve<PrivateComsRpc>();
        Mod.RpcInstance = rpc;
        Utils.RegisterAll();
    }
}

public static class Utils
{
    public const string CommandPrefix = "";
    public static string ChatCommandPrefix = "/".Trim();
    public static string HelpCommandPrefix = $"{CommandPrefix}help".Trim();
    public static Dictionary<PlayerId, string> CurrentPlayerNames = new Dictionary<PlayerId, string>();

    public static Dictionary<string, FLinearColor> ColorOptions = new Dictionary<string, FLinearColor>
    {
        { "red", FLinearColor.Red },
        { "green", FLinearColor.DarkGreen },
        { "blue", FLinearColor.Blue },
        { "yellow", FLinearColor.Yellow },
        { "cyan", FLinearColor.Cyan },
        { "magenta", FLinearColor.Magenta },
        { "orange", FLinearColor.Orange },
        { "purple", FLinearColor.Purple },
        { "white", FLinearColor.White },
        { "black", FLinearColor.Black },
        { "gray", FLinearColor.Gray }
    };
    public static FLinearColor PrivateMessageColor = FLinearColor.Gray;
    public static FLinearColor TeamMessageColor = ColorOptions["green"];

    public static readonly string WelcomeMessage = $"Using PrivateComs mod. Type {HelpCommandPrefix} for available commands.";

    public static void RegisterAll()
    {
        Commands.RegisterCommands();
        KeyBinds.RegisterKeyBinds();
        RegisterCommandsToChat();

        WukongApi.Console.AddCommand("test123",ConsoleCommand.Create(() => { WukongApi.Chat.ShowLocalMessage("Test message with digits", FLinearColor.White); }));
        WukongApi.Console.AddCommand("123",ConsoleCommand.Create(() => { WukongApi.Chat.ShowLocalMessage("Test message only digits", FLinearColor.White); }));
        WukongApi.Console.AddCommand("test.test",ConsoleCommand.Create(() => { WukongApi.Chat.ShowLocalMessage("Test message with dots", FLinearColor.White); }));
        WukongApi.Console.AddCommand("test!@#",ConsoleCommand.Create(() => { WukongApi.Chat.ShowLocalMessage("Test message with special characters", FLinearColor.White); }));
        WukongApi.Console.AddCommand("test ",ConsoleCommand.Create(() => { WukongApi.Chat.ShowLocalMessage("Test message with space", FLinearColor.White); }));
    }

    public static void RegisterCommandsToChat()
    {
        UtilityPatches.ChatCommandRegistry.RegisterChatAlias($"{ChatCommandPrefix}msg", "message", $"{Mod.CommandFallbackPrefix}", 2);
        UtilityPatches.ChatCommandRegistry.RegisterChatAlias($"{ChatCommandPrefix}msg", "message", $"{Mod.CommandFallbackPrefix}", 2);
        UtilityPatches.ChatCommandRegistry.RegisterChatAlias($"{ChatCommandPrefix}tmsg", "teammessage", $"{Mod.CommandFallbackPrefix}", 1);
        UtilityPatches.ChatCommandRegistry.RegisterChatAlias($"{ChatCommandPrefix}re", "reply", $"{Mod.CommandFallbackPrefix}", 1);
    }
    //public static void UpdateMsgAutocomplete()
    //{
    //    List<string> currentNames = new List<string>();

    //    foreach (var player in WukongApi.Sync.AllPlayers.ToList())
    //    {
    //        WukongApi.Sync.TryGetPlayerInfoById(player, out string? playerName, out _);
    //        if (playerName is not null)
    //        {
    //            currentNames.Add(playerName);
    //        }
    //    }

    //    try
    //    {
    //        WukongApi.Console.AddCommand(
    //            $"{CommandPrefix}msg",
    //            ConsoleCommand.Create(CommandSendPrivateMessage),
    //            currentNames
    //        );
    //    }
    //    catch (Exception ex)
    //    {
    //        WukongApi.Console.LogMessage($"Error updating command autocomplete: {ex.Message}");
    //    }
    //}

    //private static void UpdatePlayerList()
    //{
        
    //    CurrentPlayerNames.Clear();
    //    WukongApi.Chat.ShowLocalMessage(string.Join(", ", WukongApi.Sync.AllPlayers.ToList()), FLinearColor.Blue);
    //    foreach (var player in WukongApi.Sync.AllPlayers.ToList())
    //    {
    //        WukongApi.Sync.TryGetPlayerInfoById(player, out string? playerName, out _);
    //        if (playerName is not null)
    //        {
    //            CurrentPlayerNames.Add(player, playerName);
    //        }
    //    }
    //}
    public static IEnumerable<string> GetLazyPlayerNames()
    {
        
        foreach (var player in WukongApi.Sync.AllPlayers.ToList())
        {
            WukongApi.Sync.TryGetPlayerInfoById(player, out string? playerName, out _);
            if (playerName is not null)
            {
                //yield return $"test {DateTime.Now.Second}";
                yield return playerName;
            }
        }
    }
}

public static class Helpers
{
    public struct PlayerInfo
    {
        public string Name;
        public int Team;
    }

    public static void Log(string message)
    {
        WukongApi.Console.LogMessage($"[PrivateComs] {message}");
    }
    public static PlayerInfo GetPlayerInfo(PlayerId playerId)
    {
        if (WukongApi.Sync.TryGetPlayerInfoById(playerId, out string? name, out int? team))
        {
            return new PlayerInfo { Name = name ?? "Unknown", Team = team ?? -1 };
        }
        return new PlayerInfo { Name = "Unknown", Team = -1 };
    }
}
