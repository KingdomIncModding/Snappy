using KingdomFoundation.Api;
using UnityEngine;

namespace KingdomSnappy
{
    internal static class SnappyResearch
    {
        internal static ITechNode Register(FoundationModContext context)
        {
            var definition = new TechNodeDefinition(SnappyPlugin.ResearchId, "Snappy", SnappyPlugin.Id, "Snappy", new Vector2(480, 0), "Unlock three extra snap positions between every original grid interval for nodes, pipe bends and decorations. Free research.", research: TechNodeOptions.Create(fixedCost: new GameMoney(), iconName: "Icons_17"));
            return context.Register(definition);
        }
    }
}
