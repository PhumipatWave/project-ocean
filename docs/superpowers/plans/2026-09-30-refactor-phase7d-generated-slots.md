# Refactor Phase 7d: Inventory and Shop Slot Grids Are Generated From a Slot Count — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** The 14 hand-placed player inventory slots (`AS_PlayerHUD`) and the 14 hand-placed shop sell-mirror slots (`FishShopPanel.prefab`) are replaced by one slot prefab plus a count. `InventoryManager` builds its grid in `Awake`; `ShopManager` builds a matching mirror grid from `InventoryManager.PlayerSlotCount`. Changing the inventory size becomes one Inspector number.

**Architecture:** Last step of 7b → 7c → 7d. After 7c the slot views hold no data (the model does), so generating them is safe. A tiny `SlotBuilder` instantiates `N` slots into a `GridLayoutGroup` container in index order (slot `i` = sibling `i`, which is also the save `slotIndex`). A prefab variant `Slot_Items_Grid.prefab` is made from an existing scene slot so generated slots look exactly like today's (the scene instances override the base prefab's sprite).

**Tech Stack:** Unity 6000.3.6f1, C#, NUnit EditMode tests, Unity MCP (`execute_code`, `refresh_unity`, `read_console`, `run_tests`), Python 3 (`tools/uml`).

## Facts this plan relies on (checked 2026-09-30 in the YAML)

- `AS_PlayerHUD`: `InventoryManager.inventorySlots` = 14 instances of `Prefabs/Buttons/Inventory/Slot_Items.prefab` (guid `3b971edacb3fed845ba3c8164e940a38`), all children of scene object `ItemsSlot` (has `GridLayoutGroup`). Array order == sibling order (0..13), so generated slot `i` lands where old slot `i` was and old saves map 1:1.
- Every one of those 28 instances (14 HUD + 14 shop) overrides `Image.m_Sprite` to none. The base `Slot_Items.prefab` has a sprite and is also used by `WeaponUIPanel.prefab` (the building slot), so the base prefab is **not** edited; a variant is created instead.
- Shop: `ShopManager` is in `Land_Map`; its `shopInventorySlots` = 14 `Slot_Items` instances named `InShopSlot_Items (n)` inside `FishShopPanel.prefab` → `ItemsSlot` (`GridLayoutGroup`), each overriding `isShopSlot = 1`.
- The building slot (1 slot inside `WeaponUIPanel.prefab`) and its hint panels stay hand-placed: it is a fixed HUD element, not a grid.

## Success criteria (all must hold at the end)

1. `grep -c "3b971edacb3fed845ba3c8164e940a38" Assets/Project_Ocean/Levels/Scenes/Menus/AS_PlayerHUD.unity Assets/Project_Ocean/Prefabs/UI/Land_UI_Panel/FishShopPanel/FishShopPanel.prefab` → `0` for both (no hand-placed `Slot_Items` left).
2. `Assets/Project_Ocean/Prefabs/Buttons/Inventory/Slot_Items_Grid.prefab` exists; `InventoryManager.slotPrefab` and `ShopManager.shopSlotPrefab` point to it.
3. EditMode tests: all pass, baseline B + 2.
4. Live: 14 inventory slots and 14 shop slots are generated in the same places as before; an old save loads into the same slots; buy/sell/drag/discard/upgrade work; `git status` shows only the intended scene/prefab files changed.

## Global Constraints

