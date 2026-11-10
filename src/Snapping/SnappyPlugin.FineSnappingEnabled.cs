using Assets.Source.Player;

namespace KingdomSnappy
{
    public sealed partial class SnappyPlugin
    {
        internal static bool FineSnappingEnabled =>
            instance != null &&
            GamePlayer.Current != null &&
            GamePlayer.Current.HasTech(instance.nativeResearch);
    }
}
