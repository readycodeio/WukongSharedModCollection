using CSharpModBase;
using CSharpModBase.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.WukongUtils;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace Tester
{

    public static class WorldTester
    {
        public static List<AActor> MyActors = new List<AActor>();

        public static void ReckonActors()
        {
            HashSet<string> actorNames = new HashSet<string>();

            var world = GameUtils.GetWorld();
            if (world == null)
            {
                return;
            }

            var allActors = world.GetAllActorsOfClass<AActor>();
            foreach (var actor in allActors)
            {
                if (actor != null)
                {
                    actorNames.Add(actor.GetClass().GetName());
                }
            }

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filePath = Path.Combine(desktopPath, "WukongActorsClassList.txt");

            try
            {
                File.WriteAllLines(filePath, actorNames.OrderBy(n => n));
                
                WukongApi.Chat.ShowLocalMessage($"Saved {actorNames.Count} unique actor classes", FLinearColor.Green);
                WukongApi.Chat.ShowLocalMessage($"Actor list saved to: {filePath}", FLinearColor.Green);
            }
            catch (Exception ex)
            {
                Logging.LogError($"Error: {ex.Message}");
            }
        }

        public static void SpawnActor(string actorClassName, FVector location, FRotator rotation)
        {
            UClass actorClass = UClass.GetClass(actorClassName);
            if (actorClass == null)
            {
                WukongApi.Chat.ShowLocalMessage("No actor class named: " + actorClassName, FLinearColor.Red);
                return;
            }

            UWorld? world = GameUtils.GetWorld();
            if (world == null)
            {
                WukongApi.Chat.ShowLocalMessage("No world found", FLinearColor.Red);
                return;
            }

            MyActors.Add(world.SpawnActor(actorClass, ref location, ref rotation));
        }

        public static void TryChangeActors()
        {
            if (MyActors.Count == 0)
                return;

            var myActor = MyActors[0];
            myActor.SetActorEnableCollision(false);
            myActor.SetActorHiddenInGame(true);
            MyActors[1].OnTakeAnyDamage.Bind(OnMyActorDamaged);
        }

        public static AActor? TryGetMyPlayerActor()
        {
            try
            {
                var world = GameUtils.GetWorld();
                if (world == null)
                {
                    WukongApi.Chat.ShowLocalMessage("No world found", FLinearColor.Red);
                    return null;
                }

                var playerPawn = world.GetPlayerPawn(0);
                if (playerPawn == null)
                {
                    WukongApi.Chat.ShowLocalMessage("No player pawn 0 found", FLinearColor.Red);
                    playerPawn = world.GetPlayerPawn(1);
                    if (playerPawn == null)
                    {
                        WukongApi.Chat.ShowLocalMessage("No player pawn 1 found", FLinearColor.Red);
                        return null;
                    }
                }
                return playerPawn;
            }
            catch (Exception ex)
            {
                WukongApi.Chat.ShowLocalMessage("Error getting player actor: " + ex.Message, FLinearColor.Red);
                return null;
            }
        }

        private static void OnMyActorDamaged(AActor DamagedActor, float Damage, UDamageType DamageType, AController InstigatedBy, AActor DamageCauser)
        {
            WukongApi.Chat.ShowLocalMessage($"{DamagedActor.GetName()}, took {Damage.ToString()} {DamageType.GetName()} by {DamageCauser.GetName()}", FLinearColor.Red);
        }
    }
}
