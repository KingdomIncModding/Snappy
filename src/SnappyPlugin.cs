using Assets.Source.Player;
using BepInEx;
using HarmonyLib;
using KingdomFoundation.Api;

namespace KingdomSnappy
{
    [BepInPlugin(Id, "Snappy", Version)]
    [BepInDependency("urakka.kingdominc.foundation", FoundationMinimumVersion)]
    public sealed partial class SnappyPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.snappy";
        public const string Version = "1.2.0";
        public const string FoundationMinimumVersion = "0.11.0";
        public const string ResearchId = Id + ".research";
        public const int GridStep = 16;

        private static SnappyPlugin instance;
        private FoundationModContext context;
        private Harmony harmony;
        private TechNode nativeResearch;
    }
}
