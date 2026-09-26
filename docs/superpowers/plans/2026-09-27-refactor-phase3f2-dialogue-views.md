# Refactor Phase 3f-2: Dialogue, Quest-Offer and Skip-Prompt Views — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Gameplay classes stop driving UI directly:
- `NPC`, `ExploreAreaTrigger` and `PlayerSelfDialogueUI` fill a dialogue panel through `IDialogueView`, implemented by a new UI component `DialoguePanelView`, instead of holding TMP/Image fields and calling `DialogueSpeaker`.
- `NPC` opens the quest offer through `IQuestOfferView`, implemented by `QuestDetailPanel`.
- `Cutscene` asks "Skip this cutscene?" through `IModalPrompt`, implemented by `ConfirmationDialog`.

This removes the last 6 non-inventory wrong-way links with identical behaviour.

**Architecture:**
- **Contracts** live in `Gameplay/Contracts/`: `IDialogueView`, `IQuestOfferView`, `IModalPrompt` and the `ModalPrompt` access point.
- **Field retype:** serialized fields that point at UI objects are retyped to `MonoBehaviour` under the same name, so every existing scene and prefab reference survives. `NPC.questDetailPanel` needs no migration because of this.
- **Dialogue fields are a real migration:** 4 UI fields become 1 view reference. It runs in three steps:
  1. Add the view field next to the old fields.
  2. An editor migration puts one `DialoguePanelView` on each referenced panel GameObject (the scene's shared dialogue panel), copies the old field values into it, and assigns it. A verification pass proves every object is wired.
  3. Only then does the code switch to the view and the old fields go.

**Tech Stack:** Unity 6000.3.6f1 + Unity MCP (`execute_code` with `UnityEditor` APIs: `PrefabUtility`, `EditorSceneManager`, `SerializedObject`), NUnit EditMode.

## Global Constraints

- The user chose "Do it, with prefab rewiring" on 2026-09-27.
- Behaviour identical: same panel shows or hides at the same moments, same speaker name/portrait resolution (`DialogueSpeaker` rules), same text, same `DialogueState` calls, same skip-prompt conditions (no prompt while a `UIManager` menu is open).
- Scene/prefab edits only through the migration script in Task 2. Save scenes with `EditorSceneManager.SaveScene(scene)` inside `execute_code`, never `manage_scene save`. After each save, `git status --short` must show the expected `.unity`/`.prefab` paths only (memory `feedback_unity_mcp_scene_save_bug.md`).
- Do not edit the Bootstrap scene's managers (memory `project_bootstrap_scene_not_integrated.md`); migrate only where NPC/ExploreAreaTrigger/PlayerSelfDialogueUI components actually exist.
- Claude does not run `git commit`, `git push`, `git add`, `git stash`, `git restore`, `git checkout` or `git mv`.
- Back up the save folder `C:/Users/Admin/AppData/LocalLow/DefaultCompany/project-ocean` before Play and restore it byte-identical after.
- Unity MCP: `set_active_instance` = `project-ocean@7db8e111`; retry once on a transient timeout.
- Comment rules (CLAUDE.md).

---

### Task 1: Contracts, quest-offer and skip-prompt (no migration needed)

**Files (relative to `Assets/Project_Ocean/Scripts/`):**
- Create: `Gameplay/Contracts/IDialogueView.cs`, `Gameplay/Contracts/IQuestOfferView.cs`, `Gameplay/Contracts/IModalPrompt.cs` (create the folder via `AssetDatabase.CreateFolder("Assets/Project_Ocean/Scripts/Gameplay","Contracts")` first)
- Modify: `UI/Quest/QuestDetailPanel.cs`, `UI/ConfirmationDialog.cs`, `Gameplay/Entities/NPCs/NPC.cs` (only `questDetailPanel`), `Gameplay/Cutscenes/Cutscene.cs`
- Test: `Assets/Project_Ocean/Tests/EditMode/ViewContractTests.cs`

**Interfaces:**
- Produces:
  - `IDialogueView { GameObject Panel { get; } bool IsOpen { get; } void SetOpen(bool open); void SetSpeaker(string characterId, string fallbackName = null, Sprite fallbackPortrait = null); void SetText(string text); }`
  - `IQuestOfferView { void ShowPanel(Quest quest, System.Action onStatusChanged); void HidePanel(); }`
  - `IModalPrompt { bool CanShow { get; } void Show(string message, System.Action onConfirm, System.Action onCancel = null); void Cancel(); }`
  - `static class ModalPrompt { IModalPrompt Current { get; } void Register(IModalPrompt); void Unregister(IModalPrompt); }`

- [ ] **Step 1: Failing test**

```csharp
using Game.Testing;
using NUnit.Framework;

namespace Game.Tests
{
    public class ViewContractTests
    {
        [Test]
        public void UiComponents_ImplementGameplayContracts()
        {
            Assert.IsTrue(typeof(IQuestOfferView).IsAssignableFrom(typeof(QuestDetailPanel)));
            Assert.IsTrue(typeof(IModalPrompt).IsAssignableFrom(typeof(ConfirmationDialog)));
        }

        [Test]
        public void ModalPrompt_OnlyRegisteredInstanceCanUnregister()
        {
            ModalPrompt.Register(null);
            Assert.IsNull(ModalPrompt.Current);
        }
    }
}
```
`refresh_unity` + `read_console` `error CS`. Expected red: `CS0246` for `IQuestOfferView`, `IModalPrompt`, and `CS0103` for `ModalPrompt`.

- [ ] **Step 2: Contracts**

`Gameplay/Contracts/IDialogueView.cs`:
```csharp
using UnityEngine;

namespace Game.Testing
{
    /// <summary>A dialogue panel gameplay can open, label and fill without referencing UI types.</summary>
    public interface IDialogueView
    {
        GameObject Panel { get; }
        bool IsOpen { get; }
        void SetOpen(bool open);
        void SetSpeaker(string characterId, string fallbackName = null, Sprite fallbackPortrait = null);
        void SetText(string text);
    }
}
```
`Gameplay/Contracts/IQuestOfferView.cs`:
```csharp
namespace Game.Testing
{
    /// <summary>The quest accept/reject panel an NPC opens after its offer dialogue.</summary>
    public interface IQuestOfferView
    {
        void ShowPanel(Quest quest, System.Action onStatusChanged);
        void HidePanel();
    }
}
```
`Gameplay/Contracts/IModalPrompt.cs`:
```csharp
namespace Game.Testing
{
    /// <summary>A yes/no prompt gameplay can raise; CanShow is false while a menu owns the screen.</summary>
    public interface IModalPrompt
    {
        bool CanShow { get; }
        void Show(string message, System.Action onConfirm, System.Action onCancel = null);
        void Cancel();
    }

    /// <summary>Access point to the live modal prompt (registered by ConfirmationDialog).</summary>
    public static class ModalPrompt
    {
        public static IModalPrompt Current { get; private set; }

        public static void Register(IModalPrompt prompt) => Current = prompt;

        // Only the registered instance may clear it, so a destroyed duplicate can't wipe the live one.
        public static void Unregister(IModalPrompt prompt)
        {
            if (Current == prompt) Current = null;
        }
    }
}
```

- [ ] **Step 3: `QuestDetailPanel` and `ConfirmationDialog` implement them**

- `UI/Quest/QuestDetailPanel.cs`: change `public class QuestDetailPanel : MonoBehaviour` to `public class QuestDetailPanel : MonoBehaviour, IQuestOfferView`. `ShowPanel(Quest, Action)` and `HidePanel()` already exist.
- `UI/ConfirmationDialog.cs`: change the declaration to `public class ConfirmationDialog : MonoBehaviour, IModalPrompt`. In `Awake`, directly after `Instance = this;`, add `            ModalPrompt.Register(this);`. Add:
```csharp
        // A menu (pause, settings) owns the screen — don't stack the prompt over it.
        public bool CanShow => UIManager.Instance == null || !UIManager.Instance.IsUIOpen;

        private void OnDestroy()
        {
            if (Instance == this)
                ModalPrompt.Unregister(this);
        }
```
(If `OnDestroy` already exists, add the `if` block to it.) `Show(string, Action, Action onCancel = null)` and `Cancel()` already exist.

- [ ] **Step 4: `NPC.questDetailPanel` via `IQuestOfferView` (retype, same name)**

In `NPC.cs` replace:
```csharp
        [SerializeField] private QuestDetailPanel questDetailPanel;
```
with:
```csharp
        // Typed as MonoBehaviour (an IQuestOfferView) so gameplay doesn't reference the UI class; same name keeps the reference.
        [SerializeField] private MonoBehaviour questDetailPanel;
```
Replace:
```csharp
                    if (questDetailPanel != null)
                        questDetailPanel.ShowPanel(npcQuest, null);
```
with:
```csharp
                    if (questDetailPanel != null)
                        (questDetailPanel as IQuestOfferView)?.ShowPanel(npcQuest, null);
```
Replace:
```csharp
            if (HasQuest && questDetailPanel != null)
                questDetailPanel.HidePanel();
```
with:
```csharp
            if (HasQuest && questDetailPanel != null)
                (questDetailPanel as IQuestOfferView)?.HidePanel();
```

- [ ] **Step 5: `Cutscene` via `ModalPrompt`**

In `Gameplay/Cutscenes/Cutscene.cs` replace:
```csharp
            if (!isPlaying || confirmingSkip || ConfirmationDialog.Instance == null ||
                (UIManager.Instance != null && UIManager.Instance.IsUIOpen))
                return;

            confirmingSkip = true;
            ConfirmationDialog.Instance.Show("Skip this cutscene?", Skip, () => confirmingSkip = false);
```
with:
```csharp
            var prompt = ModalPrompt.Current;
            if (!isPlaying || confirmingSkip || prompt == null || !prompt.CanShow)
                return;

            confirmingSkip = true;
            prompt.Show("Skip this cutscene?", Skip, () => confirmingSkip = false);
```
Replace `                ConfirmationDialog.Instance?.Cancel();` with `                ModalPrompt.Current?.Cancel();`. In the class-level doc comment that mentions `(ConfirmationDialog; the cutscene keeps playing meanwhile)`, leave the text as is. It still describes the UI accurately.

- [ ] **Step 6: Green + reference kept**

`refresh_unity` (force, compile request, wait); `read_console` `error CS`: 0; `run_tests` EditMode: all pass (21 = 19 + 2).
Then check every `NPC` still references its panel. Run this `execute_code`: it loads every scene under `Assets/Project_Ocean/Levels/Scenes` that contains an `NPC`, additively and without saving, plus every prefab with an `NPC`. For each `NPC` it reads `questDetailPanel` via `SerializedObject.FindProperty("questDetailPanel").objectReferenceValue` and reports `name → value type`. Close the additively opened scenes without saving. Expected: every NPC that had a `QuestDetailPanel` before still shows one (type `QuestDetailPanel`). Compare with the HEAD file content: `git show HEAD:<scene>` and grep the `questDetailPanel:` lines for non-zero `fileID`s. Record the list.

---

### Task 2: Add `DialoguePanelView` and migrate references (old fields still present)

**Files:**
- Create: `UI/Dialogue/DialoguePanelView.cs` (create the folder `UI/Dialogue` first)
- Modify: `Gameplay/Entities/NPCs/NPC.cs`, `Gameplay/QuestSystems/Quest/ExploreAreaTrigger.cs`, `Gameplay/QuestSystems/Quest/PlayerSelfDialogueUI.cs`: add ONLY the new field (old fields untouched)
- Scene/prefab changes via the migration script

**Interfaces:**
- Produces: `DialoguePanelView : MonoBehaviour, IDialogueView` with serialized `panel` (GameObject), `nameText` (TMP_Text), `bodyText` (TMP_Text), `avatar` (Image). Each of the three gameplay classes has `[SerializeField] private MonoBehaviour dialogueView;`.

- [ ] **Step 1: Create `DialoguePanelView`**

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// <summary>The shared dialogue panel's widgets; gameplay drives it through IDialogueView.</summary>
    public class DialoguePanelView : MonoBehaviour, IDialogueView
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Image avatar;

        public GameObject Panel => panel;
        public bool IsOpen => panel != null && panel.activeSelf;

        public void SetOpen(bool open)
        {
            if (panel != null) panel.SetActive(open);
        }

        public void SetSpeaker(string characterId, string fallbackName = null, Sprite fallbackPortrait = null) =>
            DialogueSpeaker.Apply(nameText, avatar, characterId, fallbackName, fallbackPortrait);

        public void SetText(string text)
        {
            if (bodyText != null) bodyText.text = text;
        }
    }
}
```

- [ ] **Step 2: Add the new field to the three gameplay classes**

In each of `NPC.cs`, `ExploreAreaTrigger.cs` and `PlayerSelfDialogueUI.cs`, directly above the existing `[SerializeField] private GameObject dialoguePanel;` line, add:
```csharp
        [Tooltip("DialoguePanelView on the dialogue panel (an IDialogueView).")]
        [SerializeField] private MonoBehaviour dialogueView;
