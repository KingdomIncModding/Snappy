using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomSnappy
{
    internal static class FineGridPatch
    {
        internal static void Before(GameHookCall call)
        {
            if (!FineGridPolicy.FineSnappingEnabled) return;
            Vector2Int pos = call.Argument<Vector2Int>(0);
            int addedYOffset = call.Argument<int>(1);
            call.SetResult(new Vector2Int(
                Mathf.RoundToInt((float)pos.x / SnappyPlugin.GridStep) * SnappyPlugin.GridStep,
                Mathf.RoundToInt((float)pos.y / SnappyPlugin.GridStep) * SnappyPlugin.GridStep + addedYOffset));
            call.Cancel();
        }
    }
}
