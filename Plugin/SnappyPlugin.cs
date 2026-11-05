using System;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Source.Player;
using BepInEx;
using HarmonyLib;
using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomSnappy
{
    [BepInPlugin(Id, "Snappy", Version)]
    [BepInDependency("urakka.kingdominc.foundation", FoundationMinimumVersion)]
    public sealed partial class SnappyPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.snappy";
        public const string Version = "1.1.0";
        public const string FoundationMinimumVersion = "0.5.0";
        public const string ResearchId = Id + ".research";
        public const int GridStep = 16;
        internal static bool Diagnostic => Environment.GetCommandLineArgs().Contains("-snappy-smoke");
        Harmony harmony;
        ITechNode research;
        TechNode nativeResearch;
        static SnappyPlugin instance;
        internal static bool FineSnappingEnabled => instance != null && instance.research?.IsRegistered == true &&
            GamePlayer.Current != null && GamePlayer.Current.HasTech(instance.nativeResearch);

        void Awake()
        {
            instance = this;
            try
            {
                RegisterResearch();
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
                Logger.LogInfo("Snappy ready: research Snappy for free in Tech Tree > Mods to unlock finer snapping.");
            }
            catch { OnDestroy(); throw; }
        }

        void RegisterResearch()
        {
            research = FoundationApi.CreateTechNode(new TechNodeDefinition(
                ResearchId, "Snappy", new Vector2(480, 0),
                "Unlock three extra snap positions between every original grid interval for nodes, pipe bends and decorations. Free research.",
                research: new TechNodeOptions(fixedCost: new CurrencyCollection(), iconName: "Icons_17")));
            nativeResearch = ((INativeTechNode)research).NativeNode;
        }

        void OnDestroy()
        {
            harmony?.UnpatchSelf();
            research?.Dispose();
            if (instance == this) instance = null;
        }

        [HarmonyPatch(typeof(WorldFlowManager), nameof(WorldFlowManager.ClampToGrid))]
        static class FineGrid
        {
            static bool Prefix(Vector2Int pos, int addedYOffset, ref Vector2Int __result)
            {
                if (!FineSnappingEnabled) return true;
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