```
Compile: `refresh_unity`; 0 `error CS`. A "field never used" warning is expected.

- [ ] **Step 3: Migration**

Unity MCP `execute_code` (editor, not in Play). Old field names per class:

| Class | panel | name text | body text | avatar |
|---|---|---|---|---|
| `NPC` | `dialoguePanel` | `npcNameText` | `dialogueText` | `npcAvatarImage` |
| `ExploreAreaTrigger` | `dialoguePanel` | `dialogueNameText` | `dialogueText` | `dialogueAvatarImage` |
| `PlayerSelfDialogueUI` | `dialoguePanel` | `npcNameText` | `dialogueText` | `npcAvatarImage` |

Algorithm:
1. **Prefabs first:** for every prefab under `Assets/Project_Ocean` containing any of the three types, use `PrefabUtility.LoadPrefabContents`. For each component whose `dialoguePanel` is non-null and inside this prefab: get or add a `DialoguePanelView` on that panel GameObject. If it was added, set its `panel`/`nameText`/`bodyText`/`avatar` from the component's old fields via `SerializedObject`. If it already existed, compare its fields with the old fields and record a CONFLICT if they differ (don't overwrite). Then set the component's `dialogueView` to the view. Save with `PrefabUtility.SaveAsPrefabAsset` and unload with `PrefabUtility.UnloadPrefabContents`.
2. **Then scenes:** for every `.unity` under `Assets/Project_Ocean/Levels/Scenes` that contains any of the three types (`EditorSceneManager.OpenScene(path, OpenSceneMode.Additive)`), do the same for each component. The panel is usually the scene's shared dialogue panel, so one view is added once and reused. Skip components whose `dialogueView` is already set to the correct view (inherited from the prefab step). If the panel GameObject is part of a prefab instance, adding the component creates an added-component override, which is acceptable. Mark the scene dirty, `EditorSceneManager.SaveScene(scene)`, then close it if it was opened additively.
3. Return a log: for each (asset path, object path, class) the panel name, whether the view was added or reused, and any CONFLICT.

Components with a null `dialoguePanel` are left with a null `dialogueView` (same "no panel" behaviour).

- [ ] **Step 4: Verify the wiring**

Run a second `execute_code` read-only pass over the same prefabs and scenes (additive open, no save). For every component, assert: `dialogueView != null` exactly when `dialoguePanel != null`; the view's `panel == dialoguePanel`; `nameText`, `bodyText` and `avatar` equal the old fields. Expected: 0 mismatches, 0 CONFLICTs. Then:
```bash
git status --short -- "*.unity" "*.prefab"
```
Expected: only the scenes and prefabs the log says were saved, all under their real paths (no stray `Assets/<name>.unity`). Stop and report BLOCKED on any mismatch or unexpected path.

---

### Task 3: Switch the code to the view and remove the old fields

**Files:** `NPC.cs`, `ExploreAreaTrigger.cs`, `PlayerSelfDialogueUI.cs`.

**Interfaces:**
- Consumes: `IDialogueView` members.

- [ ] **Step 1: Helper property in each class**

Add under the `dialogueView` field in each class:
```csharp
        // Unity null check first: a destroyed view must read as missing.
        private IDialogueView View => dialogueView != null ? dialogueView as IDialogueView : null;
