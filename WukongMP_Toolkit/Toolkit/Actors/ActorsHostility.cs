using b1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnrealEngine.Plugins.InterchangeImport;
using WukongMp.Api;
using WukongMp.Api.WukongUtils;
using WukongMp.Sdk.Entities;

namespace WukongMp.Toolkit.Actors;

public static class ActorsHostility
{
    public static void RegisterMutualHostility(IEnumerable<int> teamIds)
    {
        if (teamIds == null) return;

        List<int> teams = teamIds.ToList();

        for (int i=0; i<teams.Count; i++)
        {
            for (int j=i+1; j<teams.Count; j++)
            {
                RegisterTeamHostility(teams[i], teams[j]);
            }
        }
    }

    public static void RegisterTeamHostility(int team1, int team2)
    {
        if (team1 == team2) return;

        var teamRelationData = (BGC_TeamRelationData)BGU_DataUtil.GetGameStateReadonlyData<IBGC_TeamRelationData, BGC_TeamRelationData>(GameUtils.GetWorld());

        EnsureTeamRelationExists(teamRelationData, team1);
        EnsureTeamRelationExists(teamRelationData, team2);

        var team1RelationInfo = teamRelationData.TeamHostileInfos[team1];
        var team2RelationInfo = teamRelationData.TeamHostileInfos[team2];

        if (!team1RelationInfo.HostileTeamIDs.Contains(team2))
        {
            team1RelationInfo.HostileTeamIDs.Add(team2);
        }

        if (!team2RelationInfo.HostileTeamIDs.Contains(team1))
        {
            team2RelationInfo.HostileTeamIDs.Add(team1);
        }
    }
    public static void RemoveTeamHostility(int team1, int team2)
    {
        var teamRelationData = (BGC_TeamRelationData)BGU_DataUtil.GetGameStateReadonlyData<IBGC_TeamRelationData, BGC_TeamRelationData>(GameUtils.GetWorld());

        EnsureTeamRelationExists(teamRelationData, team1);
        EnsureTeamRelationExists(teamRelationData, team2);

        var team1RelationInfo = teamRelationData.TeamHostileInfos[team1];
        var team2RelationInfo = teamRelationData.TeamHostileInfos[team2];

        team1RelationInfo.HostileTeamIDs.Remove(team2);
        team2RelationInfo.HostileTeamIDs.Remove(team1);
    }

    public static void RemoveAllTeamHostility(IEnumerable<int> teamIds)
    {
        if (teamIds == null) return;

        List<int> teams = teamIds.ToList();

        for (int i=0; i<teams.Count; i++)
        {
            for (int j=i+1; j<teams.Count; j++)
            {
                RemoveTeamHostility(teams[i], teams[j]);
            }
        }
    }

    public static void EnsureTeamRelationExists(BGC_TeamRelationData teamRelationData, int teamId)
    {
        if (!teamRelationData.TeamHostileInfos.ContainsKey(teamId))
        {
            teamRelationData.TeamHostileInfos.Add(teamId, new TeamRelationInfo());
        }
    }

    public static int GetPlayerTeamId(ReadyMainCharacter character)
    {
        if (character.Pawn != null)
            return character.Pawn.GetTeamID();
        else
            return character.TeamId;
    }
}
