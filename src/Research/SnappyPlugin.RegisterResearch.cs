using Assets.Source.Player;
using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomSnappy
{
    public sealed partial class SnappyPlugin
    {
        private void RegisterResearch()
        {
            var definition = new TechNodeDefinition(
                ResearchId,
                "Snappy",
                Id,
                "Snappy",
                new Vector2(480, 0),
                "Unlock three extra snap positions between every original grid interval for nodes, pipe bends and decorations. Free research.",
                research: new TechNodeOptions(
                    fixedCost: new CurrencyCollection(),
                    iconName: "Icons_17"));

            nativeResearch = ((INativeTechNode)context.Register(definition)).NativeNode;
        }
    }
}
