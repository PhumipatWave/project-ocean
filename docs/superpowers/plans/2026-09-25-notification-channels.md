# Notification Channels Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Split the single top notification into four screen areas (General top, Center, System bottom-right, Resource bottom-left stack) with a small OOP structure.

**Architecture:** `NotificationManager` stays the one static bus but now carries a `NotificationMessage` (text, icon, channel). An abstract `NotificationView` filters by channel; `ToastNotificationView` (one at a time) and `StackNotificationView` (stacked lines) render. `ResourceNotificationPresenter` turns money/item delta events from `PlayerMoney` and `InventoryManager` into Resource messages, so gameplay code never touches UI.

**Tech Stack:** Unity 6000.3.6f1, C#, uGUI + TextMeshPro, Unity Test Framework 1.6.0 (EditMode), Unity MCP.

**Spec:** `docs/superpowers/specs/2026-09-25-notification-channels-design.md`

## Global Constraints

- Namespace for all runtime code: `Game.Testing` (assembly `Game.Testing`, `Assets/Project_Ocean/Scripts/Game.Testing.asmdef`).
- Comments: 1 line, 2 max; WHY only; no history; XML `<summary>` only on public API, 1 sentence (CLAUDE.md "Comment Rules").
- All fades use unscaled time (`Time.unscaledDeltaTime`, `WaitForSecondsRealtime`).
- `showDuration = 2`, `fadeDuration = 0.4`, `maxLines = 4`.
- Every static-event subscriber unsubscribes in `OnDisable`.
- Save load and new-game reset must never show a notification.
- No new managers. Only `AS_PlayerHUD.unity` is edited among scenes.
- Unity MCP: instance `project-ocean@7db8e111` (pin with `set_active_instance` first). After any scene save, run `git status` and confirm the path is `Assets/Project_Ocean/Levels/Scenes/Menus/AS_PlayerHUD.unity`.
- Commits: the user decides. Commit steps below list exact files; skip them unless the user said to commit.

## Spec corrections found while planning

1. Quest money rewards go through `QuestManager.OnMoneyRewarded` → `PlayerMoney.AddMoneySilent`, not `AddMoney`. Fix: `PlayerMoney` subscribes `OnMoneyRewarded` to `AddMoney`, so every quest reward shows `+X Money` once (the three `OnMoneyRewarded` sites previously showed a toast at only one of them).
2. `SaveBridge.ApplyPlayerStats` resets money with `Use(CurrentMoney)` + `AddMoneySilent(data.money)`. With a delta event, `Use` would toast `-X Money` on every load. Fix: replace both with a new silent `SetMoneySilent(int)` and delete `AddMoneySilent` (no other callers).
3. `DiscardDraggedItem` also removes items; it raises an item delta too.

## File map

| File | Responsibility |
|------|----------------|
| `Scripts/UI/Notifications/NotificationChannel.cs` (new) | enum of screen areas |
| `Scripts/UI/Notifications/NotificationMessage.cs` (new) | immutable message data |
| `Scripts/UI/NotificationManager.cs` (modify) | static bus, `Notify(text, channel, icon)` |
| `Scripts/UI/Notifications/NotificationView.cs` (new) | abstract base: subscribe + channel filter |
| `Scripts/UI/Notifications/ToastNotificationView.cs` (moved from `Scripts/UI/NotificationTextUI.cs`, same GUID) | one-at-a-time toast |
| `Scripts/UI/Notifications/NotificationLine.cs` (new) | one stacked line: set up, fade, destroy |
| `Scripts/UI/Notifications/StackNotificationView.cs` (new) | stacked lines, max 4 |
| `Scripts/UI/Notifications/ResourceNotificationPresenter.cs` (new) | money/item deltas → Resource messages |
| `Scripts/Gameplay/Entitys/Players/PlayerMoney.cs` (modify) | `OnMoneyDelta`, `SetMoneySilent` |
| `Scripts/UI/Inventory/InventoryManager.cs` (modify) | `OnItemDelta`, `RaiseItemDelta` |
| `Scripts/UI/Shop/ShopTransactionService.cs` (modify) | raise item deltas on sell |
| `Scripts/Core/Save Game/SaveBridge.cs` (modify) | silent money restore |
| `Scripts/Gameplay/QuestSystems/Quest/QuestManager.cs` (modify) | step toast → Center, drop duplicate money toast |
| `Scripts/Gameplay/QuestSystems/Quest/TutorialPromptUI.cs` (modify) | prompt as Center toast |
| `Scripts/Gameplay/QuestSystems/Quest/TutorialEvents.cs` (modify) | blocked key re-shows prompt |
| `Scripts/UI/SaveNotificationUI.cs` (modify) | "Game Saved!" → System |
| `Tests/EditMode/Game.Tests.EditMode.asmdef`, `Tests/EditMode/NotificationTests.cs` (new) | EditMode checks |
| `Prefabs/UI/Notifications/NotificationLine.prefab`, `NotificationLayer.prefab` (new) | UI prefabs |
| `Levels/Scenes/Menus/AS_PlayerHUD.unity` (modify) | place layer, move SkipText |

All paths are relative to `Assets/Project_Ocean/`.

---

### Task 1: Channel bus and toast view

