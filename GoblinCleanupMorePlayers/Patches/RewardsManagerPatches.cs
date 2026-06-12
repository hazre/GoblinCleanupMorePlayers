using System.Collections.Generic;
using HarmonyLib;
using Steamworks;
using TMPro;
using UnityEngine;

namespace GoblinCleanupMorePlayers.Patches;

/// <summary>
/// Fixes RewardsManager.ShowStats to handle more than 4 players.
/// 
/// Two hardcoded 4-player limits in the original:
/// 1. `if (num8 >= playerCards.Count) break;` — skips stat display for
///    any player beyond the 4th card UI element.
/// 2. `if (stats.Count == 4)` — the fourth "specialty" nomination
///    (objects/blood/organic/etc) is only assigned when exactly 4 players
///    are present, so 5+ players never see the fourth badge.
/// 
/// This prefix replaces the entire method to allow arbitrary player counts
/// while preserving the original scoring and nomination logic.
/// </summary>
public static class RewardsManagerPatches
{
    static T Field<T>(object obj, string name) =>
        (T)AccessTools.Field(obj.GetType(), name).GetValue(obj);

    static void SetField(object obj, string name, object value) =>
        AccessTools.Field(obj.GetType(), name).SetValue(obj, value);

    [HarmonyPatch(typeof(RewardsManager), nameof(RewardsManager.ShowStats))]
    [HarmonyPrefix]
    static bool FixShowStats(RewardsManager __instance,
        ref Dictionary<CSteamID, PlayerStats> stats,
        float extraExpMultiplier,
        float extraTicketsMultiplier)
    {
        var panelHolder = Field<GameObject>(__instance, "panelHolder");
        var txtTime = Field<TextMeshProUGUI>(__instance, "txtTime");
        var playerCards = Field<List<RewardsManager.PlayerStatsCard>>(__instance, "playerCards");
        var ticketsReward = Field<TextMeshProUGUI>(__instance, "ticketsReward");
        var expReward = Field<TextMeshProUGUI>(__instance, "expReward");

        panelHolder.SetActive(true);
        float gameTime = DungeonManager.dungeonInstance.GetTime();
        if (gameTime <= 30f)
        {
            __instance.taskCount = 0;
            txtTime.color = Color.red;
        }
        else
        {
            txtTime.color = Color.white;
        }

        int expAmount = Mathf.RoundToInt(__instance.taskCount * __instance.expMultiplier * extraExpMultiplier);
        int ticketAmount = Mathf.RoundToInt(__instance.taskCount * __instance.ticketsMultiplier * extraTicketsMultiplier);
        SetField(__instance, "expRewardAmount", expAmount);
        SetField(__instance, "ticketsRewardAmount", ticketAmount);
        ProgressionController.AddStackedTickets(ticketAmount);
        ProgressionController.Instance.AddExpToClaim(expAmount);
        ticketsReward.text = ProgressionController.Instance.GetTicketsToClaim().ToString();
        expReward.text = ProgressionController.Instance.GetExpToClaim().ToString();

        // Deactivate all player card holders
        foreach (var card in playerCards)
            card.holder.SetActive(false);

        var lobbyPlayerIds = new List<CSteamID>();
        int bestIndex = -1, bestScore = 0, worstScore = 9999, worstIndex = -1;
        int mostDeaths = 0, mostDeathsIndex = -1, fourthIndex = -1;
        int cardIndex = 0;

        foreach (var kvp in stats)
        {
            if (!DungeonPlayers.instance.IsPlayerInLobby(kvp.Key))
                continue;

            // Cap at available card UI elements instead of hardcoded 4
            if (cardIndex >= playerCards.Count)
                break;

            var card = playerCards[cardIndex];
            card.holder.SetActive(true);
            card.txtName.text = SteamFriends.GetFriendPersonaName(kvp.Key);
            card.txtDeaths.text = kvp.Value.deads.ToString();
            card.txtRestorables.text = kvp.Value.restorableObjects.ToString();
            card.txtBlood.text = kvp.Value.bloodstains.ToString();
            card.txtOrganic.text = kvp.Value.organicRemains.ToString();
            card.txtTraps.text = kvp.Value.traps.ToString();
            card.txtMana.text = kvp.Value.manaObjects.ToString();
            card.txtGoldenChests.text = kvp.Value.chestsFilled.ToString();
            card.txtMonsters.text = kvp.Value.monsters.ToString();

            // Score: bloodstains + organic*2 + restorable*3 + traps*4 + mana*3 + chests*5 + monsters*10 - deaths
            int score = kvp.Value.bloodstains
                + kvp.Value.organicRemains * 2
                + kvp.Value.restorableObjects * 3
                + kvp.Value.traps * 4
                + kvp.Value.manaObjects * 3
                + kvp.Value.chestsFilled * 5
                + kvp.Value.monsters * 10
                - kvp.Value.deads;

            if (score > bestScore) { bestScore = score; bestIndex = cardIndex; }
            else if (score < worstScore) { worstScore = score; worstIndex = cardIndex; }
            else if (kvp.Value.deads > mostDeaths) { mostDeaths = kvp.Value.deads; mostDeathsIndex = cardIndex; }
            else { fourthIndex = cardIndex; }

            lobbyPlayerIds.Add(kvp.Key);
            SetNomination(__instance, "lazy", card.txtTitle);
            cardIndex++;
        }

        if (cardIndex <= 1)
        {
            SetNomination(__instance, "single", playerCards[0].txtTitle);
            return false;
        }

        if (bestIndex != -1) SetNomination(__instance, "MVP", playerCards[bestIndex].txtTitle);
        if (worstIndex != -1) SetNomination(__instance, "lazy", playerCards[worstIndex].txtTitle);
        if (mostDeathsIndex != -1) SetNomination(__instance, "death", playerCards[mostDeathsIndex].txtTitle);

        // Fourth nomination was gated on `stats.Count == 4` — changed to >= 4 for 5+ players
        if (fourthIndex != -1 && stats.Count >= 4)
        {
            var ps = stats[lobbyPlayerIds[fourthIndex]];
            int bestSpecial = ps.restorableObjects;
            int specialIdx = 0;
            if (ps.bloodstains > bestSpecial) { bestSpecial = ps.bloodstains; specialIdx = 1; }
            else if (ps.organicRemains > bestSpecial) { bestSpecial = ps.organicRemains; specialIdx = 2; }
            else if (ps.traps > bestSpecial) { bestSpecial = ps.traps; specialIdx = 3; }
            else if (ps.manaObjects > bestSpecial) { bestSpecial = ps.manaObjects; specialIdx = 4; }
            else if (ps.chestsFilled > bestSpecial) { bestSpecial = ps.chestsFilled; specialIdx = 5; }
            else if (ps.monsters > bestSpecial) { bestSpecial = ps.monsters; specialIdx = 6; }

            SetNomination(__instance, specialIdx switch
            {
                0 => "objects",
                1 => "blood",
                2 => "organic",
                3 => "traps",
                4 => "mana",
                5 => "chests",
                6 => "monsters",
                _ => "objects"
            }, playerCards[fourthIndex].txtTitle);
        }

        return false;
    }

    static void SetNomination(RewardsManager instance, string find, TextMeshProUGUI txtTitle)
    {
        var nominations = Field<List<RewardsManager.Nominations>>(instance, "nominations");
        foreach (var n in nominations)
        {
            if (n.id.ToUpper().Trim() == find.ToUpper().Trim())
            {
                txtTitle.text = n.localizatedTitle;
                break;
            }
        }
    }
}
