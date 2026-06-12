# More Players
[![Thunderstore Badge](https://modding.resonite.net/assets/available-on-thunderstore.svg)](https://thunderstore.io/c/goblincleanup/)

A [Goblin Cleanup](https://store.steampowered.com/app/2748340/Goblin_Cleanup/) mod that removes the 4-player cap.

All players share a single room instead of being split across separate rooms. Extra room doors are closed and labelled "Out of Order".

> [!NOTE]
> Only the host needs to install this mod. Clients without the mod can still join.

## Features
- [x] Remove 4-player lobby cap (configurable up to 250)
- [x] All players spawn in the same room
- [x] Extra rooms closed with "Out of Order" labels
- [x] Kickout UI, lobby list, and stats screen handle arbitrary player counts
- [x] Server-side only — clients don't need the mod

## Installation (Manual)
1. Install [BepInExPack for Goblin Cleanup](https://github.com/hazre/BepInExPack-GoblinCleanup) (includes BepInEx + unstripped corlibs).
2. Download the latest release ZIP from the [Releases](https://github.com/hazre/GoblinCleanupMorePlayers/releases) page.
3. Extract the ZIP and copy `GoblinCleanupMorePlayers.dll` to your BepInEx plugins folder:
   - **Default location:** `C:\Program Files (x86)\Steam\steamapps\common\Goblin Cleanup\BepInEx\plugins\`
4. Start the game. Host a lobby — the mod will allow more than 4 players.

## Configuration

The mod can be configured via `BepInEx/config/GoblinCleanupMorePlayers.cfg`:

- `MaxPlayers`: (Default: `8`, Range: `1`–`250`) Maximum number of players. Steam lobbies support up to 250.

## License

This project is licensed under MIT License. See [LICENSE](LICENSE) for details.