```

- [ ] **Step 2: Replace the UI calls (exact mapping)**

`NPC.cs`:
- `if (dialoguePanel != null) dialoguePanel.SetActive(false);` (in `Start`) → `View?.SetOpen(false);`
- `bool wasOpen = dialoguePanel != null && dialoguePanel.activeSelf;` → `bool wasOpen = View != null && View.IsOpen;`
- In `OpenDialogue`: `if (dialoguePanel == null) return;` → `if (View == null || View.Panel == null) return;`. The two lines `if (dialogueText != null) dialogueText.text = npcDialogue;` → `View.SetText(npcDialogue);`. The line `DialogueSpeaker.Apply(npcNameText, npcAvatarImage, npcId, npcName, npcPortrait);` → `View.SetSpeaker(npcId, npcName, npcPortrait);`
- `SetDialoguePanel` body → 
```csharp
            var view = View;
            if (view == null || view.Panel == null) return;
            view.SetOpen(show);
            if (show) DialogueState.Show(view.Panel);
            else DialogueState.Hide(view.Panel);
```
- In `StartDialogue`: `DialogueSpeaker.Apply(npcNameText, npcAvatarImage, npcId, npcName, npcPortrait);` → `View?.SetSpeaker(npcId, npcName, npcPortrait);`
- In `ShowDialogueText`: `if (dialogueText == null) return;` → `if (View == null) return;`. Every `dialogueText.text = X;` in that method → `View.SetText(X);`
- In `AdvanceDialogue`: `dialogueText.text = offerLines[currentDialogueIndex];` → `View?.SetText(offerLines[currentDialogueIndex]);`

`ExploreAreaTrigger.cs`:
- In `OpenDialogue`: `dialoguePanel.SetActive(true);` and `DialogueState.Show(dialoguePanel);` → `View?.SetOpen(true);` and `DialogueState.Show(View?.Panel);`
- In `ShowCurrentLine`: `DialogueSpeaker.Apply(dialogueNameText, dialogueAvatarImage, line.speakerId);` → `View?.SetSpeaker(line.speakerId);`. `if (dialogueText != null) dialogueText.text = line.text;` → `View?.SetText(line.text);`
- In `CloseDialogue`: the three lines after `isShowingDialogue = false;` →
```csharp
            var view = View;
            if (view == null || view.Panel == null) return;
            view.SetOpen(false);
            DialogueState.Hide(view.Panel);
