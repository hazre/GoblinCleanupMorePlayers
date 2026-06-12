using HarmonyLib;

namespace GoblinCleanupMorePlayers.Patches;

/// <summary>
/// Logs when the host clicks "Host Steam" so we can verify the
/// plugin is active before the lobby is created.
/// </summary>
public static class MenuManagerPatches
{
    [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.OnClick_Host_Steam))]
    [HarmonyPrefix]
    static void LogHostClick(MenuManager __instance)
    {
        Plugin.Logger.LogInfo($"OnClick_Host_Steam: will use maxPlayers={Plugin.ConfigMaxPlayers.Value}");
    }
}
