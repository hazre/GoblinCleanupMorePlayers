using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace GoblinCleanupMorePlayers.Patches;

/// <summary>
/// Fixes the lobby browser player count display.
///
/// The original UpdateLobbyEntry calls SetLobbyData with a hardcoded "4" player count
/// string from the lobby data set during lobby creation. This postfix recalculates
/// the count using GetNumLobbyMembers/GetLobbyMemberLimit so lobbies with >4 slots
/// show the correct player count (e.g. "3/8" instead of "3/4").
/// </summary>
public static class LobbiesListManagerPatches
{
    [HarmonyPatch(typeof(LobbiesListManager), "UpdateLobbyEntry")]
    [HarmonyPostfix]
    static void FixPlayerCountDisplay(LobbyDataEntry lobbyData, CSteamID lobbyID)
    {
        string players = SteamMatchmaking.GetNumLobbyMembers(lobbyID) + "/" + SteamMatchmaking.GetLobbyMemberLimit(lobbyID);
        lobbyData.SetLobbyData(
            SteamMatchmaking.GetLobbyData(lobbyID, "isFull") == "False",
            SteamMatchmaking.GetLobbyData(lobbyID, "password"),
            players,
            SteamMatchmaking.GetLobbyData(lobbyID, "Level"),
            SteamMatchmaking.GetLobbyData(lobbyID, "GameVersion") == Application.version
        );
    }
}