```
- Any other use of `dialoguePanel` in the file (e.g. an `Awake` null check or warning): replace `dialoguePanel` with `View?.Panel` and keep the logic the same. List each such line in the report.

`PlayerSelfDialogueUI.cs`:
- `if (dialoguePanel != null) dialoguePanel.SetActive(true);` / `DialogueState.Show(dialoguePanel);` → `View?.SetOpen(true);` / `DialogueState.Show(View?.Panel);`
- `DialogueSpeaker.Apply(npcNameText, npcAvatarImage, <id expr>, displayName);` → `View?.SetSpeaker(<id expr>, displayName);` (keep the `<id expr>` exactly: `string.IsNullOrEmpty(line.speakerId) ? PlayerId : line.speakerId`)
- `if (dialogueText != null) dialogueText.text = line.text;` → `View?.SetText(line.text);`
- `if (dialoguePanel != null) dialoguePanel.SetActive(false);` / `DialogueState.Hide(dialoguePanel);` → `View?.SetOpen(false);` / `DialogueState.Hide(View?.Panel);`

- [ ] **Step 3: Remove the old fields and now-unused usings**

Delete in `NPC.cs` the fields `dialoguePanel`, `npcNameText`, `dialogueText`, `npcAvatarImage`. Keep the `[Header("Dialogue UI")]` line above `dialogueView`: move the header to sit directly above the `dialogueView` tooltip. In `ExploreAreaTrigger.cs` delete `dialoguePanel`, `dialogueNameText`, `dialogueText`, `dialogueAvatarImage`. In `PlayerSelfDialogueUI.cs` delete `dialoguePanel`, `npcNameText`, `dialogueText`, `npcAvatarImage`. Remove `using TMPro;` / `using UnityEngine.UI;` from a file only if nothing else in it still uses them (compile tells you).

- [ ] **Step 4: Green + no UI references left**

`refresh_unity` (force, compile request, wait); `read_console` `error CS`: 0; `run_tests` EditMode: 21/21.
```bash
grep -nE "DialogueSpeaker|TextMeshProUGUI|QuestDetailPanel|ConfirmationDialog|UIManager" Assets/Project_Ocean/Scripts/Gameplay/Entities/NPCs/NPC.cs Assets/Project_Ocean/Scripts/Gameplay/QuestSystems/Quest/ExploreAreaTrigger.cs Assets/Project_Ocean/Scripts/Gameplay/QuestSystems/Quest/PlayerSelfDialogueUI.cs Assets/Project_Ocean/Scripts/Gameplay/Cutscenes/Cutscene.cs
python tools/uml/gen_uml.py && grep -oE "Layer violations — [0-9]+" docs/uml/metrics.md
```
Expected: the grep shows only comment/tooltip text; the violation count is 0 if 3e is already done, otherwise only inventory rows remain.

---

### Task 4: Verify live, docs

- [ ] **Step 1: Back up saves** (as in earlier phases, to `$TEMP/p3f2_savebackup`).

- [ ] **Step 2: Play checks**

Play from Bootstrap (`Application.runInBackground = true`; memories `feedback_unity_play_mode_unfocused.md`, `reference_live_enemy_test_route.md`). With `execute_code`:
1. **NPC plain dialogue:** interact with an NPC that has no quest. The panel opens, the name and portrait match `NPCManager`'s data for its `npcId` (or its own fallback), and the text equals its `npcDialogue`. Interact again: the panel closes and `NpcEvents` fires.
2. **NPC quest dialogue:** talk to the NPC with the active tutorial or main quest. The offer lines advance, and after the last line the quest detail panel opens (`QuestDetailPanel` active). Walking away (`HandlePlayerExited`) closes it.
3. **ExploreAreaTrigger:** trigger one whose step is active (or force the step via QuestManager dev tools). The panel shows the speaker and lines, advances, and closes.
4. **PlayerSelfDialogueUI:** trigger an Auto Talk/Cutscene step that uses it. The panel shows "You" (or the authored speaker) and the lines, then closes.
5. **Cutscene skip:** during a cutscene, raise the skip input. The prompt "Skip this cutscene?" appears. Cancel keeps playing, confirm skips. With the pause menu open, no prompt appears.
6. `DialogueState`: while any of these panels is open, other land UI (shop or board) hides as before. Check `DialogueState`'s open flag.
Mark any item that can't be triggered as UNVERIFIED with the reason. Exit Play.

- [ ] **Step 3: Restore, dirty check, docs**

Restore the save folder byte-identical (as in earlier phases) and print RESTORED. Run `git status --short -- "*.unity" "*.prefab"`: only the migration's saved files may appear (list them). Run `python tools/uml/gen_uml.py`, `python tools/uml/test_gen_uml.py`, `graphify update .`.
Append to the `**Progress:**` line under `### Phase 3` in `docs/uml/findings.md`: ` 3f-2 done: NPC/ExploreAreaTrigger/PlayerSelfDialogueUI use IDialogueView (DialoguePanelView), NPC → IQuestOfferView, Cutscene → IModalPrompt (<new count> links).` Add ` — done in 3f-1/3f-2` at the end of the Phase 3 "Gameplay → UI helpers" bullet.

- [ ] **Step 4: Hand-off**

List the changed scenes and prefabs explicitly for the user. Suggested commits: "refactor(ui): gameplay drives dialogue/quest-offer/skip prompt through view interfaces" (code), "chore(scenes): wire DialoguePanelView" (scenes/prefabs). Do not commit.
