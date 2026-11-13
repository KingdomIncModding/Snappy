using KingdomFoundation.Api;

namespace KingdomSnappy
{
    internal static class FineGridPolicy
    {
        internal static bool FineSnappingEnabled => SnappyPlugin.IsActive && FoundationApi.World.IsInKingdom && FoundationApi.Progression.HasTech(SnappyPlugin.ResearchId);
    }
}
