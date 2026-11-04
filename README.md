# Snappy 1.0.0

[Nexus Mods page](https://www.nexusmods.com/kingdominc/mods/12)

Adds three evenly spaced snap positions between each original Kingdom Inc grid position, on both axes. A normal 64-unit interval becomes four 16-unit intervals: **0, 16, 32, 48, 64**.

Works with node placement and dragging, copy/blueprint placement, pipe bends, decoration placement and dragging, and container resizing. Existing pipe and label alignment offsets are preserved. Hold Ctrl for the game's normal free placement. The visible background grid keeps its original spacing; the extra positions are between those lines. Node sizes, port counts and the minimum container size stay the same.

Requires Kingdom Inc (Unity Mono) and BepInEx 5 with its bundled Harmony. Tested on Release v1.0.12, Unity 6000.3.27f2, and BepInEx 5.4.23.6. No other plugin is required. Another plugin replacing WorldFlowManager.ClampToGrid may conflict.

## Installation and removal

Close Kingdom Inc. Extract the player ZIP into the game folder so the file is at `BepInEx/plugins/Snappy/Snappy.dll`, then launch the game. Snappy is enabled automatically and needs no hotkey or configuration.

To update, close the game and replace that DLL. To remove, close the game and delete the Snappy folder under BepInEx/plugins. Snappy adds no save fields or custom nodes. Existing layouts are not moved; native saves retain fine positions after removal, and subsequent dragging returns to the normal grid. Manual installation is supported; Vortex deployment is untested.

## Development

Keep the source at `<KingdomInc>/_Mods/Snappy` with the game and BepInEx assemblies installed locally. Requires a .NET SDK that supports .NET Standard 2.1.

```powershell
dotnet build _Mods/Snappy/Snappy.csproj -c Release
& ./_Mods/Snappy/verify-isolated.ps1
& ./_Mods/Snappy/verify-isolated.ps1 -WithExistingPlugins
& ./_Mods/Snappy/Tools/package.ps1
& ./_Mods/Snappy/install.ps1
```

Diagnostics run the actual Unity game in a private runtime with latest-save loading, save writes and Steam achievements blocked. They use a disposable kingdom to check all quarter steps, negative coordinates, rounding boundaries, native pipe/label offsets, real node/pipe/deco dragging, placement, collision rejection, Ctrl placement, native JSON persistence, and patch removal. Companion mode loads the installed plugins from the main game into that private runtime. Verification artifacts and game assemblies are excluded from the package.

Version 1.0.0 passed 353 isolated checks and the same 353 checks with Container Colors 1.2.0, Factory API 0.1.3, Ledger 0.4.3, Leyline Conduits 0.3.0, Nightmare Distiller Fix 1.0.2, Party Node 0.6.0, Path Finder 1.0.3, Port Layouts 1.0.1, and Sharable Blueprints 0.5.2 loaded. This verifies Snappy's native placement behavior while those plugins are present; it does not retest every feature of each companion.

Author: Urakka. Plugin ID: `urakka.kingdominc.snappy`. Repository naming convention: `urakkaamyx/KingdomInc_Snappy` 

Source repository: https://github.com/urakkaamyx/KingdomInc_Snappy