- Run after Phase 7c is committed.
- No behaviour or visual change. Slot count stays 14; `firstUnlockedSlotCount` stays 3.
- Save format unchanged. `playerSlotCount` must never go below the highest saved `slotIndex` + 1 (14 today), or those items are dropped on load (logged by `InventorySaveMapper`).
- Scene/prefab edits only in `AS_PlayerHUD.unity`, `Land_Map.unity`, `FishShopPanel.prefab`, and the new `Slot_Items_Grid.prefab`. Edit them through `execute_code` + `EditorSceneManager`/`PrefabUtility` (not `manage_scene save`: memory `feedback_unity_mcp_scene_save_bug.md`). After each save run `git status --short -- "*.unity" "*.prefab"` and confirm the path.
- Before a scene/prefab step: Unity not in Play mode (`mcpforunity://editor/state` → `is_playing == false`) and the target scene has no unsaved edits (if dirty, ask the user to save first).
- Files keep their encoding, BOM and line endings.
- CLAUDE.md comment rules: 1–2 lines, why not what, XML `<summary>` on public API only, 1 sentence.
- The user commits. Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`. "Compile" means `refresh_unity(mode=force, compile=request)` then `read_console(filter_text="error CS")` = 0 entries and `EditorUtility.scriptCompilationFailed == false`. "Run tests" means `run_tests(mode=EditMode)` + `get_test_job(wait_timeout=60)`.
- Back up `C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean` before any Play session and restore it byte-identical after (RESTORED).

---

### Task 1: `SlotBuilder` (TDD)

**Files:**
- Create: `Assets/Project_Ocean/Scripts/UI/Inventory/SlotBuilder.cs`
- Create: `Assets/Project_Ocean/Tests/EditMode/SlotBuilderTests.cs`

**Interfaces:**
- Produces: `Game.UI.SlotBuilder.Build(InventorySlot prefab, Transform container, int count, string namePrefix) → InventorySlot[]` — slot `i` is named `"{namePrefix} ({i})"` and is the container's `i`-th new child; missing prefab/container logs an error and returns an empty array.

- [ ] **Step 1: Record baseline** — run tests, write the count as **B**.

- [ ] **Step 2: Write the failing tests**

`Assets/Project_Ocean/Tests/EditMode/SlotBuilderTests.cs`:
```csharp
using System.Text.RegularExpressions;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Game.Tests
{
    public class SlotBuilderTests
    {
        private GameObject template;
        private GameObject container;

        [SetUp]
        public void SetUp()
        {
            template = new GameObject("SlotBuilderTests_Template", typeof(RectTransform), typeof(InventorySlot));
            container = new GameObject("SlotBuilderTests_Container", typeof(RectTransform));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(container);
            Object.DestroyImmediate(template);
        }

        [Test]
        public void Build_CreatesCountSlots_InSiblingOrder()
        {
            InventorySlot[] slots = SlotBuilder.Build(template.GetComponent<InventorySlot>(), container.transform, 3, "Slot");

            Assert.AreEqual(3, slots.Length);
            for (int i = 0; i < 3; i++)
            {
                Assert.AreSame(container.transform, slots[i].transform.parent);
                Assert.AreEqual(i, slots[i].transform.GetSiblingIndex());
                Assert.AreEqual($"Slot ({i})", slots[i].name);
            }
        }

        [Test]
        public void Build_WithoutPrefab_LogsErrorAndReturnsEmpty()
        {
            LogAssert.Expect(LogType.Error, new Regex("slot prefab or container not set"));
            Assert.AreEqual(0, SlotBuilder.Build(null, container.transform, 3, "Slot").Length);
        }
    }
}
```

- [ ] **Step 3: Compile — expect red** (`CS0103: The name 'SlotBuilder' does not exist in the current context`).

- [ ] **Step 4: Create `SlotBuilder`**

`Assets/Project_Ocean/Scripts/UI/Inventory/SlotBuilder.cs`:
```csharp
using UnityEngine;

