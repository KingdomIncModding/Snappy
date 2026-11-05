using System;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.UI.TechTree;
using Assets.Source.Player;
using BepInEx.Bootstrap;
using HarmonyLib;
using KingdomFoundation;
using KingdomFoundation.Api;
using LightJson;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomSnappy
{
    public sealed partial class SnappyPlugin
    {
        JsonValue lockedPlayerJson;

        void VerifyFoundationLocked(GamePlayer player)
        {
            Check(Chainloader.PluginInfos.TryGetValue("urakka.kingdominc.foundation", out var dependency) &&
                dependency.Metadata.Version >= new System.Version(FoundationMinimumVersion), "Required Foundation loads before Snappy");
            Check(FoundationApi.TechNodes.TryGet(ResearchId, out var registration) && ReferenceEquals(registration, research), "Public Foundation registry owns Snappy research");
            Check(ReferenceEquals(((INativeTechNode)registration).NativeNode, nativeResearch), "Registration uses actual native research model");
            Check(nativeResearch.Name == "Snappy" && nativeResearch.Root == null && nativeResearch.Previous == null &&
                !nativeResearch.AllRequirements.Any(), "Free Snappy research has no prerequisite or branch gate");
            Check(nativeResearch.GetCost().IsEmpty, "Snappy research is free");
            nativeResearch.ResetCost();
            Check(nativeResearch.GetCost().IsEmpty, "Free cost survives native cost cache reset");
            Check(nativeResearch.IsAvailable && !player.HasTech(nativeResearch) && !FineSnappingEnabled, "New kingdom requires Snappy research");
            Check(WorldFlowManager.ClampToGrid(new Vector2Int(19, -19)) == Vector2Int.zero, "Unresearched nodes retain native 64-unit snapping");
            Check(WorldFlowManager.ClampToGrid(new Vector2Int(19, -19), 24) == new Vector2Int(0, 24), "Unresearched label retains native offset");
            Check(WorldFlowPipeNode.ClampToGrid(new Vector2Int(19, 107)) == new Vector2Int(32, 88), "Unresearched pipe bends retain native grid and offset");
        }

        void OpenResearchMap()
        {
            GameUI.Instance.ToggleTechMenu(true);
            var menu = TechTreeMenu.Instance;
            Check(menu.Nodes.All(n => n.Tech.Identifier != ResearchId), "Snappy research is absent from Base Game tab");
            var tabs = menu.GetComponent<TechTreeTabs>();
            Check(tabs != null, "Native Tech Tree has Foundation tabs");
            ((Button)AccessTools.Field(typeof(TechTreeTabs), "ModsButton").GetValue(tabs)).onClick.Invoke();
            Resources.FindObjectsOfTypeAll<Button>().Single(b => b.name == "Mod:" + Id && b.gameObject.activeInHierarchy).onClick.Invoke();
            Check(menu.Nodes.Count(n => n.Tech.Identifier == ResearchId) == 1, "Mods tab shows exactly one Snappy research entry");
        }

        void VerifyFreeResearch()
        {
            var menu = TechTreeMenu.Instance;
            var view = menu.Nodes.Single(n => n.Tech.Identifier == ResearchId);
            Check(view is UIElementTechNode && view.Tech == nativeResearch, "Snappy is displayed through the native building research view");
            Check(view.gameObject.activeInHierarchy && view.GetTooltipTitle() == "Snappy", "Visible research uses correct title and native tooltip");
            var player = GamePlayer.Current;
            lockedPlayerJson = JsonValue.Parse(player.ToJson().ToString());
            string currencyBefore = player.ToJson()["Currency"].ToString();
            view.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
            Check(player.HasTech(nativeResearch) && FineSnappingEnabled, "Native research click unlocks fine snapping immediately");
            Check(player.ToJson()["Currency"].ToString() == currencyBefore, "Free research consumes no currency");
            Check(WorldFlowManager.ClampToGrid(new Vector2Int(19, -19)) == new Vector2Int(16, -16), "Research enables quarter-grid placement without restart");
            view.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
            Check(player.HasTech(nativeResearch) && player.ToJson()["Currency"].ToString() == currencyBefore, "Repeated click preserves unlock and currency");
            GameUI.Instance.ToggleTechMenu(false);
        }

        void VerifyResearchLifecycle()
        {
            var player = GamePlayer.Current;
            var unlocked = JsonValue.Parse(player.ToJson().ToString());
            Check(unlocked["Tech"].AsJsonArray.Any(t => t.AsString == ResearchId), "Unlock uses native saved research IDs");
            try
            {
                var restored = GamePlayer.FromJson(unlocked);
                Check(restored.HasTech(nativeResearch) && FineSnappingEnabled, "Saved kingdom restores research and fine snapping");
                Check(restored.ActiveFlow.Elements[0].Position == player.ActiveFlow.Elements[0].Position, "Research save reload preserves existing fine positions");
                var locked = GamePlayer.FromJson(lockedPlayerJson);
                Check(!locked.HasTech(nativeResearch) && !FineSnappingEnabled &&
                    WorldFlowManager.ClampToGrid(new Vector2Int(19, -19)) == Vector2Int.zero, "Older unresearched kingdom keeps native snapping");
                var fresh = GamePlayer.CreateNewPlayer();
                Check(!fresh.HasTech(nativeResearch) && !FineSnappingEnabled, "Switching to a new kingdom does not leak the unlock");
            }
            finally { GamePlayer.Current = player; }
            Check(FineSnappingEnabled, "Returning to researched kingdom restores fine snapping");
            var originalModel = nativeResearch;
            research.Dispose();
            Check(!FoundationApi.TechNodes.TryGet(ResearchId, out _) && !FineSnappingEnabled &&
                WorldFlowManager.ClampToGrid(new Vector2Int(19, -19)) == Vector2Int.zero, "Disposing registration removes research and restores vanilla snapping");
            RegisterResearch();
            Check(ReferenceEquals(nativeResearch, originalModel) && player.HasTech(nativeResearch) && FineSnappingEnabled,
                "Registration restoration preserves native ordinal and saved unlock");
        }
    }
}
