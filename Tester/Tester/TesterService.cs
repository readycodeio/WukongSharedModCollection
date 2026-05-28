using ReadyM.Api.DI;
using System;
using System.Collections.Generic;
using System.Text;
using UnrealEngine.Runtime;
using WukongMp.Api;
using WukongMp.Sdk.Api;
using WukongMp.Sdk.Entities;

namespace Tester;

public sealed class TesterService : IHostedService
{
    private static int _monstersKilled = 0;
    public void OnScopeStart()
    {
        WukongApi.Events.OnMonsterDead += OnMonsterDeadHandler;
    }

    public void Dispose()
    {
        WukongApi.Events.OnMonsterDead -= OnMonsterDeadHandler;
    }
    private static void OnMonsterDeadHandler(ReadyTamer monster, ReadyCharacter? source)
    {
        _monstersKilled++;
        WukongApi.Chat.ShowLocalMessage($"Monster killed by {source.Value.Owner.ToString()}, Total kills: {_monstersKilled}", FLinearColor.Red);
        Logging.LogDebug($"[TesterService] Monster killed by {source.Value.Owner.ToString()}, Total kills: {_monstersKilled}");
    }
}
