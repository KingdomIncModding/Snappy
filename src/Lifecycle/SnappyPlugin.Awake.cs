using HarmonyLib;
using KingdomFoundation.Api;

namespace KingdomSnappy
{
    public sealed partial class SnappyPlugin
    {
        private void Awake()
        {
            instance = this;
            try
            {
                context = FoundationApi.ForMod(Id);
                RegisterResearch();

                harmony = new Harmony(Id);
                harmony.CreateClassProcessor(typeof(FineGridPatch)).Patch();
                Logger.LogInfo("Snappy ready: research Snappy for free in Tech Tree > Mods.");
            }
            catch
            {
                OnDestroy();
                throw;
            }
        }
    }
}