Moves every existing notification onto the new bus. After this task the game behaves as before, except tutorial prompts fade instead of staying on screen. Save toast and quest toast still render in the old top slot until Task 4 adds the other views (the old object's view defaults to `General`, so System/Center messages are not shown between Task 1 and Task 4; do Task 4 in the same session).

**Files:**
- Create: `Scripts/UI/Notifications/NotificationChannel.cs`, `Scripts/UI/Notifications/NotificationMessage.cs`, `Scripts/UI/Notifications/NotificationView.cs`
- Move + rewrite: `Scripts/UI/NotificationTextUI.cs` (+ `.meta`) → `Scripts/UI/Notifications/ToastNotificationView.cs` (+ `.meta`)
- Modify: `Scripts/UI/NotificationManager.cs`, `Scripts/UI/SaveNotificationUI.cs:17`, `Scripts/Gameplay/QuestSystems/Quest/QuestManager.cs:494-498`, `Scripts/Gameplay/QuestSystems/Quest/TutorialPromptUI.cs:6-23,102-116`, `Scripts/Gameplay/QuestSystems/Quest/TutorialEvents.cs:93-98`
- Test: `Tests/EditMode/Game.Tests.EditMode.asmdef`, `Tests/EditMode/NotificationTests.cs`

**Interfaces:**
- Produces:
  - `enum NotificationChannel { General, Center, System, Resource }` (order matters: scene serializes the int)
  - `class NotificationMessage { string Text; Sprite Icon; NotificationChannel Channel; NotificationMessage(string text, NotificationChannel channel, Sprite icon) }`
  - `static event Action<NotificationMessage> NotificationManager.OnNotify`
  - `static void NotificationManager.Notify(string message, NotificationChannel channel = NotificationChannel.General, Sprite icon = null)`
  - `abstract class NotificationView : MonoBehaviour` with `protected virtual void OnEnable()`, `protected virtual void OnDisable()`, `protected abstract void Show(NotificationMessage msg)`
  - `class ToastNotificationView : NotificationView` with serialized `channel`, `canvasGroup`, `messageText`, `icon`, `showDuration`, `fadeDuration`
  - `static string TutorialPromptUI.CurrentPrompt { get; }`

- [ ] **Step 1: Create the test assembly**

`Assets/Project_Ocean/Tests/EditMode/Game.Tests.EditMode.asmdef`:

```json
{
    "name": "Game.Tests.EditMode",
    "rootNamespace": "Game.Tests",
    "references": [
        "Game.Testing",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 2: Write the failing tests**

`Assets/Project_Ocean/Tests/EditMode/NotificationTests.cs`:

```csharp
using Game.Testing;
using NUnit.Framework;
using UnityEngine;

namespace Game.Tests
{
    public class NotificationTests
    {
        private NotificationMessage received;

        private void Capture(NotificationMessage msg) => received = msg;

        [SetUp]
        public void SetUp()
        {
            received = null;
            NotificationManager.OnNotify += Capture;
        }

        [TearDown]
        public void TearDown() => NotificationManager.OnNotify -= Capture;

        [Test]
        public void Notify_WithoutChannel_UsesGeneral()
        {
            NotificationManager.Notify("hello");

            Assert.AreEqual("hello", received.Text);
            Assert.AreEqual(NotificationChannel.General, received.Channel);
            Assert.IsNull(received.Icon);
        }

        [Test]
        public void Notify_KeepsChannelAndIcon()
        {
            var icon = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), Vector2.zero);

            NotificationManager.Notify("+1 Tuna", NotificationChannel.Resource, icon);

            Assert.AreEqual(NotificationChannel.Resource, received.Channel);
            Assert.AreSame(icon, received.Icon);
            Object.DestroyImmediate(icon);
        }

        [Test]
        public void Notify_EmptyText_SendsNothing()
        {
            NotificationManager.Notify("");

            Assert.IsNull(received);
        }
    }
}
```

- [ ] **Step 3: Run tests to verify they fail**

Unity MCP: `refresh_unity(compile="request", scope="scripts")`, then `read_console(types=["error"])`.
Expected: compile errors `The type or namespace name 'NotificationMessage' could not be found` and `'NotificationChannel' does not exist`.

- [ ] **Step 4: Create the data types**

`Assets/Project_Ocean/Scripts/UI/Notifications/NotificationChannel.cs`:

```csharp
namespace Game.Testing
{
    /// <summary>Screen area a notification shows in.</summary>
    // Serialized as int on views in AS_PlayerHUD — only append new values.
    public enum NotificationChannel
    {
        General,  // top center
        Center,   // quest + tutorial
        System,   // bottom right
        Resource  // bottom left, money + items
    }
}
```

`Assets/Project_Ocean/Scripts/UI/Notifications/NotificationMessage.cs`:

```csharp
using UnityEngine;

namespace Game.Testing
{
    /// <summary>One notification: what to show, where, and an optional icon.</summary>
    public class NotificationMessage
    {
        public string Text { get; }
        public Sprite Icon { get; }
        public NotificationChannel Channel { get; }

        public NotificationMessage(string text, NotificationChannel channel, Sprite icon)
        {
            Text = text;
            Channel = channel;
            Icon = icon;
        }
    }
}
```

- [ ] **Step 5: Replace `NotificationManager.cs`**

Full content of `Assets/Project_Ocean/Scripts/UI/NotificationManager.cs`:

```csharp
using System;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>Static notification bus — callers pick a channel, views listen for theirs.</summary>
    public static class NotificationManager
    {
        public static event Action<NotificationMessage> OnNotify;

        public static void Notify(string message, NotificationChannel channel = NotificationChannel.General, Sprite icon = null)
        {
            if (string.IsNullOrEmpty(message))
                return;

            OnNotify?.Invoke(new NotificationMessage(message, channel, icon));
        }
    }
}
```

- [ ] **Step 6: Create the view base class**

`Assets/Project_Ocean/Scripts/UI/Notifications/NotificationView.cs`:

```csharp
using UnityEngine;

namespace Game.Testing
{
    /// <summary>Base for anything that displays notifications of one channel.</summary>
    public abstract class NotificationView : MonoBehaviour
    {
        [SerializeField] private NotificationChannel channel;

        protected virtual void OnEnable() => NotificationManager.OnNotify += HandleNotify;
        protected virtual void OnDisable() => NotificationManager.OnNotify -= HandleNotify;

        private void HandleNotify(NotificationMessage msg)
        {
            if (msg.Channel == channel)
                Show(msg);
        }

