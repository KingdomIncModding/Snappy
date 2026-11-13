using BepInEx;

namespace KingdomSnappy
{
    [BepInPlugin(Id, "Snappy", Version)]
    [BepInDependency("urakka.kingdominc.foundation", FoundationMinimumVersion)]
    public sealed class SnappyPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.snappy";
        public const string Version = "1.2.1";
        public const string FoundationMinimumVersion = "0.14.0";
        public const string ResearchId = Id + ".research";
        public const int GridStep = 16;
        private static SnappyPlugin instance;
        private SnappyLifecycle lifecycle;
        internal static bool IsActive => instance != null;

        private void Awake()
        {
            instance = this;
            lifecycle = new SnappyLifecycle();
            try
            {
                lifecycle.Start(Logger);
            }
            catch
            {
                OnDestroy();
                throw;
            }
        }

        private void OnDestroy()
        {
            lifecycle?.Dispose();
            if (instance == this)
                instance = null;
        }
    }
}
