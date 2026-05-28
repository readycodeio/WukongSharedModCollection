using CSharpModBase;
using ReadyM.Api.Command;
using System;
using System.Collections.Generic;
using System.Text;
using WukongMp.Sdk.Api;

namespace WukongMp.PrivateComs;

public static class Commands
{
    private const string CommandPrefix = Utils.CommandPrefix;

    public static void RegisterCommands()
    {
        //WukongApi.Console.AddCommand($"{CommandPrefix}msg",ConsoleCommand.Create(CommandSendPrivateMessage),CurrentPlayerNames.Values);
        WukongApi.Console.AddCommand($"{Utils.CommandPrefix}msg", ConsoleCommand.Create(CommandSendPrivateMessage), Utils.GetLazyPlayerNames());
        WukongApi.Console.AddCommand($"{Utils.CommandPrefix}message", ConsoleCommand.Create(CommandSendPrivateMessage), Utils.GetLazyPlayerNames());

        WukongApi.Console.AddCommand($"{Utils.CommandPrefix}tmsg", ConsoleCommand.Create(CommandSendTeamMessage));
        WukongApi.Console.AddCommand($"{Utils.CommandPrefix}teammessage", ConsoleCommand.Create(CommandSendTeamMessage));

        WukongApi.Console.AddCommand($"{Utils.CommandPrefix}reply", ConsoleCommand.Create(CommandReplyPrivateMessage));
        WukongApi.Console.AddCommand($"{Utils.CommandPrefix}re", ConsoleCommand.Create(CommandReplyPrivateMessage));

        WukongApi.Console.AddCommand($"{Utils.CommandPrefix}teamreply", ConsoleCommand.Create(CommandTeamReplyMessage));
        WukongApi.Console.AddCommand($"{Utils.CommandPrefix}tre", ConsoleCommand.Create(CommandTeamReplyMessage));

        WukongApi.Console.AddCommand($"{Utils.HelpCommandPrefix}", ConsoleCommand.Create(CommandDisplayHelp));

        WukongApi.Console.AddCommand($"{Utils.CommandPrefix}msgcolor", ConsoleCommand.Create(CommandChangePrivateMessageColor), Utils.ColorOptions.Keys);
        WukongApi.Console.AddCommand($"{Utils.CommandPrefix}tmsgcolor", ConsoleCommand.Create(CommandChangeTeamMessageColor), Utils.ColorOptions.Keys);
    }

    private static void CommandSendPrivateMessage(string receiver = null, string message = null)
    {
        if (string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(message))
        {
            WukongApi.Console.LogMessage($"Usage: {Utils.CommandPrefix}msg <playerName> <message>");
            return;
        }
        if (Mod.PlayerId is null || Mod.PlayerName is null)
        {
            WukongApi.Console.LogMessage("Player info not available.");
            return;
        }
        WukongApi.Chat.ShowLocalMessage($"[MSG] To {receiver}: {message}", Utils.PrivateMessageColor);
        Mod.RpcInstance?.SendPrivateMessage(receiver, message);

    }
    private static void CommandReplyPrivateMessage(string message = null)
    {
        if (string.IsNullOrEmpty(message))
        {
            WukongApi.Console.LogMessage($"Usage: {Utils.CommandPrefix}reply <message>");
            return;
        }
        if (Mod.RecentMessenger is null)
        {
            WukongApi.Console.LogMessage("No recent private message sender to reply to.");
            return;
        }
        var receiver = WukongApi.Sync.TryGetPlayerInfoById(Mod.RecentMessenger.Value, out string? playerName, out _);
        if (playerName is not null)
        {
            CommandSendPrivateMessage(playerName, message);
        }
    }

    private static void CommandSendTeamMessage(string message = null)
    {
        if (string.IsNullOrEmpty(message))
        {
            WukongApi.Console.LogMessage($"Usage: {Utils.CommandPrefix}teammsg <message>");
            return;
        }
        if (Mod.PlayerTeam is null)
        {
            WukongApi.Console.LogMessage("Player team info not available.");
            return;
        }
        WukongApi.Chat.ShowLocalMessage($"[TEAM] To Team: {message}", Utils.TeamMessageColor);
        Mod.RpcInstance?.SendTeamMessage(Mod.PlayerTeam.Value, message);
    }

    private static void CommandTeamReplyMessage(string message = null)
    {
        if (string.IsNullOrEmpty(message))
        {
            WukongApi.Console.LogMessage($"Usage: {Utils.CommandPrefix}teamreply <message>");
            return;
        }
        if (Mod.PlayerTeam is null)
        {
            WukongApi.Console.LogMessage("Player team info not available.");
            return;
        }
        if (Mod.RecentTeamMessenger is null)
        {
            WukongApi.Console.LogMessage("No recent team message sender to reply to.");
            return;
        }
        CommandSendPrivateMessage(Helpers.GetPlayerInfo(Mod.RecentTeamMessenger.Value).Name, message);
    }