        protected abstract void Show(NotificationMessage msg);
    }
}
```

- [ ] **Step 7: Move `NotificationTextUI` to `ToastNotificationView`, keeping its GUID**

Moving the `.meta` with the file keeps the GUID, so the component on `GameHUD/Notification` in `AS_PlayerHUD` stays linked. The serialized field names `canvasGroup`, `messageText`, `showDuration`, `fadeDuration` are kept on purpose so their Inspector values survive.

```bash
cd "D:/UnityGithub/project-ocean/project-ocean/Assets/Project_Ocean/Scripts/UI"
mkdir -p Notifications
git mv NotificationTextUI.cs Notifications/ToastNotificationView.cs
git mv NotificationTextUI.cs.meta Notifications/ToastNotificationView.cs.meta
```

Then replace the full content of `Scripts/UI/Notifications/ToastNotificationView.cs`:

```csharp
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// <summary>Shows one notification at a time: fade in, hold, fade out.</summary>
    public class ToastNotificationView : NotificationView
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Image icon;
        [SerializeField] private float showDuration = 2f;
        [SerializeField] private float fadeDuration = 0.4f;

        private Coroutine routine;
        private string currentText;
        private NotificationMessage pending;

        private void Awake()
        {
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            // The coroutine dies with the object; without this reset the view would stay "busy" forever.
            routine = null;
            currentText = null;
            pending = null;
            if (canvasGroup != null) canvasGroup.alpha = 0f;
        }

        protected override void Show(NotificationMessage msg)
        {
            if (routine != null)
            {
                // Same text again = spam, drop it. A different one waits (latest wins).
                if (msg.Text != currentText) pending = msg;
                return;
            }

            routine = StartCoroutine(ToastRoutine(msg));
        }

        private IEnumerator ToastRoutine(NotificationMessage msg)
        {
            currentText = msg.Text;
            if (messageText != null) messageText.text = msg.Text;
            if (icon != null)
            {
                icon.sprite = msg.Icon;
                icon.gameObject.SetActive(msg.Icon != null);
            }

            yield return Fade(0f, 1f);
            yield return new WaitForSecondsRealtime(showDuration);
            yield return Fade(1f, 0f);

            routine = null;
            currentText = null;

            if (pending != null)
            {
                var next = pending;
                pending = null;
                Show(next);
            }
        }

        private IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }
}
```

- [ ] **Step 8: Route the save toast to System**

In `Scripts/UI/SaveNotificationUI.cs`, replace the class doc and handler. Full content:

```csharp
using UnityEngine;

namespace Game.Testing
{
    /// <summary>Shows "Game Saved!" on the System channel after every save.</summary>
    public class SaveNotificationUI : MonoBehaviour
    {
        [SerializeField] private string message = "Game Saved!";

        private void OnEnable()  => SaveBridge.OnSaved += HandleSaved;
        private void OnDisable() => SaveBridge.OnSaved -= HandleSaved;

        private void HandleSaved() => NotificationManager.Notify(message, NotificationChannel.System);
    }
}
```

- [ ] **Step 9: Route the quest step toast to Center**

In `Scripts/Gameplay/QuestSystems/Quest/QuestManager.cs` (inside `NotifyStepDescriptionDelayed`), replace:

```csharp
            // Re-read now, not at schedule time — the quest may have advanced again since.
            // Skip guided steps — TutorialPromptUI already shows this same description on
            // the dedicated persistent panel; toasting it too would show it twice at once.
            if (!string.IsNullOrEmpty(q.StepDescription) && !q.IsGuidedSequence)
                NotificationManager.Notify(q.StepDescription);
```

with:

```csharp
            // Re-read now: the quest may have advanced during the delay.
            // Guided steps are shown by TutorialPromptUI; skip them to avoid a duplicate.
            if (!string.IsNullOrEmpty(q.StepDescription) && !q.IsGuidedSequence)
                NotificationManager.Notify(q.StepDescription, NotificationChannel.Center);
```

- [ ] **Step 10: Make the tutorial prompt a Center toast**

In `Scripts/Gameplay/QuestSystems/Quest/TutorialPromptUI.cs`:

Replace the last paragraph of the class doc:

```csharp
    ///
    /// Rendering is delegated to GameHUD's shared NotificationTextUI (via
    /// NotificationManager.ShowPersistent/ClearPersistent) rather than a dedicated panel —
    /// this is decision-logic only, no UI of its own.
    /// </summary>
```

with:

```csharp
    /// Shows the prompt as a Center-channel toast; decision logic only, no UI of its own.
    /// </summary>
