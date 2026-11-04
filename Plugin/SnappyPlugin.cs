using System;
using System.Linq;
using Assets.Behaviour.Flow;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace KingdomSnappy
{
    [BepInPlugin(Id, "Snappy", Version)]
    public sealed partial class SnappyPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.snappy";
        public const string Version = "1.0.0";
        public const int GridStep = 16;
        internal static bool Diagnostic => Environment.GetCommandLineArgs().Contains("-snappy-smoke");
        Harmony harmony;

        void Awake()
        {
            harmony = new Harmony(Id);
            harmony.CreateClassProcessor(typeof(FineGrid)).Patch();
            if (Diagnostic)
            {
                // These protections are installed only in the disposable diagnostic runtime.
                foreach (var type in new[] { typeof(NoSave), typeof(NoAutosave), typeof(NoLoad), typeof(NoAchievement), typeof(Offline) })
                    harmony.CreateClassProcessor(type).Patch();
                Application.runInBackground = true;
                StartCoroutine(RuntimeCheck());
            }
            Logger.LogInfo("Snappy ready: 16-unit snapping; three additional positions between each original grid interval.");
        }

        void OnDestroy() => harmony?.UnpatchSelf();

        [HarmonyPatch(typeof(WorldFlowManager), nameof(WorldFlowManager.ClampToGrid))]
        static class FineGrid
        {
            static bool Prefix(Vector2Int pos, int addedYOffset, ref Vector2Int __result)
            {
                // Preserve Unity's native midpoint rounding and the caller's label offset.
                // Pipe bends apply their native (32,40) offset around this same function.
                __result = new Vector2Int(
                    Mathf.RoundToInt((float)pos.x / GridStep) * GridStep,
                    Mathf.RoundToInt((float)pos.y / GridStep) * GridStep + addedYOffset);
                return false;
            }
        }
    }
}
