using System;
using HarmonyLib;

namespace GoblinCleanupMorePlayers.Patches;

[HarmonyPatch]
public static class DungeonPlayersPatches
{
    /// <summary>
    /// All players spawn at the host's room (index 0) instead of separate rooms.
    /// </summary>
    [HarmonyPatch(typeof(DungeonPlayers), nameof(DungeonPlayers.GetPlayerDefaultSpawnPoint))]
    [HarmonyPrefix]
    static bool AllSpawnAtHostRoom(ref UnityEngine.Transform __result)
    {
        var dp = DungeonPlayers.instance;
        if (dp.RoomsspawnPoints.Length == 0)
        {
            __result = dp.staffRoomSpawnPoint;
        }
        else
        {
            __result = dp.RoomsspawnPoints[0];
        }
        return false;
    }

    /// <summary>
    /// On every client start: disable all restore colliders, close non-host doors,
    /// open host's door, set non-host frames to "Shared Room".
    /// </summary>
    [HarmonyPatch(typeof(DungeonPlayers), nameof(DungeonPlayers.OnStartClient))]
    [HarmonyPostfix]
    static void InitSharedRoomState(DungeonPlayers __instance)
    {
        for (int i = 0; i < __instance.restoreColliders.Length; i++)
        {
            if (__instance.restoreColliders[i] != null)
                __instance.restoreColliders[i].gameObject.SetActive(false);
        }

        for (int i = 1; i < __instance.RoomDoorObject.Length; i++)
        {
            if (__instance.RoomDoorObject[i] != null)
                __instance.RoomDoorObject[i].SetActive(true);
        }

        if (__instance.RoomDoorObject.Length > 0 && __instance.RoomDoorObject[0] != null)
            __instance.RoomDoorObject[0].SetActive(false);

        if (__instance.playerFrames.Length > 0 && __instance.playerFrames[0] != null)
            __instance.playerFrames[0].SetDisconnectData("Shared Room", __instance.noAvatarImage);
        for (int i = 1; i < __instance.playerFrames.Length; i++)
        {
            if (__instance.playerFrames[i] != null)
                __instance.playerFrames[i].SetDisconnectData("Out of Order", __instance.noAvatarImage);
        }
    }

    /// <summary>
    /// All SetRoomData is suppressed — frames are set by OnStartClient instead.
    /// </summary>
    [HarmonyPatch(typeof(DungeonPlayers), "SetRoomData")]
    [HarmonyPrefix]
    static bool SuppressSetRoomData()
    {
        return false;
    }

    /// <summary>
    /// Skip door/collider management for non-host rooms.
    /// Prevents RemovePlayerObserver from re-enabling restore colliders on closed rooms.
    /// </summary>
    [HarmonyPatch(typeof(DungeonPlayers), "EnableRoomDoor")]
    [HarmonyPrefix]
    static bool SkipNonHostRoomManagement(int playerIndex)
    {
        return playerIndex == 0;
    }
}