namespace Game.UI
{
    /// <summary>Fills a layout container with slot instances; slot i is the container's i-th new child.</summary>
    public static class SlotBuilder
    {
        public static InventorySlot[] Build(InventorySlot prefab, Transform container, int count, string namePrefix)
        {
            if (prefab == null || container == null)
            {
                Debug.LogError($"[SlotBuilder] {namePrefix}: slot prefab or container not set");
                return new InventorySlot[0];
            }

            var slots = new InventorySlot[count];
            for (int i = 0; i < count; i++)
            {
                slots[i] = Object.Instantiate(prefab, container);
                slots[i].name = $"{namePrefix} ({i})";
            }
            return slots;
        }
    }
}
```

- [ ] **Step 5: Compile + run tests** — 0 errors; B + 2 pass.

- [ ] **Step 6: Commit (user)** — suggested: `feat: SlotBuilder builds slot grids from a prefab and count`.

---

### Task 2: Inventory grid is generated (`InventoryManager` + `AS_PlayerHUD`)

**Files:**
- Modify: `Assets/Project_Ocean/Scripts/UI/Inventory/InventoryManager.cs` (slot fields, `CreateModel`)
- Create (Editor, via MCP): `Assets/Project_Ocean/Prefabs/Buttons/Inventory/Slot_Items_Grid.prefab`
- Modify (Editor, via MCP): `Assets/Project_Ocean/Levels/Scenes/Menus/AS_PlayerHUD.unity`

**Interfaces:**
- Consumes: `SlotBuilder.Build` (Task 1).
- Produces: serialized fields `InventoryManager.slotPrefab` (`InventorySlot`), `slotContainer` (`Transform`), `playerSlotCount` (`int`, 14). `inventorySlots` stays as a private runtime array (same name, so the 7c code using it is unchanged). `PlayerSlotCount` still comes from the model, so `ShopManager` (Task 3) reads the generated count.

- [ ] **Step 1: Before-picture**

Ask the user to open the inventory in Play mode once and take a screenshot (for the Task 4 visual compare). Then exit Play and restore saves (RESTORED).

- [ ] **Step 2: Code — replace the slot fields**

In `InventoryManager.cs` replace:
```csharp
        [Header("Inventory Slots")]
        [SerializeField] private InventorySlot[] inventorySlots;
        [SerializeField] private int firstUnlockedSlotCount = 3;
        [SerializeField] private List<InventorySlot> buildingSlots;
```
with:
```csharp
        [Header("Inventory Slots")]
        [SerializeField] private InventorySlot slotPrefab;
        [SerializeField] private Transform slotContainer;
        // Saves store slot indices: going below an old save's highest index drops those items on load.
        [SerializeField, Min(1)] private int playerSlotCount = 14;
        [SerializeField] private int firstUnlockedSlotCount = 3;
        [SerializeField] private List<InventorySlot> buildingSlots;

        private InventorySlot[] inventorySlots;
```
In `CreateModel()`, add as the first line:
```csharp
            inventorySlots = SlotBuilder.Build(slotPrefab, slotContainer, playerSlotCount, "Slot_Items");
```

- [ ] **Step 3: Compile + run tests** — 0 errors; B + 2. Do **not** enter Play mode before Step 4 (the scene still has the old slots and no prefab wired).

- [ ] **Step 4: Scene — make the variant, delete the 14 slots, wire the fields**

Unity MCP `execute_code`:
```csharp
const string scenePath   = "Assets/Project_Ocean/Levels/Scenes/Menus/AS_PlayerHUD.unity";
const string variantPath = "Assets/Project_Ocean/Prefabs/Buttons/Inventory/Slot_Items_Grid.prefab";
var invType  = System.Type.GetType("Game.UI.InventoryManager, Game.UI");
var slotType = System.Type.GetType("Game.UI.InventorySlot, Game.UI");
if (invType == null || slotType == null) return "types not found";

var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scenePath);
bool openedHere = !scene.isLoaded;
if (openedHere)
    scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);

Component inv = null;
Transform container = null;
foreach (var root in scene.GetRootGameObjects())
{
    if (inv == null) inv = root.GetComponentInChildren(invType, true);
    foreach (var t in root.GetComponentsInChildren<Transform>(true))
    {
        if (t.name != "ItemsSlot" || t.GetComponent<UnityEngine.UI.GridLayoutGroup>() == null) continue;
        if (t.childCount == 0 || t.GetChild(0).GetComponent(slotType) == null) continue;
        if (container != null) return "more than one ItemsSlot grid with slots";
        container = t;
    }
}
if (inv == null || container == null) return "InventoryManager or ItemsSlot not found";

