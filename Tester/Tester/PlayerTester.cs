using CSharpModBase.Input;
using HarmonyLib;
using ReadyM.Api.Command;
using ReadyM.Api.DI;
using ReadyM.Api.Idents;
using ReadyM.Api.Multiplayer.Client;
using ReadyM.Api.Multiplayer.Generators;
using ReadyM.Api.Multiplayer.Protocol.Enums;
using ReadyM.Api.Multiplayer.RPC;
using ReadyM.Api.Multiplayer.Serialization;
using System;
using System.Linq;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.Configuration;
using WukongMp.Sdk;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;
namespace Tester;

public static class PlayerTester
{
    public static ReadyMainCharacter? MyCharacter;

    public static void invoke()
    {
        try
        {
            MyCharacter = WukongApi.Sync.LocalMainCharacter;

            WukongApi.Input.RegisterKeyBind(Key.F2, test);
            WukongApi.Input.RegisterKeyBind(ModifierKeys.Shift, Key.F2, testOther);
            WukongApi.Input.RegisterKeyBind(Key.F3, moveTest);
            WukongApi.Input.RegisterKeyBind(ModifierKeys.Shift, Key.F3, moveOther);
            WukongApi.Input.RegisterKeyBind(Key.F4, spinTest);
            WukongApi.Input.RegisterKeyBind(ModifierKeys.Shift, Key.F4, spinOther);

            WukongApi.Input.RegisterKeyBind(Key.F6, teleportTest);
            WukongApi.Input.RegisterKeyBind(ModifierKeys.Shift, Key.F6, teleportOther);

        }
        catch (Exception ex)
        {
            Logging.LogError($"[Tester] ERROR w invoke(): {ex}");
        }
    }

    public static void testOther()
    {
        try
        {
            var characterNullable = WukongApi.Sync.AllMainCharacters
                .Cast<ReadyMainCharacter?>()
                .FirstOrDefault(c => c.Value.PlayerId != Mod.PlayerId);

            if (!characterNullable.HasValue)
            {
                WukongApi.Chat.ShowLocalMessage("No other characters found", FLinearColor.Red);
                return;
            }

            var otherCharacter = characterNullable.Value;
            otherCharacter.Hp -= 50;

            WukongApi.Chat.ShowLocalMessage($"{otherCharacter.Hp} {otherCharacter.HpMaxBase}", FLinearColor.Red);
        }
        catch (Exception ex)
        {
            Logging.LogError($"[Tester] ERROR w testOther(): {ex}");
        }
    }

    public static void test()
    {
        try
        {
            var character = WukongApi.Sync.LocalMainCharacter.Value;
            character.Hp -= 50;

            WukongApi.Chat.ShowLocalMessage($"{character.Hp} {character.HpMaxBase}", FLinearColor.Red);
        }
        catch (Exception ex)
        {
            Logging.LogError($"[Tester] ERROR w test(): {ex}");
        }
    }
    public static void moveOther()
    {
        try
        {
            var characterNullable = WukongApi.Sync.AllMainCharacters
                .Cast<ReadyMainCharacter?>()
                .FirstOrDefault(c => c.Value.PlayerId != Mod.PlayerId);

            if (!characterNullable.HasValue)
            {
                WukongApi.Chat.ShowLocalMessage("No other characters found", FLinearColor.Red);
                return;
            }

            var otherCharacter = characterNullable.Value;
            var oldLocation = otherCharacter.Location;
            var newLocation = otherCharacter.Location;
            newLocation = new(otherCharacter.Location.X, otherCharacter.Location.Y, otherCharacter.Location.Z + 2000);

            otherCharacter.SetLocationRotation(newLocation, otherCharacter.Rotation);

            WukongApi.Chat.ShowLocalMessage($"Moved {otherCharacter.Nickname} to: {newLocation.Z} from: {oldLocation.Z}", FLinearColor.Red);
        }
        catch (Exception ex)
        {
            Logging.LogError($"[Tester] ERROR w moveOther(): {ex}");
        }
    }