```

Add a property as the first member inside the class body (directly after `public class TutorialPromptUI : MonoBehaviour` and its `{`):

```csharp
        /// <summary>Prompt text currently in effect, or null — re-shown when the player presses a blocked key.</summary>
        public static string CurrentPrompt { get; private set; }

```

Replace `Show` and `Hide`:

```csharp
        private void Show(string text, Sprite image, TutorialActionId requiredAction)
        {
            NotificationManager.ShowPersistent(text, image);
            TutorialGate.RequiredAction = requiredAction;
        }

        private void Hide()
        {
            if (TutorialGate.RequiredAction.HasValue)
                Debug.Log($"[TutorialPromptUI] Hide — was requiring {TutorialGate.RequiredAction.Value} " +
                    $"(locked={PlayerUIState.IsLocked}, questManagerNull={QuestManager.Instance == null})");

            NotificationManager.ClearPersistent();
            TutorialGate.RequiredAction = null;
        }
```

with:

```csharp
        private void Show(string text, Sprite image, TutorialActionId requiredAction)
        {
            // Refresh runs on many events; only toast when the prompt actually changes.
            if (text != CurrentPrompt)
            {
                CurrentPrompt = text;
                NotificationManager.Notify(text, NotificationChannel.Center, image);
            }
            TutorialGate.RequiredAction = requiredAction;
        }

        private void Hide()
        {
            if (TutorialGate.RequiredAction.HasValue)
                Debug.Log($"[TutorialPromptUI] Hide — was requiring {TutorialGate.RequiredAction.Value} " +
                    $"(locked={PlayerUIState.IsLocked}, questManagerNull={QuestManager.Instance == null})");

            CurrentPrompt = null;
            TutorialGate.RequiredAction = null;
        }
```

- [ ] **Step 11: Re-show the prompt on a blocked key**

In `Scripts/Gameplay/QuestSystems/Quest/TutorialEvents.cs`, replace:

```csharp
        /// <summary>Call when Blocks() returns true for a direct key press (Sleep/Dive/OpenInventory)
        /// so the player gets a reason instead of the key silently doing nothing. Pulses
        /// TutorialPromptUI's persistent banner rather than a Notify() toast — a toast is
        /// silently dropped while a persistent message owns the notification slot, which is
        /// always the case whenever Blocks() can return true.</summary>
        public static void NotifyBlocked() => NotificationManager.PulsePersistent();
```

with:

```csharp
        /// <summary>Call when Blocks() rejects a key press, so the player sees the current prompt again.</summary>
        public static void NotifyBlocked() =>
            NotificationManager.Notify(TutorialPromptUI.CurrentPrompt, NotificationChannel.Center);
```

- [ ] **Step 12: Compile and run tests**

Unity MCP: `refresh_unity(compile="request", scope="scripts")`, check `mcpforunity://editor/state` until `is_compiling` is false, then `read_console(types=["error"])`.
Expected: no errors except the pre-existing `Missing types referenced from component UniversalRenderPipelineGlobalSettings`.

Then `run_tests(mode="EditMode", assembly_names=["Game.Tests.EditMode"], include_failed_tests=true)` and poll `get_test_job`.
Expected: 3 passed, 0 failed.

- [ ] **Step 13: Confirm the scene link survived the move**

Unity MCP `execute_code`:

```csharp
var guid = UnityEditor.AssetDatabase.AssetPathToGUID("Assets/Project_Ocean/Scripts/UI/Notifications/ToastNotificationView.cs");
var scene = System.IO.File.ReadAllText("Assets/Project_Ocean/Levels/Scenes/Menus/AS_PlayerHUD.unity");
return scene.Contains(guid) ? "linked " + guid : "NOT LINKED " + guid;
```

Expected: `linked <guid>`.

- [ ] **Step 14: Commit (only if the user approved commits)**

```bash
git add Assets/Project_Ocean/Tests Assets/Project_Ocean/Scripts/UI/Notifications Assets/Project_Ocean/Scripts/UI/NotificationManager.cs Assets/Project_Ocean/Scripts/UI/SaveNotificationUI.cs Assets/Project_Ocean/Scripts/Gameplay/QuestSystems/Quest/QuestManager.cs Assets/Project_Ocean/Scripts/Gameplay/QuestSystems/Quest/TutorialPromptUI.cs Assets/Project_Ocean/Scripts/Gameplay/QuestSystems/Quest/TutorialEvents.cs
git commit -m "feat: add notification channels and toast view"
```

---

### Task 2: Money and item delta events + presenter

**Files:**
- Create: `Scripts/UI/Notifications/ResourceNotificationPresenter.cs`
- Modify: `Scripts/Gameplay/Entitys/Players/PlayerMoney.cs` (whole file), `Scripts/Core/Save Game/SaveBridge.cs:153-158`, `Scripts/Gameplay/QuestSystems/Quest/QuestManager.cs:434-438`, `Scripts/UI/Inventory/InventoryManager.cs` (`AddItem`, `ConsumeItem`, `RemoveQuestItem`, `RemoveSelectedItem`, `DiscardDraggedItem`, new members), `Scripts/UI/Shop/ShopTransactionService.cs` (`SellSingle`, `SellAll`)
- Test: `Tests/EditMode/NotificationTests.cs`

**Interfaces:**
- Consumes: `NotificationManager.Notify(string, NotificationChannel, Sprite)`, `NotificationChannel.Resource` (Task 1)
- Produces:
  - `static event Action<int> PlayerMoney.OnMoneyDelta` (+gain, −spend)
  - `void PlayerMoney.SetMoneySilent(int amount)`
  - `static event Action<Item, int> InventoryManager.OnItemDelta`
  - `void InventoryManager.RaiseItemDelta(Item item, int delta)`
  - `static string ResourceNotificationPresenter.Format(int delta, string name)`
  - `class ResourceNotificationPresenter : MonoBehaviour` with serialized `Sprite moneyIcon`

- [ ] **Step 1: Write the failing test**

Add inside `NotificationTests` in `Tests/EditMode/NotificationTests.cs`, after `Notify_EmptyText_SendsNothing`:

```csharp
        [Test]
        public void Format_AddsSignAndName()
        {
            Assert.AreEqual("+1 Tuna", ResourceNotificationPresenter.Format(1, "Tuna"));
            Assert.AreEqual("-20 Money", ResourceNotificationPresenter.Format(-20, "Money"));
        }
```

- [ ] **Step 2: Run to verify it fails**

`refresh_unity(compile="request", scope="scripts")`, `read_console(types=["error"])`.
Expected: `The name 'ResourceNotificationPresenter' does not exist in the current context`.

- [ ] **Step 3: Create the presenter**

`Assets/Project_Ocean/Scripts/UI/Notifications/ResourceNotificationPresenter.cs`:

```csharp
using UnityEngine;

namespace Game.Testing
{
    /// <summary>Turns money and item changes into Resource-channel notifications.</summary>
    public class ResourceNotificationPresenter : MonoBehaviour
    {
        [SerializeField] private Sprite moneyIcon;

        private void OnEnable()
        {
            PlayerMoney.OnMoneyDelta += HandleMoneyDelta;
            InventoryManager.OnItemDelta += HandleItemDelta;
        }

        private void OnDisable()
        {
            PlayerMoney.OnMoneyDelta -= HandleMoneyDelta;
            InventoryManager.OnItemDelta -= HandleItemDelta;
        }

        /// <summary>"+1 Tuna" / "-20 Money".</summary>
        public static string Format(int delta, string name) => $"{(delta > 0 ? "+" : "")}{delta} {name}";

        private void HandleMoneyDelta(int delta)
        {
            if (delta == 0) return;
            NotificationManager.Notify(Format(delta, "Money"), NotificationChannel.Resource, moneyIcon);
        }

        private void HandleItemDelta(Item item, int delta)
        {
            if (item == null || delta == 0) return;
            NotificationManager.Notify(Format(delta, item.ItemName), NotificationChannel.Resource, item.ItemVisual);
        }
    }
}
```

- [ ] **Step 4: Replace `PlayerMoney.cs`**

Full content of `Assets/Project_Ocean/Scripts/Gameplay/Entitys/Players/PlayerMoney.cs`:

```csharp
using UnityEngine;

namespace Game.Testing
{
    public class PlayerMoney : MonoBehaviour
    {
        [SerializeField] private int money = 100;

        /// <summary>New balance after any change — QuestManager re-checks RequiredMoney steps on it.</summary>
        public static event System.Action<int> OnMoneyChanged;

        /// <summary>Signed amount of a player-visible change (gain or spend); never raised by save load.</summary>
        public static event System.Action<int> OnMoneyDelta;

        public int Money => money;
        public int CurrentMoney => money;

        private void OnEnable() => QuestManager.OnMoneyRewarded += AddMoney;
        private void OnDisable() => QuestManager.OnMoneyRewarded -= AddMoney;

        public void AddMoney(int amount)
        {
            money += amount;
            if (amount != 0) OnMoneyDelta?.Invoke(amount);
            OnMoneyChanged?.Invoke(money);
        }

        /// <summary>Sets the balance with no notification — save load only.</summary>
        public void SetMoneySilent(int amount)
        {
            money = amount;
            OnMoneyChanged?.Invoke(money);
        }

        public bool Use(int amount)
        {
            if (money < amount)
                return false;

            money -= amount;
            if (amount != 0) OnMoneyDelta?.Invoke(-amount);
            OnMoneyChanged?.Invoke(money);
            return true;
        }
    }
}
```

- [ ] **Step 5: Silent money restore on load**

In `Scripts/Core/Save Game/SaveBridge.cs`, replace:

```csharp
                playerMoney.Use(playerMoney.CurrentMoney);
                playerMoney.AddMoneySilent(data.money);
```

with:

```csharp
                playerMoney.SetMoneySilent(data.money);
```

- [ ] **Step 6: Drop the duplicate quest money toast**

In `Scripts/Gameplay/QuestSystems/Quest/QuestManager.cs`, replace:

```csharp
            else if (completedStep.IsComplete && completedStep.RewardMoney > 0)
            {
                OnMoneyRewarded?.Invoke(completedStep.RewardMoney);
                NotificationManager.Notify($"+{completedStep.RewardMoney} Money");
            }
```

with:

```csharp
            else if (completedStep.IsComplete && completedStep.RewardMoney > 0)
            {
                OnMoneyRewarded?.Invoke(completedStep.RewardMoney);
            }
```

- [ ] **Step 7: Add the item delta event to `InventoryManager`**

In `Scripts/UI/Inventory/InventoryManager.cs`, add directly above `public bool AddItem(Item item)`:

```csharp
        /// <summary>Signed count of a player-visible item change; save load and reset never raise it.</summary>
        public static event System.Action<Item, int> OnItemDelta;

        /// <summary>Public so ShopTransactionService, which edits slots directly, can report sales.</summary>
        public void RaiseItemDelta(Item item, int delta)
        {
            if (item != null && delta != 0)
                OnItemDelta?.Invoke(item, delta);
        }

```

In `AddItem`, replace:

```csharp
            if (itemAdded)
                NotifyChanged();
            else
                NotificationManager.Notify("Inventory is full");
```

with:

```csharp
            if (itemAdded)
            {
                NotifyChanged();
                RaiseItemDelta(item, 1);
            }
            else
                NotificationManager.Notify("Inventory is full");
```

In `ConsumeItem`, replace:

```csharp
            foundItem = itemInSlot.GetItem();
            RemoveOneItem(itemInSlot);
            NotifyChanged();
            return true;
```

with:

```csharp
            foundItem = itemInSlot.GetItem();
            RemoveOneItem(itemInSlot);
            NotifyChanged();
            RaiseItemDelta(foundItem, -1);
            return true;
```

Replace `RemoveQuestItem`:

```csharp
        public void RemoveQuestItem(int itemId, int count = 1)
        {
            if (InventoryQuestService.RemoveQuestItem(inventorySlots, itemId, count))
                NotifyChanged();
        }
```

with:

```csharp
        public void RemoveQuestItem(int itemId, int count = 1)
        {
            if (InventoryQuestService.RemoveQuestItem(inventorySlots, itemId, count))
            {
                NotifyChanged();
                RaiseItemDelta(FindItemInDatabase(itemId), -count);
            }
        }
```

In `RemoveSelectedItem`, replace the confirm callback body:

```csharp
                    Draggableltem toRemove = GetItemInSlot(slotToRemove);
                    if (toRemove == null) return;
                    Debug.Log($"[Inventory] Remove item {toRemove.GetItem().ItemName} from slot {slotToRemove}");
                    Destroy(toRemove.gameObject);
                    NotifyChanged();
```

with:

```csharp
                    Draggableltem toRemove = GetItemInSlot(slotToRemove);
                    if (toRemove == null) return;
                    Debug.Log($"[Inventory] Remove item {toRemove.GetItem().ItemName} from slot {slotToRemove}");
                    Destroy(toRemove.gameObject);
                    NotifyChanged();
                    RaiseItemDelta(toRemove.GetItem(), -toRemove.count);
```

In `DiscardDraggedItem`, replace:

```csharp
                    Destroy(itemToDiscard.gameObject);
                    NotifyChanged();
```

with:

```csharp
                    Destroy(itemToDiscard.gameObject);
                    NotifyChanged();
                    RaiseItemDelta(item, -itemToDiscard.count);
```

(`AddRewardItem` goes through `AddItem`; `ResetToDefault` and `SaveBridge.ApplyInventory` write slots directly — both stay silent with no change.)

- [ ] **Step 8: Report shop sales**

In `Scripts/UI/Shop/ShopTransactionService.cs`, `SellSingle`, replace:

```csharp
            playerMoney.AddMoney(GetSellPrice(realItem.GetItem()));
            RemoveOne(realItem);
            inventory.NotifyChanged();
            return true;
```

with:

```csharp
            Item sold = realItem.GetItem();
            playerMoney.AddMoney(GetSellPrice(sold));
            RemoveOne(realItem);
            inventory.NotifyChanged();
            inventory.RaiseItemDelta(sold, -1);
            return true;
```

In `SellAll`, replace:

```csharp
            int total = 0;
            bool soldAny = false;
            foreach (InventorySlot slot in inventory.InventorySlots)
            {
                if (slot == null) continue;

                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI == null || !IsSellAllItem(itemUI.GetItem())) continue;

                total += GetSellPrice(itemUI.GetItem()) * itemUI.count;
                Object.Destroy(itemUI.gameObject);
                soldAny = true;
            }

            if (!soldAny) return false;

            playerMoney.AddMoney(total);
            inventory.NotifyChanged();
```

with:

```csharp
            int total = 0;
            // One notification per item type, not per slot.
            var soldCounts = new System.Collections.Generic.Dictionary<Item, int>();
            foreach (InventorySlot slot in inventory.InventorySlots)
            {
                if (slot == null) continue;

                Draggableltem itemUI = slot.GetComponentInChildren<Draggableltem>();
                if (itemUI == null || !IsSellAllItem(itemUI.GetItem())) continue;

                Item item = itemUI.GetItem();
                total += GetSellPrice(item) * itemUI.count;
                soldCounts[item] = (soldCounts.TryGetValue(item, out int n) ? n : 0) + itemUI.count;
                Object.Destroy(itemUI.gameObject);
            }

            if (soldCounts.Count == 0) return false;

            playerMoney.AddMoney(total);
            inventory.NotifyChanged();
            foreach (var pair in soldCounts)
                inventory.RaiseItemDelta(pair.Key, -pair.Value);
```

- [ ] **Step 9: Compile and run tests**

`refresh_unity(compile="request", scope="scripts")`, wait for `is_compiling == false`, `read_console(types=["error"])`.
Expected: only the pre-existing URP error. `grep -rn "AddMoneySilent" Assets/Project_Ocean/Scripts` returns nothing.

`run_tests(mode="EditMode", assembly_names=["Game.Tests.EditMode"], include_failed_tests=true)`.
Expected: 4 passed, 0 failed.

- [ ] **Step 10: Commit (only if the user approved commits)**

```bash
git add Assets/Project_Ocean/Scripts/UI/Notifications/ResourceNotificationPresenter.cs Assets/Project_Ocean/Scripts/Gameplay/Entitys/Players/PlayerMoney.cs "Assets/Project_Ocean/Scripts/Core/Save Game/SaveBridge.cs" Assets/Project_Ocean/Scripts/Gameplay/QuestSystems/Quest/QuestManager.cs Assets/Project_Ocean/Scripts/UI/Inventory/InventoryManager.cs Assets/Project_Ocean/Scripts/UI/Shop/ShopTransactionService.cs Assets/Project_Ocean/Tests/EditMode/NotificationTests.cs
git commit -m "feat: raise money and item deltas for resource notifications"
```

---

### Task 3: Stack view and line

**Files:**
- Create: `Scripts/UI/Notifications/NotificationLine.cs`, `Scripts/UI/Notifications/StackNotificationView.cs`

**Interfaces:**
- Consumes: `NotificationView`, `NotificationMessage` (Task 1)
- Produces:
  - `class NotificationLine : MonoBehaviour` with serialized `icon` (Image), `text` (TextMeshProUGUI), `showDuration`, `fadeDuration`; `public void Setup(NotificationMessage msg)`
  - `class StackNotificationView : NotificationView` with serialized `linePrefab` (NotificationLine), `container` (Transform), `maxLines`

- [ ] **Step 1: Create `NotificationLine`**

`Assets/Project_Ocean/Scripts/UI/Notifications/NotificationLine.cs`:

```csharp
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// <summary>One line in a notification stack; fades out and destroys itself.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class NotificationLine : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float showDuration = 2f;
        [SerializeField] private float fadeDuration = 0.4f;

        private CanvasGroup group;

        public void Setup(NotificationMessage msg)
        {
            group = GetComponent<CanvasGroup>();
            text.text = msg.Text;
            icon.sprite = msg.Icon;
            icon.gameObject.SetActive(msg.Icon != null);
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            yield return Fade(0f, 1f);
            yield return new WaitForSecondsRealtime(showDuration);
            yield return Fade(1f, 0f);
            Destroy(gameObject);
        }

        private IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < fadeDuration; t += Time.unscaledDeltaTime)
            {
                group.alpha = Mathf.Lerp(from, to, t / fadeDuration);
                yield return null;
            }
            group.alpha = to;
        }
    }
}
```

- [ ] **Step 2: Create `StackNotificationView`**

`Assets/Project_Ocean/Scripts/UI/Notifications/StackNotificationView.cs`:

```csharp
using UnityEngine;

namespace Game.Testing
{
    /// <summary>Shows several notifications at once as stacked lines, newest at the bottom.</summary>
    public class StackNotificationView : NotificationView
    {
        [SerializeField] private NotificationLine linePrefab;
        [SerializeField] private Transform container;
        [SerializeField] private int maxLines = 4;

        protected override void OnDisable()
        {
            base.OnDisable();
            // Line coroutines stop while disabled; clear so no frozen lines remain on re-enable.
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }

        protected override void Show(NotificationMessage msg)
        {
            // Destroy runs at end of frame, so detach first or childCount stays over the cap.
            while (container.childCount >= maxLines)
            {
                var oldest = container.GetChild(0);
                oldest.SetParent(null, false);
                Destroy(oldest.gameObject);
            }

            Instantiate(linePrefab, container).Setup(msg);
        }
    }
}
```

- [ ] **Step 3: Compile**

`refresh_unity(compile="request", scope="scripts")`, wait for `is_compiling == false`, `read_console(types=["error"])`.
Expected: only the pre-existing URP error. `run_tests(mode="EditMode", assembly_names=["Game.Tests.EditMode"])` still 4 passed.

- [ ] **Step 4: Commit (only if the user approved commits)**

```bash
git add Assets/Project_Ocean/Scripts/UI/Notifications/NotificationLine.cs Assets/Project_Ocean/Scripts/UI/Notifications/StackNotificationView.cs
git commit -m "feat: add stacked notification view"
```

---

### Task 4: Prefabs and scene setup

**Files:**
- Create: `Prefabs/UI/Notifications/NotificationLine.prefab`, `Prefabs/UI/Notifications/NotificationLayer.prefab`
- Modify: `Levels/Scenes/Menus/AS_PlayerHUD.unity`

**Interfaces:**
- Consumes: `ToastNotificationView`, `StackNotificationView`, `NotificationLine`, `ResourceNotificationPresenter`, `SaveNotificationUI` and their serialized field names from Tasks 1–3. Channel ints: General 0, Center 1, System 2, Resource 3.

- [ ] **Step 1: Make sure Unity is not in Play mode and AS_PlayerHUD has no unsaved edits**

`mcpforunity://editor/state` → `play_mode.is_playing == false`. If `AS_PlayerHUD` is open and dirty, ask the user to save it first.

- [ ] **Step 2: Build prefabs and edit the scene**

Unity MCP `execute_code` (compiler `auto`). Game types are looked up by name so this compiles even if the in-memory compiler cannot reference `Game.Testing` directly:

```csharp
System.Func<string, System.Type> T = n => System.Type.GetType("Game.Testing." + n + ", Game.Testing");
const string scenePath = "Assets/Project_Ocean/Levels/Scenes/Menus/AS_PlayerHUD.unity";
const string dir = "Assets/Project_Ocean/Prefabs/UI/Notifications";

var scene = UnityEditor.SceneManagement.EditorSceneManager.GetSceneByPath(scenePath);
bool openedHere = !scene.isLoaded;
if (openedHere)
    scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Additive);

GameObject hud = null;
foreach (var root in scene.GetRootGameObjects()) if (root.name == "GameHUD") hud = root;
if (hud == null) return "GameHUD not found";
var oldNote = hud.transform.Find("Notification");
var skip = hud.transform.Find("SkipText");
if (oldNote == null || skip == null) return "Notification or SkipText not found";

var oldText = oldNote.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
var oldSave = oldNote.GetComponent(T("SaveNotificationUI"));
string saveMessage = oldSave != null ? new UnityEditor.SerializedObject(oldSave).FindProperty("message").stringValue : "Game Saved!";

if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Project_Ocean/Prefabs/UI")) UnityEditor.AssetDatabase.CreateFolder("Assets/Project_Ocean/Prefabs", "UI");
if (!UnityEditor.AssetDatabase.IsValidFolder(dir)) UnityEditor.AssetDatabase.CreateFolder("Assets/Project_Ocean/Prefabs/UI", "Notifications");

System.Action<TMPro.TextMeshProUGUI, TMPro.TextAlignmentOptions> styleText = (t, align) =>
{
    if (oldText != null) { t.font = oldText.font; t.fontSize = oldText.fontSize; t.color = oldText.color; }
    t.alignment = align;
    t.raycastTarget = false;
};
System.Action<UnityEngine.Object, string, UnityEngine.Object> setRef = (c, field, value) =>
{
    var so = new UnityEditor.SerializedObject(c); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo();
};
System.Action<UnityEngine.Object, string, int> setInt = (c, field, value) =>
{
    var so = new UnityEditor.SerializedObject(c); var p = so.FindProperty(field);
    if (p.propertyType == UnityEditor.SerializedPropertyType.Enum) p.enumValueIndex = value; else p.intValue = value;
    so.ApplyModifiedPropertiesWithoutUndo();
};

// --- NotificationLine.prefab ---
var line = new GameObject("NotificationLine", typeof(RectTransform), typeof(CanvasGroup), typeof(UnityEngine.UI.HorizontalLayoutGroup), typeof(UnityEngine.UI.ContentSizeFitter));
var hlg = line.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
hlg.spacing = 8; hlg.childAlignment = TextAnchor.MiddleLeft;
hlg.childControlWidth = true; hlg.childControlHeight = true; hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
var fit = line.GetComponent<UnityEngine.UI.ContentSizeFitter>();
fit.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize; fit.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
line.GetComponent<CanvasGroup>().blocksRaycasts = false;
var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.LayoutElement));
iconGo.transform.SetParent(line.transform, false);
var le = iconGo.GetComponent<UnityEngine.UI.LayoutElement>(); le.preferredWidth = 32; le.preferredHeight = 32;
var iconImg = iconGo.GetComponent<UnityEngine.UI.Image>(); iconImg.preserveAspect = true; iconImg.raycastTarget = false;
var lineTextGo = new GameObject("Text", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
lineTextGo.transform.SetParent(line.transform, false);
var lineText = lineTextGo.GetComponent<TMPro.TextMeshProUGUI>();
styleText(lineText, TMPro.TextAlignmentOptions.Left);
var lineComp = line.AddComponent(T("NotificationLine"));
setRef(lineComp, "icon", iconImg); setRef(lineComp, "text", lineText);
var linePrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(line, dir + "/NotificationLine.prefab");
UnityEngine.Object.DestroyImmediate(line);

// --- NotificationLayer ---
var layer = new GameObject("NotificationLayer", typeof(RectTransform));
layer.transform.SetParent(hud.transform, false);
layer.transform.SetSiblingIndex(oldNote.GetSiblingIndex());
var lrt = (RectTransform)layer.transform;
lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one; lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
var save = layer.AddComponent(T("SaveNotificationUI"));
var so2 = new UnityEditor.SerializedObject(save); so2.FindProperty("message").stringValue = saveMessage; so2.ApplyModifiedPropertiesWithoutUndo();
layer.AddComponent(T("ResourceNotificationPresenter"));

System.Func<string, int, Vector2, Vector2, TMPro.TextAlignmentOptions, GameObject> makeToast = (name, channel, anchor, pos, align) =>
{
    var go = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
    go.transform.SetParent(layer.transform, false);
    var rt = (RectTransform)go.transform;
    rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor; rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(900, 80);
    var cg = go.GetComponent<CanvasGroup>(); cg.alpha = 0; cg.blocksRaycasts = false; cg.interactable = false;
    var tGo = new GameObject("Text", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
    tGo.transform.SetParent(go.transform, false);
    var trt = (RectTransform)tGo.transform; trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = Vector2.zero; trt.offsetMax = Vector2.zero;
    var t = tGo.GetComponent<TMPro.TextMeshProUGUI>(); styleText(t, align);
    var view = go.AddComponent(T("ToastNotificationView"));
    setInt(view, "channel", channel); setRef(view, "canvasGroup", cg); setRef(view, "messageText", t);
    return go;
};
makeToast("Toast_General", 0, new Vector2(0.5f, 1f), new Vector2(0, -80), TMPro.TextAlignmentOptions.Center);
makeToast("Toast_Center", 1, new Vector2(0.5f, 0.5f), new Vector2(0, 150), TMPro.TextAlignmentOptions.Center);
makeToast("Toast_System", 2, new Vector2(1f, 0f), new Vector2(-40, 110), TMPro.TextAlignmentOptions.Right);

var stack = new GameObject("Stack_Resource", typeof(RectTransform), typeof(UnityEngine.UI.VerticalLayoutGroup));
stack.transform.SetParent(layer.transform, false);
var srt = (RectTransform)stack.transform;
srt.anchorMin = Vector2.zero; srt.anchorMax = Vector2.zero; srt.pivot = Vector2.zero; srt.anchoredPosition = new Vector2(40, 40); srt.sizeDelta = new Vector2(500, 300);
var vlg = stack.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
vlg.childAlignment = TextAnchor.LowerLeft; vlg.spacing = 6;
vlg.childControlWidth = false; vlg.childControlHeight = false; vlg.childForceExpandWidth = false; vlg.childForceExpandHeight = false;
var stackView = stack.AddComponent(T("StackNotificationView"));
setInt(stackView, "channel", 3);
setRef(stackView, "linePrefab", linePrefab.GetComponent(T("NotificationLine")));
setRef(stackView, "container", stack.transform);

UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(layer, dir + "/NotificationLayer.prefab", UnityEditor.InteractionMode.AutomatedAction);

// --- Remove old object, move skip hint to bottom-right ---
UnityEngine.Object.DestroyImmediate(oldNote.gameObject);
var skrt = (RectTransform)skip;
skrt.anchorMin = new Vector2(1f, 0f); skrt.anchorMax = new Vector2(1f, 0f); skrt.pivot = new Vector2(1f, 0f); skrt.anchoredPosition = new Vector2(-40, 40);

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
if (openedHere) UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
return "done";
```

Expected return: `done`.

- [ ] **Step 3: Verify files and scene path**

```bash
cd "D:/UnityGithub/project-ocean/project-ocean" && git status --short Assets/Project_Ocean/Prefabs/UI Assets/Project_Ocean/Levels/Scenes Assets/*.unity
```

Expected: `?? Assets/Project_Ocean/Prefabs/UI/` (or the two prefabs + metas) and ` M Assets/Project_Ocean/Levels/Scenes/Menus/AS_PlayerHUD.unity`. No new `Assets/*.unity` file.

`read_console(types=["error","warning"])` — expected no new `missing script` / `NullReference` entries.

- [ ] **Step 4: Tell the user what to set in the Inspector**

`NotificationLayer` → `ResourceNotificationPresenter.moneyIcon`: assign a coin sprite (optional; money lines show text only until set). Positions and sizes are starting values; the user can tweak them on the prefab.

- [ ] **Step 5: Commit (only if the user approved commits)**

```bash
git add Assets/Project_Ocean/Prefabs/UI Assets/Project_Ocean/Levels/Scenes/Menus/AS_PlayerHUD.unity
git commit -m "feat: add notification layer prefab to player HUD"
```

---

### Task 5: Live verification

**Files:** none changed (fix-ups go back into the task that owns the file).

- [ ] **Step 1: Enter Play mode from Bootstrap and keep it running unfocused**

`manage_editor(action="play")`, then `execute_code`: `Application.runInBackground = true; return "ok";`

- [ ] **Step 2: Fire one message per channel and screenshot**

`execute_code` (Play mode):

```csharp
var nm = System.Type.GetType("Game.Testing.NotificationManager, Game.Testing");
var ch = System.Type.GetType("Game.Testing.NotificationChannel, Game.Testing");
var notify = nm.GetMethod("Notify");
notify.Invoke(null, new object[] { "General test", System.Enum.Parse(ch, "General"), null });
notify.Invoke(null, new object[] { "Center test", System.Enum.Parse(ch, "Center"), null });
notify.Invoke(null, new object[] { "Game Saved!", System.Enum.Parse(ch, "System"), null });
for (int i = 1; i <= 5; i++)
    notify.Invoke(null, new object[] { "+" + i + " Tuna", System.Enum.Parse(ch, "Resource"), null });
return "sent";
```

Then `manage_camera(action="screenshot")` within 1 second.
Expected: text top, center, bottom-right; bottom-left shows 4 lines `+2 Tuna` … `+5 Tuna` (oldest `+1` removed).

- [ ] **Step 3: Gameplay checks**

Play through New Game (or Continue) and confirm each:

| Action | Expected |
|--------|----------|
| Load a save | no money or item notification |
| Buy an item in the shop | bottom-left `-X Money` and `+1 <item>` |
| Sell All | one `-N <item>` per type + `+X Money` |
| Pick up / loot an item in the ocean | bottom-left `+1 <item>` |
| Return to land | bottom-right "Game Saved!"; at 0/3 dives, top "You reach limit to dive today go to sleep" |
| Complete a quest step with money reward | exactly one `+X Money` bottom-left |
| Non-guided quest step starts | center toast, fades |
| Guided tutorial step, then press a blocked key | center prompt; shows again after it has faded |
| Cutscene | skip hint bottom-right, "Game Saved!" above it, not overlapping |
| Pause menu open while a toast fades | toast still fades (unscaled time) |

- [ ] **Step 4: Exit Play mode, final checks**

`manage_editor(action="stop")`. `read_console(types=["error"])` — only the pre-existing URP error.

```bash
cd "D:/UnityGithub/project-ocean/project-ocean" && graphify update .
```