// The scene slots override the base prefab (sprite none); a variant keeps that look.
var variant = UnityEditor.PrefabUtility.SaveAsPrefabAsset(container.GetChild(0).gameObject, variantPath);
if (variant == null) return "variant save failed";

int removed = 0;
for (int i = container.childCount - 1; i >= 0; i--)
{
    var child = container.GetChild(i);
    if (child.GetComponent(slotType) == null) continue;
    UnityEngine.Object.DestroyImmediate(child.gameObject);
    removed++;
}

var so = new UnityEditor.SerializedObject(inv);
so.FindProperty("slotPrefab").objectReferenceValue = variant.GetComponent(slotType);
so.FindProperty("slotContainer").objectReferenceValue = container;
so.FindProperty("playerSlotCount").intValue = removed;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
bool saved = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
var image = variant.GetComponent<UnityEngine.UI.Image>();
string result = $"removed {removed}; variant {UnityEditor.PrefabUtility.GetPrefabAssetType(variant)}; " +
                $"sprite {(image != null && image.sprite != null ? image.sprite.name : "none")}; saved {saved}";
if (openedHere) UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
return result;
```
Expected: `removed 14; variant Variant; sprite none; saved True` (`Regular` instead of `Variant` is also fine; the look is what matters). Anything else → stop and report.

- [ ] **Step 5: Check the files**

```bash
git status --short -- "*.unity" "*.prefab"
grep -c "3b971edacb3fed845ba3c8164e940a38" Assets/Project_Ocean/Levels/Scenes/Menus/AS_PlayerHUD.unity
grep -n "playerSlotCount\|slotPrefab\|slotContainer" Assets/Project_Ocean/Levels/Scenes/Menus/AS_PlayerHUD.unity
```
Expected: `AS_PlayerHUD.unity` modified at its real path (no new `Assets/AS_PlayerHUD.unity`), `Slot_Items_Grid.prefab` new; guid count `0`; the three fields present with `playerSlotCount: 14`.

- [ ] **Step 6: Quick Play check** (back up saves) — enter Play, load slot 0, then `execute_code`:
```csharp
var inv = Game.UI.InventoryManager.Instance;
return $"slots {inv.PlayerSlotCount}; first {inv.GetStack(0).Item?.ItemName ?? "empty"}";
```
Expected: `slots 14`, and the stacks match what the save held (compare with `SaveBridge` data or the before-picture). Exit Play, restore saves (RESTORED), scene-dirty check.

- [ ] **Step 7: Commit (user)** — suggested: `refactor: inventory slots generated from Slot_Items_Grid + playerSlotCount`.

---

### Task 3: Shop mirror grid is generated (`ShopManager` + `FishShopPanel` + `Land_Map`)

**Files:**
- Modify: `Assets/Project_Ocean/Scripts/UI/Shop/ShopManager.cs`
- Modify (Editor, via MCP): `Assets/Project_Ocean/Prefabs/UI/Land_UI_Panel/FishShopPanel/FishShopPanel.prefab`, `Assets/Project_Ocean/Levels/Scenes/Lands/Land_Map.unity`

**Interfaces:**
- Consumes: `SlotBuilder.Build`, `Slot_Items_Grid.prefab` (Tasks 1–2), `InventoryManager.PlayerSlotCount`.
- Produces: serialized `ShopManager.shopSlotPrefab` (`InventorySlot`), `shopSlotContainer` (`Transform`); `shopInventorySlots` becomes a private runtime array (same name, so the rest of `ShopManager` is unchanged).

- [ ] **Step 1: Code — replace the mirror slot fields and build them lazily**

In `ShopManager.cs` replace:
```csharp
        [Header("Shop Slots Sell Display")]
        [SerializeField] private InventorySlot[] shopInventorySlots;
        [SerializeField] private GameObject inventoryItemPrefab;
