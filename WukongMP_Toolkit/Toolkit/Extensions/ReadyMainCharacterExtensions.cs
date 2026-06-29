using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WukongMp.Sdk.Entities;
using WukongMp.Toolkit.Widgets;

namespace WukongMp.Toolkit.Actors;

public static class ReadyMainCharactersExtensions
{
    public static void HideHp(this ReadyMainCharacter character)
    {
        if (WidgetsHpBars.PlayerHpBars.TryGetValue(character.PlayerId, out var hpBar))
        {
            hpBar.SetVisibility(UnrealEngine.UMG.ESlateVisibility.Hidden);
        }
    }
    public static void ShowHp(this ReadyMainCharacter character)
    {
        if (WidgetsHpBars.PlayerHpBars.TryGetValue(character.PlayerId, out var hpBar))
        {
            hpBar.SetVisibility(UnrealEngine.UMG.ESlateVisibility.Visible);
        }
    }

    public static void SetHostileAgainstTeams(this ReadyMainCharacter character, IEnumerable<int> teamIds)
    {
        int myTeamId = ActorsHostility.GetPlayerTeamId(character);
        foreach (var teamId in teamIds.ToList())
        {
            if (teamId == myTeamId) continue;
            ActorsHostility.RegisterTeamHostility(myTeamId, teamId);
        }
    }

    public static void SetHostileAgainstTeam(this ReadyMainCharacter character, int targetTeamId)
    {
        int myTeamId = ActorsHostility.GetPlayerTeamId(character);
        ActorsHostility.RegisterTeamHostility(myTeamId, targetTeamId);
    }
    public static void SetHostileAgainstPlayer(this ReadyMainCharacter character, ReadyMainCharacter targetCharacter)
    {
        int myTeamId = ActorsHostility.GetPlayerTeamId(character);
        int targetTeamId = ActorsHostility.GetPlayerTeamId(targetCharacter);

        ActorsHostility.RegisterTeamHostility(myTeamId, targetTeamId);
    }

    public static void RemoveHostilityAgainstPlayer(this ReadyMainCharacter character, ReadyMainCharacter targetCharacter)
    {
        int myTeamId = ActorsHostility.GetPlayerTeamId(character);
        int targetTeamId = ActorsHostility.GetPlayerTeamId(targetCharacter);

        ActorsHostility.RemoveTeamHostility(myTeamId, targetTeamId);
    }

    public static void RemoveHostilityAgainstTeam(this ReadyMainCharacter character, int targetTeamId)
    {
        int myTeamId = ActorsHostility.GetPlayerTeamId(character);

        ActorsHostility.RemoveTeamHostility(myTeamId, targetTeamId);
    }

    public static void RemoveHostileAgainstTeams(this ReadyMainCharacter character, IEnumerable<int> teamIds)
    {
        int myTeamId = ActorsHostility.GetPlayerTeamId(character);
        foreach (var teamId in teamIds.ToList())
        {
            if (teamId == myTeamId) continue;
            ActorsHostility.RemoveTeamHostility(myTeamId, teamId);
        }
    }
}
