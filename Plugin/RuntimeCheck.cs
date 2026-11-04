using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using Assets.Source.Flow.Elements;
using Assets.Source.Player;
using Assets.Source.Util;
using BepInEx;
using HarmonyLib;
using LightJson;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace KingdomSnappy
{
    public sealed partial class SnappyPlugin
    {
        readonly List<string> checks = new List<string>();
        void Check(bool pass, string name)
        { if (!pass) throw new InvalidOperationException(name); checks.Add(name); }
        void At(Vector2 actual, Vector2 expected, string name)
            => Check(Vector2.Distance(actual, expected) < .01f, name + " (" + actual + " / " + expected + ")");
        static void Control(Keyboard keyboard, bool pressed)
        {
            InputSystem.QueueStateEvent(keyboard, pressed ? new KeyboardState(Key.LeftCtrl) : new KeyboardState());
            InputSystem.Update();
        }

        IEnumerator RuntimeCheck()
        {
            yield return new WaitForSecondsRealtime(5);
            string directory = Path.Combine(Paths.GameRootPath, "_Mods", "Snappy", "Verification");
            Directory.CreateDirectory(directory);
            string error = null;
            Manifold source = null, sink = null;
            FlowPipe pipe = null;
            Keyboard keyboard = null; Mouse mouse = null;
            try
            {
                Check(!GameUI.Instance && GamePlayer.Current == null, "No normal kingdom or save loaded");
                Check(!SaveGame.LoadLatestSave(), "Diagnostic blocks latest-save loading");
                VerifyGrid();
                var player = GamePlayer.CreateNewPlayer();
                player.Flows[0].Elements.Clear(); player.Flows[0].Pipes.Clear(); player.Flows[0].Decorations.Clear();
                player.Flows[0].Label = "Snappy disposable verification";
                player.AddTech("manifold");
                player.AddCurrency(Currency.Gold, 1000000000, false);
                player.AddCurrency(Currency.Medal, 1000000000, false);
                source = new Manifold { Position = new Vector2Int(-512, 128) };
                sink = new Manifold { Position = new Vector2Int(512, 128) };
                player.Flows[0].Elements.AddRange(new FlowElement[] { source, sink });
                pipe = new FlowPipe(); pipe.Connect(source.Outputs[0], sink.Inputs[0]);
                pipe.Nodes.Add(new Vector2Int(0, 88)); pipe.Nodes.Add(new Vector2Int(384, 88));
                player.Flows[0].Pipes.Add(pipe);
                SceneManager.LoadScene("Game");
            }
            catch (Exception ex) { error = ex.ToString(); }
            yield return new WaitForSecondsRealtime(4);
            if (error == null) try
            {
                Check(source.World && sink.World && pipe.World, "Native node and pipe prefabs loaded in Unity");
                keyboard = InputSystem.AddDevice<Keyboard>(); keyboard.MakeCurrent();
                mouse = InputSystem.AddDevice<Mouse>(); mouse.MakeCurrent();
                Control(keyboard, false);
                VerifyNode(source, sink, keyboard);
                VerifyPipe(pipe, source, sink, keyboard);
                VerifyGhost(source, keyboard);
                VerifyDecoration(keyboard);
                VerifyPersistenceAndRemoval(source, pipe);
            }
            catch (Exception ex) { error = ex.ToString(); }
            if (keyboard != null) InputSystem.RemoveDevice(keyboard);
            if (mouse != null) InputSystem.RemoveDevice(mouse);
            var report = new JsonObject {
                {"passed", error == null}, {"version", Version}, {"gameVersion", Application.version},
                {"unityVersion", Application.unityVersion}, {"bepInExVersion", typeof(BaseUnityPlugin).Assembly.GetName().Version.ToString()},
                {"dllSha256", HashFile(typeof(SnappyPlugin).Assembly.Location)}, {"count", checks.Count},
                {"checks", new JsonArray(checks.Select(c => (JsonValue)c).ToArray())},
                {"plugins", new JsonArray(BepInEx.Bootstrap.Chainloader.PluginInfos.Values.Select(p => (JsonValue)(p.Metadata.Name + " " + p.Metadata.Version)).ToArray())},
                {"error", error ?? ""}
            };
            File.WriteAllText(Path.Combine(directory, "runtime-tests.json"), report.ToString());
            if (error != null) Logger.LogError(error); else Logger.LogInfo("Snappy diagnostic passed: " + checks.Count + " checks.");
            Application.Quit();
        }

        void VerifyGrid()
        {
            for (int x = -128; x <= 128; x += 16)
            for (int y = -128; y <= 128; y += 16)
                Check(WorldFlowManager.ClampToGrid(new Vector2Int(x, y)) == new Vector2Int(x, y), "Exact quarter-grid position " + x + "," + y);
            int[] input = { -25, -24, -23, -9, -8, -7, 7, 8, 9, 23, 24, 25 };
            int[] expected = { -32, -32, -16, -16, 0, 0, 0, 0, 16, 16, 32, 32 };
            for (int i = 0; i < input.Length; i++)
                Check(WorldFlowManager.ClampToGrid(new Vector2Int(input[i], -input[i])) == new Vector2Int(expected[i], -expected[i]), "Nearest rounding with native midpoint rule " + input[i]);
            Check(WorldFlowManager.ClampToGrid(new Vector2Int(17, -17), 24) == new Vector2Int(16, 8), "Decoration label retains added 24-unit Y offset");
            for (int x = -96; x <= 96; x += 16)
                Check(WorldFlowPipeNode.ClampToGrid(new Vector2Int(x, 88)) == new Vector2Int(x, 88), "Pipe quarter-grid retains port Y alignment " + x);
            Check(WorldFlowPipeNode.ClampToGrid(new Vector2Int(19, 91)) == new Vector2Int(16, 88), "Pipe offset applied before rounding and removed afterward");
        }

        void VerifyNode(Manifold node, Manifold blocker, Keyboard keyboard)
        {
            var start = node.Position;
            var label = node.World.GetComponentInChildren<WorldFlowLabel>(true);
            AccessTools.Method(typeof(WorldFlowLabel), "_startDragDrop").Invoke(label, new object[] { false });
            Check(ReferenceEquals(DragDropManager.Instance.Target, label), "Native node drag starts");
            for (int step = 1; step <= 3; step++)
            {
                label.OnDragDropMove(start + new Vector2Int(step * 16 + 3, -step * 16 - 3), Vector2Int.zero);
                Check(node.Position == start + new Vector2Int(step * 16, -step * 16), "Node reaches extra snap position " + step);
            }
            Control(keyboard, true);
            label.OnDragDropMove(start + new Vector2Int(19, -27), Vector2Int.zero);
            Check(node.Position == start + new Vector2Int(19, -27), "Ctrl bypasses node snapping");
            Control(keyboard, false);
            label.OnDragDropCancel();
            Check(node.Position == start, "Cancel restores exact original node position");
            AccessTools.Method(typeof(WorldFlowLabel), "_startDragDrop").Invoke(label, new object[] { false });
            label.OnDragDropMove(start + new Vector2Int(19, -19), Vector2Int.zero);
            Physics2D.SyncTransforms();
            label.OnDragDropEnd(); DragDropManager.Instance.ClearDragDrop();
            Check(node.Position == start + new Vector2Int(16, -16), "Release commits quarter-grid node placement");
            var committed = node.Position;
            AccessTools.Method(typeof(WorldFlowLabel), "_startDragDrop").Invoke(label, new object[] { false });
            label.OnDragDropMove(blocker.Position, Vector2Int.zero); Physics2D.SyncTransforms();
            label.OnDragDropEnd(); DragDropManager.Instance.ClearDragDrop();
            Check(node.Position == committed, "Native collision rejection restores committed fine position");
        }

        void VerifyPipe(FlowPipe pipe, Manifold source, Manifold sink, Keyboard keyboard)
        {
            var bend = pipe.World.Nodes.First();
            var start = bend.Position;
            bend.StartDragDrop(false);
            for (int step = 1; step <= 3; step++)
            {
                bend.OnDragDropMove(start + new Vector2Int(step * 16 + 2, step * 16 + 2), Vector2Int.zero);
                Check(bend.Position == start + new Vector2Int(step * 16, step * 16), "Pipe reaches extra snap position " + step);
                At(pipe.World.GetComponentInChildren<LineRenderer>().GetPosition(1), bend.Position, "Rendered pipe follows fine bend " + step);
            }
            Control(keyboard, true);
            bend.OnDragDropMove(new Vector2Int(21, 99), Vector2Int.zero);
            Check(bend.Position == new Vector2Int(21, 99), "Ctrl bypasses pipe snapping");
            Control(keyboard, false); bend.OnDragDropCancel();
            Check(bend.Position == start, "Cancel restores original pipe bend");
            bend.StartDragDrop(false); bend.OnDragDropMove(new Vector2Int(19, 107), Vector2Int.zero);
            bend.OnDragDropEnd(); DragDropManager.Instance.ClearDragDrop();
            Check(bend.Position == new Vector2Int(16, 104), "Pipe bend commit preserves quarter step and native offset");
            Check(pipe.ConnectedOutput == source.Outputs[0] && pipe.ConnectedInput == sink.Inputs[0], "Pipe connections remain attached after fine movement");
        }

        void VerifyGhost(Manifold source, Keyboard keyboard)
        {
            var ghost = WorldFlowManager.Instance.ShowPurchaseBlueprint(source, false);
            ghost.OnDragDropMove(new Vector2Int(-653, -397), Vector2Int.zero);
            At(((RectTransform)ghost.transform).anchoredPosition, new Vector2(-656, -400), "Copy preview snaps to nearest quarter grid");
            Control(keyboard, true);
            ghost.OnDragDropMove(new Vector2Int(-653, -397), Vector2Int.zero);
            At(((RectTransform)ghost.transform).anchoredPosition, new Vector2(-653, -397), "Ctrl bypasses preview snapping");
            Control(keyboard, false);
            ghost.OnDragDropMove(new Vector2Int(-653, -397), Vector2Int.zero);
            int count = GamePlayer.Current.ActiveFlow.Elements.Count;
            Physics2D.SyncTransforms();
            DragDropManager.Instance.ClearDragDrop(); AccessTools.Method(typeof(WorldFlowGhost), "Update").Invoke(ghost, null);
            ghost.OnDragDropEnd();
            Check(GamePlayer.Current.ActiveFlow.Elements.Count == count + 1, "Native copy placement creates node");
            Check(GamePlayer.Current.ActiveFlow.Elements.Last().Position == new Vector2Int(-656, -400), "Placed node exactly matches fine-grid preview");
            WorldFlowManager.Instance.StopBlueprintMode(); DragDropManager.Instance.ClearDragDrop();
        }

        void VerifyDecoration(Keyboard keyboard)
        {
            var deco = new FlowDeco { Position = new Vector2Int(256, -256), Size = new Vector2Int(192, 192) };
            GamePlayer.Current.ActiveFlow.Decorations.Add(deco);
            var world = WorldFlowManager.Instance.ShowDeco(deco);
            var mover = world.GetComponentInChildren<WorldFlowDecoMover>(true);
            mover.StartDragDrop(false);
            mover.OnDragDropMove(new Vector2Int(275, -269), Vector2Int.zero);
            Check(deco.Position == new Vector2Int(272, -272), "Container moves on fine grid");
            Control(keyboard, true); mover.OnDragDropMove(new Vector2Int(275, -269), Vector2Int.zero);
            Check(deco.Position == new Vector2Int(275, -269), "Ctrl bypasses decoration snapping");
            Control(keyboard, false); mover.OnDragDropCancel();
            Check(deco.Position == new Vector2Int(256, -256), "Decoration cancel restores original placement");
            var resizer = world.GetComponentsInChildren<WorldFlowDecoResizer>(true).First();
            resizer.StartDragDrop(false);
            resizer.OnDragDropMove(new Vector2Int(483, -483), Vector2Int.zero);
            Check(deco.Position.x % 16 == 0 && deco.Position.y % 16 == 0 && deco.Size.x % 16 == 0 && deco.Size.y % 16 == 0, "Native container resize uses quarter grid");
            Check(deco.Size.x >= 64 && deco.Size.y >= 64, "Native minimum container size preserved");
            resizer.OnDragDropCancel();
            Check(deco.Size == new Vector2Int(192, 192), "Resize cancel restores original size");
            var label = new FlowDeco { Label = "Snappy", Position = new Vector2Int(0, -600), Size = new Vector2Int(192, 64) };
            var ghost = WorldFlowManager.Instance.ShowDecoBlueprint(label, false, false);
            ghost.OnDragDropMove(new Vector2Int(19, -603), Vector2Int.zero);
            At(((RectTransform)ghost.transform).anchoredPosition, new Vector2(16, -584), "Decoration label preview retains native 24-unit offset");
            WorldFlowManager.Instance.StopBlueprintMode(); DragDropManager.Instance.ClearDragDrop();
        }

        void VerifyPersistenceAndRemoval(Manifold source, FlowPipe pipe)
        {
            string json = GamePlayer.Current.ActiveFlow.ToJson().ToString();
            var restored = FlowDiagram.FromJson(JsonValue.Parse(json));
            Check(restored.Elements.First().Position == source.Position, "Native JSON round trip retains fine node position");
            Check(restored.Pipes.First().Nodes.SequenceEqual(pipe.Nodes), "Native JSON round trip retains fine pipe bends");
            var method = AccessTools.Method(typeof(WorldFlowManager), nameof(WorldFlowManager.ClampToGrid));
            harmony.Unpatch(method, HarmonyPatchType.All, Id);
            Check(WorldFlowManager.ClampToGrid(new Vector2Int(19, -19)) == Vector2Int.zero, "Removing Snappy restores native 64-unit grid");
            var without = FlowDiagram.FromJson(JsonValue.Parse(json));
            Check(without.Elements.First().Position == source.Position && without.Pipes.First().Nodes.SequenceEqual(pipe.Nodes), "Fine layout loads unchanged without Snappy patch");
            harmony.CreateClassProcessor(typeof(FineGrid)).Patch();
            Check(WorldFlowManager.ClampToGrid(new Vector2Int(19, -19)) == new Vector2Int(16, -16), "Reapplying Snappy restores fine snapping");
        }

        static string HashFile(string path)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            using (var stream = File.OpenRead(path)) return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.Store))] static class NoSave { static bool Prefix() => false; }
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.StoreAutosaveState))] static class NoAutosave { static bool Prefix() => false; }
        [HarmonyPatch(typeof(SaveGame), nameof(SaveGame.LoadLatestSave))] static class NoLoad
        { static bool Prefix(ref bool __result) { __result = false; return false; } }
        [HarmonyPatch(typeof(SteamAchievement), nameof(SteamAchievement.Trigger))] static class NoAchievement { static bool Prefix() => false; }
        [HarmonyPatch(typeof(SteamManager), nameof(SteamManager.Initialized), MethodType.Getter)] static class Offline
        { static bool Prefix(ref bool __result) { __result = false; return false; } }
    }
}