    private static void CommandChangePrivateMessageColor(string? colorName = null)
    {
        if (string.IsNullOrEmpty(colorName))
        {
            WukongApi.Console.LogMessage("No color specified. Available colors: " + string.Join(", ", Utils.ColorOptions.Keys));
            return;
        }

        if (!Utils.ColorOptions.ContainsKey(colorName.Trim().ToLower()))
        {
            WukongApi.Console.LogMessage($"Unknown color: {colorName}. Available colors: " + string.Join(", ", Utils.ColorOptions.Keys));
            return;
        }
        Utils.PrivateMessageColor = Utils.ColorOptions[colorName.Trim().ToLower()];
        WukongApi.Console.LogMessage($"Private message color changed to {colorName}.");
    }
    private static void CommandChangeTeamMessageColor(string? colorName = null)
    {
        if (string.IsNullOrEmpty(colorName))
        {
            WukongApi.Console.LogMessage("No color specified. Available colors: " + string.Join(", ", Utils.ColorOptions.Keys));
            return;
        }

        if (!Utils.ColorOptions.ContainsKey(colorName.Trim().ToLower()))
        {
            WukongApi.Console.LogMessage($"Unknown color: {colorName}. Available colors: " + string.Join(", ", Utils.ColorOptions.Keys));
            return;
        }
        Utils.TeamMessageColor = Utils.ColorOptions[colorName.Trim().ToLower()];
        WukongApi.Console.LogMessage($"Team message color changed to {colorName}.");
    }
    private static void CommandDisplayHelp(string command = null)
    {
        if (string.IsNullOrEmpty(command))
        {
            WukongApi.Console.LogMessage($"Available commands:");
            WukongApi.Console.LogMessage($"{CommandPrefix}message <playerName> <message> - Send a private message to a player.");
            WukongApi.Console.LogMessage($"{CommandPrefix}teammessage <message> - Send a private message to your team.");
            WukongApi.Console.LogMessage($"{CommandPrefix}reply <message> - Reply to the most recent private message sender.");
            WukongApi.Console.LogMessage($"{CommandPrefix}teamreply <message> - Reply to the most recent team message sender as private message.");
            WukongApi.Console.LogMessage($"{CommandPrefix}msgcolor <colorName> - Change the color of private messages.");
            WukongApi.Console.LogMessage($"{CommandPrefix}teammsgcolor <colorName> - Change the color of team messages.");
            return;
        }
        command = command.ToLower();
        switch (command)
        {
            case "msg":
            case "message":
                WukongApi.Console.LogMessage($"{CommandPrefix}msg|message <playerName> <message>");
                WukongApi.Console.LogMessage($"{CommandPrefix}Send a private message to a player.");
                WukongApi.Console.LogMessage($"{CommandPrefix}You can use \"{CommandPrefix}msg\" for short.");
                break;
            case "re":
            case "reply":
                WukongApi.Console.LogMessage($"{CommandPrefix}re|reply <message>");
                WukongApi.Console.LogMessage($"{CommandPrefix}Reply to the most recent private message sender.");
                WukongApi.Console.LogMessage($"{CommandPrefix}You can use \"{CommandPrefix}re\" for short.");
                WukongApi.Console.LogMessage($"{CommandPrefix}Note: This is unreliable for consistent communication with one person, as the recent messenger may change just before you send a message.");
                WukongApi.Console.LogMessage($"{CommandPrefix}Note: DO NOT SHARE SENSITIVE DATA OVER REPLY COMMAND! USE PRIVATE MESSAGE INSTEAD.");
                break;
            case "tmsg":
            case "teammessage":
                WukongApi.Console.LogMessage($"{CommandPrefix}tmsg|teammsg <message>");
                WukongApi.Console.LogMessage($"{CommandPrefix}Send a private message to your team.");
                WukongApi.Console.LogMessage($"{CommandPrefix}You can use \"{CommandPrefix}tmsg\" for short.");
                break;
            case "tre":
            case "teamreply":
                WukongApi.Console.LogMessage($"{CommandPrefix}tre|teamreply <message>");
                WukongApi.Console.LogMessage($"{CommandPrefix}Reply to the most recent team message sender as private message.");
                WukongApi.Console.LogMessage($"{CommandPrefix}You can use \"{CommandPrefix}tre\" for short.");
                WukongApi.Console.LogMessage($"{CommandPrefix}Note: This is unreliable for consistent communication with one person, as the recent messenger may change just before you send a message.");
                WukongApi.Console.LogMessage($"{CommandPrefix}Note: DO NOT SHARE SENSITIVE DATA OVER REPLY COMMAND! USE PRIVATE MESSAGE INSTEAD.");
                break;
            case "msgcolor":
                WukongApi.Console.LogMessage($"{CommandPrefix}msgcolor <colorName>");
                WukongApi.Console.LogMessage($"{CommandPrefix}Change the color of private messages. Available colors: " + string.Join(", ", Utils.ColorOptions.Keys));
                break;
            case "tmsgcolor":
                WukongApi.Console.LogMessage($"{CommandPrefix}teammsgcolor <colorName>");
                WukongApi.Console.LogMessage($"{CommandPrefix}Change the color of team messages. Available colors: " + string.Join(", ", Utils.ColorOptions.Keys));
                break;
            default:
                WukongApi.Console.LogMessage($"Unknown command: {command}");
                break;
        }
    }
}
