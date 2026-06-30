using b1;
using ReadyM.Api.Command;
using System;
using System.IO;
using System.Text;
using UnrealEngine.AssetRegistry;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using UnrealEngine.UMG;
using WukongMp.Api;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;
using WukongMp.Toolkit.Actors;
using WukongMp.Toolkit.Audio;

namespace WukongMp.Toolkit.Debug;


public static class Test
{
    public static AActor? chest = null;

    public static void Testing()
    {

        WukongApi.Console.AddCommand("actortest", ConsoleCommand.Create(() =>
        {
            var player = WukongApi.Sync.LocalMainCharacter.Value.Pawn;
            var location = player.GetActorLocation();
            var rotation = player.GetActorRotation();

            //Short object name only works if class is stream loaded with level

            //if (Actors.Actors.SpawnActorAtLocation("HFS_Destructible_ShuiGang_C", location.Add_VectorVector(new FVector(500, 0, 500)), rotation) == null)
            if (Actors.Actors.SpawnActorAtLocation(Assets.Destructible.HFS_Destructible_ShuiGang, location.Add_VectorVector(new FVector(500, 0, 500)), rotation) == null)
            {
                WukongApi.Chat.ShowLocalMessage($"Failed to spawn prop at location", FLinearColor.Red);
            }

            //var actor = Actors.Actors.SpawnActor("HFS_Destructible_HuoPen_C");
            var actor = Actors.Actors.SpawnActor(Assets.Destructible.HFS_Destructible_HuoPen);
            if (actor == null)
            {
                WukongApi.Chat.ShowLocalMessage($"Failed to spawn prop", FLinearColor.Red);
            }
            actor?.SetActorLocationAndRotation(location.Add_VectorVector(new FVector(500, 500, 500)), rotation, false, out _, true);

            //if (Actors.Actors.SpawnActorAtFloor("BPO_TreasureBox_JiaSi_07a_C", location.Add_VectorVector(new FVector(0, 500, 500)), rotation) == null)
            if (Actors.Actors.SpawnActorAtFloor(Assets.Chests.BPO_TreasureBox_Coffin_02, location.Add_VectorVector(new FVector(0, 500, 500)), rotation) == null)
            {
                WukongApi.Chat.ShowLocalMessage($"Failed to spawn prop at floor", FLinearColor.Red);
            }
        }));

        WukongApi.Console.AddCommand("audiotest", ConsoleCommand.Create(() =>
        {
            //AkAudioEvent'/Game/00Main/Audio/SFX/Environment/BPO/ENV_Position_BPO_Stick.ENV_Position_BPO_Stick'
            AudioCore.RegisterSound("Stick", "AkAudioEvent'/Game/00Main/Audio/SFX/Environment/BPO/ENV_Position_BPO_Stick.ENV_Position_BPO_Stick'");
            AudioCore.PlaySound("Stick", location: WukongApi.Sync.LocalMainCharacter.Value.Location.ToFVector());
        }));

        WukongApi.Console.AddCommand("hptest", ConsoleCommand.Create((int state) =>
        {
            ESlateVisibility eState = (ESlateVisibility)(state % 5);
            Widgets.WidgetsHpBars.SetHpBarsState(eState);
        }));

        WukongApi.Console.AddCommand("levelstest", ConsoleCommand.Create((string level) =>
        {
            Levels.LevelsCore.LoadLevel(level);
        }));

        WukongApi.Console.AddCommand("shrinetest", ConsoleCommand.Create((int shrine) =>
        {
            Levels.LevelsCore.TeleportToShrine(shrine);
        }));

        WukongApi.Console.AddCommand("dumpAssets", ConsoleCommand.Create(() =>
        {
            var reg = UAssetRegistryHelpers.GetAssetRegistry();
            reg.GetAssetsByPath(new FName("/Game/00MainHZ/Environment/BPO/"), out var list, bRecursive: true);

            reg.GetAssetsByClass(new FName("BPO_TreasureBox_04a"), out var chestlist, true);

            reg.GetAssetsByPackageName(new FName("BPO_TreasureBox_04a"), out var chestlist2, true);

            StringBuilder sb = new();
            var desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Wukong_AssetDump.txt");

            //foreach (var asset in list)
            //{
            //    sb.AppendLine(/*asset.AssetName.ToString() + " " + asset.PackagePath.ToString() + */asset.PackageName.ToString());
            //}

            foreach (var chest in chestlist)
            {
                sb.Append(chest.PackageName);
            }

            foreach (var chest in chestlist2)
            {
                sb.Append(chest.PackageName);
            }

            File.WriteAllText(desktopPath, sb.ToString());
        }));

        WukongApi.Console.AddCommand("chestlootTest", ConsoleCommand.Create(() =>
        {
            var character = WukongApi.Sync.LocalMainCharacter.Value;

            var chestClass = UClass.LoadClass<AActor>(default, Assets.Chests.BPO_TreasureBox_04a);

            chest = Actors.Actors.SpawnActorAtLocation(Assets.Chests.BPO_TreasureBox_04a, character.Location.ToFVector(), character.Rotation.ToFRotator());

            //var comp = GetComponent<BUS_DropItemComp>(actor); 

        }));

        WukongApi.Console.AddCommand("snap", ConsoleCommand.Create(() =>
        {
            if (chest == null)
                return;

            float z = (float)chest.GetActorLocation().Z;
            chest.SnapToFloor(10000, out var newLocation);

            WukongApi.Chat.ShowLocalMessage($"Diff: {Math.Abs(newLocation.Z - z)}", FLinearColor.Beige);

        }));

        WukongApi.Console.AddCommand("chestActivate", ConsoleCommand.Create((int option) =>
        {
            try
            {
                if (chest == null)
                {
                    WukongApi.Chat.ShowLocalMessage("Chest does not exist", FLinearColor.Red);
                    return;
                }

                switch (option)
                {
                    case 0:
                        var drops = chest.GetComponentByClass<BUS_DropItemComp>() as BUS_DropItemComp;
                        drops.Activate();
                        WukongApi.Chat.SendServerMessage("drop");
                        break;
                    case 1:
                        var interaction = chest.GetComponentByClass<BUS_InteractComp>() as BUS_InteractComp;
                        interaction.Activate();
                        WukongApi.Chat.SendServerMessage("interaction");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
            }
        }));
    }
}