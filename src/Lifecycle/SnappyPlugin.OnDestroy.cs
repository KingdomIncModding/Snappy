namespace KingdomSnappy
{
    public sealed partial class SnappyPlugin
    {
        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            context?.Dispose();
            if (instance == this)
                instance = null;
        }
    }
}
