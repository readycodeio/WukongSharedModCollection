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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using UnrealEngine.Engine;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Api.Configuration;
using WukongMp.Sdk;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace Tester;

public static class Spawner
{
    private static List<TamerKind> _allEnemies;
    private static int _currentEnemyIndex = 0;

    public static void Init()
    {
        WukongApi.Input.RegisterKeyBind(ModifierKeys.Shift, Key.G, SpawnEnemy);
    }

    public static void SpawnEnemy()
    {
        try
        {
            var characterNullable = WukongApi.Sync.LocalMainCharacter;
            if (!characterNullable.HasValue) return; 

            var enemyLocation = characterNullable.Value.Location;
            enemyLocation.X += 500;
            var nextEnemy = GetNextEnemy();

            WukongApi.Sync.SpawnEnemy(nextEnemy, enemyLocation);

            WukongApi.Chat.ShowLocalMessage($"Spawning: {nextEnemy}", new FLinearColor(0f, 1f, 0f, 1f));
        }
        catch (Exception ex)
        {
            Logging.LogError($"[Spawner] ERROR: {ex}");
        }
    }

    public static TamerKind GetNextEnemy()
    {
        if (_allEnemies == null || !_allEnemies.Any())
        {
            _allEnemies = TamerKinds.GetAllValidTamerKinds().ToList();
        }

        var enemyToReturn = _allEnemies[_currentEnemyIndex];

        _currentEnemyIndex = (_currentEnemyIndex + 1) % _allEnemies.Count;

        return enemyToReturn;
    }
}
