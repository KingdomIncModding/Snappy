# Snappy 1.1.0

[Nexus Mods page](https://www.nexusmods.com/kingdominc/mods/12)

Adds three evenly spaced snap positions between each original grid position on both axes. A 64-unit interval becomes four 16-unit intervals: **0, 16, 32, 48, 64**.

Works with node placement and dragging, copy previews, pipe bends, decorations and container resizing. Hold **Ctrl** for free placement. The background grid keeps its original spacing; the extra snap positions lie between its lines.

## Requirements and installation

Requires Kingdom Inc, BepInEx 5 and **Foundation 0.5.0 or later**.

1. Install the requirements.
2. Close Kingdom Inc.
3. Extract the Snappy ZIP into the game folder so the plugin is at `BepInEx/plugins/Snappy/Snappy.dll`.
4. Launch the game, open **Tech Tree → Mods**, and research **Snappy** for free.

Normal snapping remains active until Snappy is researched. The unlock is saved separately for each kingdom. No additional research or hotkey is needed afterward.

## Updating and removal

Close the game before replacing the DLL. When updating from 1.0.0, research Snappy for free to enable finer snapping in your kingdom. Existing placements stay where they are.

To remove Snappy, close the game and delete `BepInEx/plugins/Snappy`. Existing positions remain intact; subsequent dragging uses the normal grid. Saving without Snappy may discard its research unlock, requiring the free research again after reinstalling.

Author: Urakka.

[Source code](https://github.com/urakkaamyx/KingdomInc_Snappy)