```
with:
```csharp
        [Header("Shop Slots Sell Display")]
        [SerializeField] private InventorySlot shopSlotPrefab;
        [SerializeField] private Transform shopSlotContainer;
        [SerializeField] private GameObject inventoryItemPrefab;
        private InventorySlot[] shopInventorySlots = new InventorySlot[0];
```
In `RefreshShopInventory()`, directly after `if (inventory == null) return;` add:
```csharp
            EnsureShopSlots(inventory.PlayerSlotCount);
```
Add below `RefreshShopInventory()`:
```csharp
        // Built on first refresh, not Awake: the count comes from the persistent InventoryManager.
        private void EnsureShopSlots(int count)
        {
            if (shopInventorySlots.Length == count) return;

            foreach (InventorySlot old in shopInventorySlots)
                if (old != null) Destroy(old.gameObject);

            shopInventorySlots = SlotBuilder.Build(shopSlotPrefab, shopSlotContainer, count, "InShopSlot_Items");
            foreach (InventorySlot slot in shopInventorySlots)
                slot.isShopSlot = true;
        }
```
(`DeselectAllShopSlots`, `GetSelectedSlot`, `GetShopSlotIndex`, `ClearShopInventoryMirror` loop over `shopInventorySlots`; an empty array before the first refresh is safe for all of them.)

- [ ] **Step 2: Compile + run tests** — 0 errors; B + 2. No Play before Step 3.

- [ ] **Step 3: Prefab + scene — delete the 14 shop slots, wire the fields**

Unity MCP `execute_code`:
```csharp
const string prefabPath  = "Assets/Project_Ocean/Prefabs/UI/Land_UI_Panel/FishShopPanel/FishShopPanel.prefab";
const string scenePath   = "Assets/Project_Ocean/Levels/Scenes/Lands/Land_Map.unity";
const string variantPath = "Assets/Project_Ocean/Prefabs/Buttons/Inventory/Slot_Items_Grid.prefab";
var shopType = System.Type.GetType("Game.UI.ShopManager, Game.UI");
var slotType = System.Type.GetType("Game.UI.InventorySlot, Game.UI");
if (shopType == null || slotType == null) return "types not found";

// 1. Prefab asset: a scene instance can't delete children that belong to its prefab.
var prefabRoot = UnityEditor.PrefabUtility.LoadPrefabContents(prefabPath);
string containerPath = null;
int removed = 0;
foreach (var t in prefabRoot.GetComponentsInChildren<Transform>(true))
{
    if (t.GetComponent<UnityEngine.UI.GridLayoutGroup>() == null || t.childCount == 0 || t.GetChild(0).GetComponent(slotType) == null) continue;
    if (containerPath != null) { UnityEditor.PrefabUtility.UnloadPrefabContents(prefabRoot); return "more than one slot grid in FishShopPanel"; }
    containerPath = UnityEditor.AnimationUtility.CalculateTransformPath(t, prefabRoot.transform);
    for (int i = t.childCount - 1; i >= 0; i--)
    {
        if (t.GetChild(i).GetComponent(slotType) == null) continue;
        UnityEngine.Object.DestroyImmediate(t.GetChild(i).gameObject);
        removed++;
    }
}
if (containerPath == null) { UnityEditor.PrefabUtility.UnloadPrefabContents(prefabRoot); return "shop slot grid not found"; }
UnityEditor.PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
UnityEditor.PrefabUtility.UnloadPrefabContents(prefabRoot);

// 2. Scene: point ShopManager at the (now empty) grid inside the FishShopPanel instance.
var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scenePath);
bool openedHere = !scene.isLoaded;
if (openedHere)
    scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);

