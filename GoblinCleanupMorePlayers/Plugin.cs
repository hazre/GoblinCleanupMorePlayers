using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace GoblinCleanupMorePlayers;

[BepInAutoPlugin]
public partial class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger = null!;
    internal static ConfigEntry<int> ConfigMaxPlayers = null!;
    internal static Harmony HarmonyInstance = null!;

    public const int VanillaMaxPlayers = 4;

    private void Awake()
    {
        Logger = base.Logger;
        HarmonyInstance = new Harmony(Id);

        ConfigMaxPlayers = Config.Bind(
            "General",
            "MaxPlayers",
            8,
            "Maximum number of players (including yourself). Steam lobbies support up to 250. Range: 1-250."
        );
        if (ConfigMaxPlayers.Value < 1) ConfigMaxPlayers.Value = 1;
        if (ConfigMaxPlayers.Value > 250) ConfigMaxPlayers.Value = 250;

        Logger.LogInfo($"Plugin {Id} loaded! Max players set to {ConfigMaxPlayers.Value}");

        HarmonyInstance.PatchAll(typeof(Patches.SteamLobbyPatches));
        HarmonyInstance.PatchAll(typeof(Patches.MenuManagerPatches));
        HarmonyInstance.PatchAll(typeof(Patches.DungeonManagerPatches));
        HarmonyInstance.PatchAll(typeof(Patches.DungeonPlayersPatches));
        HarmonyInstance.PatchAll(typeof(Patches.LobbiesListManagerPatches));
        HarmonyInstance.PatchAll(typeof(Patches.RewardsManagerPatches));

        Logger.LogInfo("All patches applied");
    }

    void OnDestroy()
    {
        HarmonyInstance?.UnpatchSelf();
        Logger.LogInfo("Plugin unloaded");
    }
}
