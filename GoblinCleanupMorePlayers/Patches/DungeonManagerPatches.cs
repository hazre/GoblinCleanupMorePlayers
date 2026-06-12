using System.Collections.Generic;
using HarmonyLib;

namespace GoblinCleanupMorePlayers.Patches;

/// <summary>
/// Fixes DungeonManager.UpdatePlayerKickout to handle more than 4 players.
/// 
/// The original method hardcodes indices [0],[1],[2] for the kickout UI arrays.
/// With >3 non-local players, the loop at line 1174 writes past the array
/// bounds, crashing the game.
/// 
/// This prefix replaces the method with a bounds-safe version that iterates
/// all kickout slots and clamps accesses to the actual array lengths.
/// </summary>
public static class DungeonManagerPatches
{
    [HarmonyPatch(typeof(DungeonManager), "UpdatePlayerKickout")]
    [HarmonyPrefix]
    static bool FixKickoutBounds(DungeonManager __instance)
    {
        // Deactivate all kickout slots (original only deactivated first 3)
        for (int i = 0; i < __instance.kickoutPlayer.Count; i++)
            __instance.kickoutPlayer[i].SetActive(false);

        // Build list of non-local players (same logic as original)
        var nonLocalPlayers = new List<GoblinCharacterController>();
        foreach (var player in DungeonPlayers.instance.players)
        {
            if ((bool)player && player != GoblinCharacterController.localPlayer)
                nonLocalPlayers.Add(player);
        }

        // Activate kickout buttons and set names, clamped to array bounds
        for (int i = 0; i < nonLocalPlayers.Count; i++)
        {
            if (i < __instance.kickoutPlayer.Count)
                __instance.kickoutPlayer[i].SetActive(true);
            if (i < __instance.txtkickoutNames.Count)
                __instance.txtkickoutNames[i].text = nonLocalPlayers[i].playerName.text;
        }

        return false;
    }
}
