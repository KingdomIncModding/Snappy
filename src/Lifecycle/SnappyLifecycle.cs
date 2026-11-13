using System;
using BepInEx.Logging;
using KingdomFoundation.Api;

namespace KingdomSnappy
{
    internal sealed class SnappyLifecycle : IDisposable
    {
        private FoundationModContext context;
        private IDisposable snapHook;
        private ITechNode research;
        internal void Start(ManualLogSource logger)
        {
            context = FoundationApi.ForMod(SnappyPlugin.Id);
            research = SnappyResearch.Register(context);
            snapHook = FoundationApi.GameHooks.Subscribe(SnappyPlugin.Id, GameHookPoints.GridSnap, before: FineGridPatch.Before);
            logger.LogInfo("Snappy ready: research Snappy for free in Tech Tree > Mods.");
        }

        public void Dispose()
        {
            snapHook?.Dispose();
            context?.Dispose();
        }
    }
}