    public static void moveTest()
    {
        try
        {
            var character = WukongApi.Sync.LocalMainCharacter.Value;
            var oldLocation = character.Location;
            var newLocation = character.Location;
            newLocation = new(character.Location.X, character.Location.Y, character.Location.Z + 2000);

            character.SetLocationRotation(newLocation, character.Rotation);

            WukongApi.Chat.ShowLocalMessage($"{oldLocation.Z} {character.Location.Z}", FLinearColor.Red);
        }
        catch (Exception ex)
        {
            Logging.LogError($"[Tester] ERROR w moveTest(): {ex}");
        }
    }

    public static void spinTest()
    {
        try
        {
            var character = WukongApi.Sync.LocalMainCharacter.Value;
            var newRotation = character.Rotation;
            newRotation = new(character.Rotation.X + 1, character.Rotation.Y + 1, character.Rotation.Z + 1);

            character.SetLocationRotation(character.Location, newRotation);
        }
        catch (Exception ex)
        {
            Logging.LogError($"[Tester] ERROR w spinTest(): {ex}");
        }
    }

    public static void spinOther()
    {
        try
        {
            var characterNullable = WukongApi.Sync.AllMainCharacters
                .Cast<ReadyMainCharacter?>()
                .FirstOrDefault(c => c.Value.PlayerId != Mod.PlayerId);

            if (!characterNullable.HasValue)
            {
                WukongApi.Chat.ShowLocalMessage("No other characters found", FLinearColor.Red);
                return;
            }

            var otherCharacter = characterNullable.Value;
            var newRotation = otherCharacter.Rotation;
            newRotation = new(otherCharacter.Rotation.X + 1, otherCharacter.Rotation.Y + 1, otherCharacter.Rotation.Z + 1);

            otherCharacter.SetLocationRotation(otherCharacter.Location, newRotation);
        }
        catch (Exception ex)
        {
            Logging.LogError($"[Tester] ERROR w spinOther(): {ex}");
        }
    }

    public static void teleportTest()
    {
        try
        {
            var character = WukongApi.Sync.LocalMainCharacter.Value;
            var newLocation = character.Location;
            newLocation = new(character.Location.X + 100, character.Location.Y, character.Location.Z + 500);
            var newRotation = character.Rotation;
            newRotation = new(character.Rotation.X + 10, character.Rotation.Y + 10, character.Rotation.Z);

            character.Teleport(newLocation, newRotation);
        }
        catch (Exception ex)
        {
            Logging.LogError($"[Tester] ERROR w teleportTest(): {ex}");
        }
    }

    public static void teleportOther()
    {
        try
        {
            var characterNullable = WukongApi.Sync.AllMainCharacters
                .Cast<ReadyMainCharacter?>()
                .FirstOrDefault(c => c.Value.PlayerId != Mod.PlayerId);

            if (!characterNullable.HasValue)
            {
                WukongApi.Chat.ShowLocalMessage("No other characters found", FLinearColor.Red);
                return;
            }


            var otherCharacter = characterNullable.Value;

            ReadyMainCharacter myChar = (ReadyMainCharacter)WukongApi.Sync.LocalMainCharacter;

            WukongApi.Sync.TryGetPlayerInfoById(otherCharacter.PlayerId, out var nickname, out _);
            WukongApi.Chat.ShowLocalMessage($"Teleportowanie {nickname}", FLinearColor.Red);
            var newLocation = otherCharacter.Location;
            newLocation = new(otherCharacter.Location.X + 100, otherCharacter.Location.Y, otherCharacter.Location.Z + 500);
            var newRotation = otherCharacter.Rotation;
            newRotation = new(otherCharacter.Rotation.X + 10, otherCharacter.Rotation.Y + 10, otherCharacter.Rotation.Z);

            otherCharacter.Teleport(newLocation, newRotation);
        }
        catch (Exception ex)
        {
            Logging.LogError($"[Tester] ERROR w teleportOther(): {ex}");
        }
    }
}