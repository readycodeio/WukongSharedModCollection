using b1;
using HarmonyLib;
using ReadyM.Api.Idents;
using ReadyM.Wukong.Common.ECS.Components;
using ReadyM.Wukong.Common.ECS.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnrealEngine.AIModule;
using UnrealEngine.AssetRegistry;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using UnrealEngine.UMG;
using WukongMp.Api;
using WukongMp.Api.WukongUtils;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace WukongMp.PropHunt;

public static class Utils
{
    public static AActor? GetMyPlayer()
    {
        AActor? myActor = null;
        if (WukongApi.Sync.LocalMainCharacter is not { } localCharacter) return null;
        myActor = GetCharacterActor(localCharacter);
        return myActor;
    }

    public static string[] propClassNames = new string[]
    {
            //"BP_yaocai_biou_C",
            //"BP_yaocai_gancao_C",
            //"BP_Yaocai_MuJinHua_C",
            //"BP_yaocai_tujun_C",
            //"BPO_TreasureBox_03a_C",
            "BPO_TreasureBox_04a_C",
            "BPO_TreasureBox_06_C",
            "BPO_TreasureBox_07a_C",
            "BPO_TreasureBox_08_C",
            "BPO_TreasureBox_Coffin_01_C",
            //"BPO_TreasureBox_fsc_C",
            "BPO_TreasureBox_JiaSi_07a_C",
            //"HFS_Destructible_HuoPen_C",
            //"HFS_Destructible_ShuiGang_C",
            //"HFS_Destructible_ShuiGang_droppable_C",
            //"HFS_Tudipo_Yun_C"
    };

    public static AActor? SpawnProp(string propClassName, FVector location, FRotator rotation, AActor actorToIgnore, bool enableCollision = false)
    {
        var startLocation = location;
        startLocation.Z += 100;
        var endLocation = location;
        endLocation.Z -= 1000;

        var newLocation = location;
        //actorToIgnore.GetActorBounds(true, out FVector origin, out FVector boxExtent, false);

        //newLocation.Z-=110;

        List<AActor> actorsToIgnore = new List<AActor> { actorToIgnore };

        bool bhit = Mod.World.LineTraceSingle(
            startLocation, endLocation,
            ETraceTypeQuery.TraceTypeQuery2,
            false,
            actorsToIgnore,
            EDrawDebugTrace.ForDuration,
            out FHitResult hitResult,
            true,
            FLinearColor.Red,
            FLinearColor.Green,
            5f
            );

        if (bhit)
        {
            newLocation = new FVector(hitResult.Location.X, hitResult.Location.Y, hitResult.Location.Z);
        }
        else
        {
            newLocation.Z -= 122;
        }

        AActor? prop = SpawnActor(propClassName, newLocation, rotation);
        if (prop == null)
        {
            WukongApi.Chat.ShowLocalMessage("Failed to spawn prop: " + propClassName, FLinearColor.Red);
        }
        prop?.SetActorHiddenInGame(false);
        prop?.SetActorEnableCollision(enableCollision);

        WukongApi.Chat.ShowLocalMessage($"Spawning prop at: {newLocation.Z} | Player.Z: {location.Z}",FLinearColor.Yellow);
        prop?.AttachToActor(actorToIgnore, new FName(""), EAttachmentRule.KeepWorld, EAttachmentRule.KeepWorld, EAttachmentRule.KeepWorld, false);
        WukongApi.Chat.ShowLocalMessage($"After attachment: {prop?.GetActorLocation().Z}", FLinearColor.Yellow);
        return prop;
    }

    public static AActor? SpawnActor(string actorClassName, FVector location, FRotator rotation)
    {
        AActor? prop = null;

        UClass actorClass = UClass.GetClass(actorClassName);
        if (actorClass == null)
        {
            WukongApi.Chat.ShowLocalMessage("No actor class named: " + actorClassName, FLinearColor.Red);
            return null;
        }

        if (Mod.World == null)
        {
            Mod.World = GameUtils.GetWorld();

            if (Mod.World == null)
            {
                WukongApi.Chat.ShowLocalMessage("No world found", FLinearColor.Red);
                return null;
            }
        }

        prop = Mod.World.SpawnActor(actorClass, ref location, ref rotation);
        return prop;
    }

    public static AActor? GetCharacterActor(ReadyMainCharacter character)
    {
        Type charType = character.GetType();

        try
        {
            PropertyInfo? entityProperties = charType.GetProperty("Entity", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (entityProperties != null)
            {
                object? entityObj = entityProperties.GetValue(character);
                if (entityObj != null)
                {
                    foreach (PropertyInfo property in entityObj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        try
                        {
                            if (property.GetValue(entityObj) is AActor actor) return actor;
                        }
                        catch (Exception ex)
                        {
                            Logging.LogError(ex.Message);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logging.LogError(ex.Message);
        }
        return null;
    }

    //public static void test()
    //{
    //    AActor actor = new AActor();
    //    var pawn = WukongApi.Sync.LocalMainCharacter!.Value.Pawn;

    //    actor.DisableInput(GameUtils.GetPlayerController());

    //    //UTextBlock textBlock = new UTextBlock();
    //    //textBlock.SetText("abc");

    //    UGridPanel panel = new UGridPanel();

    //    BUS_PlayerInputActionComp bus = new BUS_PlayerInputActionComp();

    //    USoundBase sound = new();
    //    AIController controller = new AIController();
    //    BGU_CharacterAI ai = new BGU_CharacterAI();
    //    UGameplayStatics.GetObjects<USoundBase>(EObjectFlags.NoFlags, true, EInternalObjectFlags.None).Current.GetType().Name.ToLower();
    //    var registry = UAssetRegistryHelpers.GetAssetRegistry();

    //    //var myPawn = GameUtils.GetControlledPawn();
    //    //var playerComponent = EcsExtensions.GetComponent<MainCharacterComponent>(myPawn);
    //    //var currentEq = playerComponent.Equipment;
    //    //var weaponId = currentEq.GetItem(EquipPosition.Weapon);
    //    //playerComponent.Equipment.WithSetItem(EquipPosition.Weapon, weaponId + 1);

    //    //var myPawn = GameUtils.GetControlledPawn();

    //    //pawn = WukongApi.Sync.LocalMainCharacter?.Pawn;
    //    //BGUActorBaseCS actor = new BGUActorBaseCS();

    //    //var playerComponent = EcsExtensions.GetComponent<MainCharacterComponent>(pawn);

    //}
}