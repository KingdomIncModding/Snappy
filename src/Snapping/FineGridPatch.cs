using Assets.Behaviour.Flow;
using HarmonyLib;
using UnityEngine;

namespace KingdomSnappy
{
    // Foundation handles research and ownership; Harmony is needed only to change
    // the native snapping calculation, which Foundation does not configure.
    [HarmonyPatch(typeof(WorldFlowManager), nameof(WorldFlowManager.ClampToGrid))]
    internal static class FineGridPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Vector2Int pos, int addedYOffset, ref Vector2Int __result)
        {
            if (!SnappyPlugin.FineSnappingEnabled)
                return true;

            // Preserve native midpoint rounding and the caller's vertical offset.
            __result = new Vector2Int(
                Mathf.RoundToInt((float)pos.x / SnappyPlugin.GridStep) * SnappyPlugin.GridStep,
                Mathf.RoundToInt((float)pos.y / SnappyPlugin.GridStep) * SnappyPlugin.GridStep + addedYOffset);
            return false;
        }
    }
}
