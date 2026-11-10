# Snappy 1.2.0

Snappy makes placement more precise in Kingdom Inc by adding three extra snap positions between each original grid point. A normal 64-unit interval becomes four 16-unit intervals: **0, 16, 32, 48, 64**.

## Features

- Finer snapping for node placement and dragging.
- More precise placement for copied-node previews, pipe bends, decorations, and container resizing.
- Keeps the visible background grid at its original spacing.
- Preserves the game's usual **Ctrl** free-placement control.
- Free Snappy research unlock in **Tech Tree > Mods**, saved separately for each kingdom.

## Requirements

- Kingdom Inc (Windows, Unity Mono).
- BepInEx 5.
- **Foundation 0.11.0 or newer.**

## Installation and Use

1. Install BepInEx 5 and Foundation first.
2. Close Kingdom Inc.
3. Install Snappy so `Snappy.dll` is located at `BepInEx/plugins/Snappy/Snappy.dll`.
4. Start the game and open **Tech Tree > Mods**.
5. Research **Snappy** for free to enable the finer grid in that kingdom.

Until Snappy is researched, normal 64-unit snapping remains active. No additional configuration or hotkey is needed after unlocking it.

## Updating and Removal

Close the game before replacing `Snappy.dll` with a newer version. Existing placed objects are not moved by the mod.

To uninstall, close the game and remove `BepInEx/plugins/Snappy/`. Existing placements remain where they are, but later movement uses normal snapping. Saving without Snappy may remove its research unlock, so you may need to research it again after reinstalling.

## Compatibility

Snappy changes the game's grid-snap calculation only while the research is unlocked. Other mods that replace the same snapping behavior may conflict. The extra snap positions do not add new lines to the background grid.

Created by Urakka.