Component shop = null;
Transform container = null;
foreach (var root in scene.GetRootGameObjects())
{
    if (shop == null) shop = root.GetComponentInChildren(shopType, true);
    foreach (var t in root.GetComponentsInChildren<Transform>(true))
    {
        if (!UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(t.gameObject)) continue;
        if (UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestPrefabInstanceRoot(t.gameObject) != prefabPath) continue;
        var c = t.Find(containerPath);
        if (c == null) continue;
        if (container != null) return "two FishShopPanel instances";
        container = c;
    }
}
if (shop == null || container == null) return "ShopManager or shop grid not found in Land_Map";

var variant = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(variantPath);
var so = new UnityEditor.SerializedObject(shop);
so.FindProperty("shopSlotPrefab").objectReferenceValue = variant.GetComponent(slotType);
so.FindProperty("shopSlotContainer").objectReferenceValue = container;
so.ApplyModifiedPropertiesWithoutUndo();

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
bool saved = UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
if (openedHere) UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
return $"removed {removed} from '{containerPath}'; wired; saved {saved}";
```
Expected: `removed 14 from '<…>/ItemsSlot'; wired; saved True`.

- [ ] **Step 4: Check the files**

```bash
git status --short -- "*.unity" "*.prefab"
grep -c "3b971edacb3fed845ba3c8164e940a38" Assets/Project_Ocean/Prefabs/UI/Land_UI_Panel/FishShopPanel/FishShopPanel.prefab
grep -n "shopSlotPrefab\|shopSlotContainer" Assets/Project_Ocean/Levels/Scenes/Lands/Land_Map.unity
```
Expected: `FishShopPanel.prefab` and `Land_Map.unity` modified at their real paths; guid count `0`; both fields present with non-zero `fileID`.

- [ ] **Step 5: Commit (user)** — suggested: `refactor: shop sell-mirror slots generated to match the inventory`.

---

### Task 4: Verify live, docs

**Files:** `docs/uml/findings.md` (new `### Phase 7d` under §4), regenerated `docs/uml/*`, `graphify-out/*`.

- [ ] **Step 1: Live smoke** (back up saves; route in memory `reference_live_enemy_test_route.md`; `Application.runInBackground = true` after entering Play)
  1. Load a save made **before** 7d (the backup) → `GetStack(i)` for every `i` matches the save's `inventoryItems` (slotIndex → same item and count). Building slot unchanged.
  2. Inventory open: 14 slots named `Slot_Items (0..13)` under `ItemsSlot`; only the unlocked ones visible; user compares with the Task 2 before-picture (same size, sprite, spacing).
  3. Fish shop (`ShopManager.Instance.OpenFishShopWindow()`): 14 `InShopSlot_Items (n)` with `isShopSlot == true`; click one → selected + price shown; `Sell()` and `SellAll()` work; mirror matches the inventory.
  4. Buy a Mini Hub and a Normal item; drag between slots; discard via bin; upgrade inventory once → one more slot in both grids.
  5. Optional (user): in Edit mode set `playerSlotCount` to 16 without saving the scene, Play → 16 inventory and 16 shop slots; exit Play and revert the value (don't save).
  6. Console: no new errors. Exit Play, restore saves (RESTORED). `git status --short -- "*.unity" "*.prefab"` shows only the files changed in Tasks 2–3 (plus anything that was already modified before 7d).

- [ ] **Step 2: Measure + docs**

```bash
python tools/uml/gen_uml.py && python tools/uml/test_gen_uml.py
```
`findings.md` §4, new `### Phase 7d` with a **Status** line: done date, the variant prefab, the two generated grids, how to change the size (`InventoryManager.playerSlotCount`, never below 14 for existing saves), and that the building slot stays hand-placed. Then `graphify update .`.

- [ ] **Step 3: Hand-off (user commits)** — list changed code, scene and prefab files; suggested messages in Tasks 1–3.
