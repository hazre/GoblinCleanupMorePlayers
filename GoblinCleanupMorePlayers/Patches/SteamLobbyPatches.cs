using HarmonyLib;
using Steamworks;

namespace GoblinCleanupMorePlayers.Patches;

/// <summary>
/// Overrides the 4-player lobby cap by hooking SteamLobby methods.
/// 
/// HostLobby receives a maxPlayers parameter that the game sets to 4.
/// This prefix replaces it with the configured MaxPlayers value.
/// 
/// OnLobbyCreated resets maxPlayers to 4 after lobby creation via a
/// private field. This postfix fixes it back to the configured value.
/// </summary>
public static class SteamLobbyPatches
{
    [HarmonyPatch(typeof(SteamLobby), nameof(SteamLobby.HostLobby))]
    [HarmonyPrefix]
    static void OverrideMaxPlayers(ref int maxPlayers, ref ELobbyType lobbyType)
    {
        Plugin.Logger.LogInfo($"HostLobby: overriding maxPlayers {maxPlayers} -> {Plugin.ConfigMaxPlayers.Value}");
        maxPlayers = Plugin.ConfigMaxPlayers.Value;
    }

    [HarmonyPatch(typeof(SteamLobby), "OnLobbyCreated")]
    [HarmonyPostfix]
    static void FixLobbyCreatedMaxPlayers(SteamLobby __instance)
    {
        if (__instance.CurrentLobbyID == 0)
            return;

        var field = typeof(SteamLobby).GetField("maxPlayers",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field != null)
        {
            int current = (int)field.GetValue(__instance);
            field.SetValue(__instance, Plugin.ConfigMaxPlayers.Value);
            Plugin.Logger.LogInfo($"OnLobbyCreated: fixed maxPlayers {current} -> {Plugin.ConfigMaxPlayers.Value}");
        }

        var steamID = new CSteamID(__instance.CurrentLobbyID);
        int max = Plugin.ConfigMaxPlayers.Value;
        SteamMatchmaking.SetLobbyMemberLimit(steamID, max);
        int players = SteamMatchmaking.GetNumLobbyMembers(steamID);
        SteamMatchmaking.SetLobbyData(steamID, "players", players + " / " + max);
        SteamMatchmaking.SetLobbyData(steamID, "isFull", (players >= max).ToString());
        Plugin.Logger.LogInfo($"OnLobbyCreated: SetLobbyMemberLimit -> {max}, players={players}/{max}");
    }
}
