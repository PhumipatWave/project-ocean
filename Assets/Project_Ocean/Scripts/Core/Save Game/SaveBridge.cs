using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class SaveBridge : MonoBehaviour
    {
        public static SaveBridge Instance { get; private set; }

        /// Fired after a successful save so UI can show a "Game Saved" toast.
        public static event System.Action OnSaved;

        /// <summary>
        /// The slot most recently used for save/load. The save/load menu should
        /// set this (or call the (int slot) overloads directly) before the
        /// player picks a slot; auto-saves (day change, returning to land)
        /// always write to whichever slot is currently active.
        /// </summary>
        public int CurrentSlot { get; private set; } = 0;

        private SaveData _pendingData;

        // True once this play session has gone through SaveGame(slot)/LoadGame(slot) — the
        // explicit "New Game"/"Continue" entry points (see SaveMenuUI). Entering Land_Map
        // directly (e.g. pressing Play on that scene in the Editor) skips both, so without
        // this guard OnSceneLoaded's auto-save would silently overwrite the real save with
        // default/fresh state the moment the scene loads.
        private bool _sessionStarted;

        // Order is intentional and stable. Built fresh each call because scene-level
        // managers (QuestManager, EnemyKillTracker) may not exist at save time.
        private List<ISaveParticipant> BuildParticipants()
        {
            var list = new List<ISaveParticipant> { UpgradeService.Instance };
            if (QuestManager.Instance != null)     list.Add(QuestManager.Instance);
            if (EnemyKillTracker.Instance != null) list.Add(EnemyKillTracker.Instance);
            // Must come after QuestManager — it re-derives unlocks from quest progress on load.
            list.Add(FeatureUnlockService.Instance);
            return list;
        }

        void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            // Persistence is handled by the parent GameManager root's DontDestroyOnLoad.
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            DayManager.OnDayAdvanced += SaveGame; // auto-save on day change (uses CurrentSlot)
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            DayManager.OnDayAdvanced -= SaveGame;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_pendingData != null)
            {
                var data = _pendingData;
                _pendingData = null;
                StartCoroutine(ApplyAfterFrame(data));
                return;
            }

            // Auto-save whenever the player returns to land from the ocean.
            // Delayed one frame so GameManager.OnSceneLoaded (which repositions
            // the player to the land PlayerSpawnPoint) has already run before we
            // capture the position — otherwise this can save the stale ocean pos.
            if (scene.name == SceneNames.Land && GameManager.Instance != null)
            {
                if (_sessionStarted)
                    StartCoroutine(AutoSaveAfterFrame());
                else
                    Debug.LogWarning("[SaveBridge] Landed on " + SceneNames.Land +
                        " without going through New Game/Continue this session — skipping " +
                        "auto-save so slot " + CurrentSlot + " isn't overwritten.");
            }
        }

        private IEnumerator AutoSaveAfterFrame()
        {
            // GameManager's OnSceneLoaded runs synchronously in the same
            // SceneManager.sceneLoaded broadcast (regardless of subscriber
            // order), so by the time this coroutine resumes next frame the
            // player has already been moved to the land PlayerSpawnPoint.
            yield return null;
            SaveGame();
        }

        private IEnumerator ApplyAfterFrame(SaveData data)
        {
            yield return null;

            try
            {
                // Position is applied by GameManager.OnSceneLoaded (via SceneLoadContext),
                // not here.
                ApplyPlayerStats(data);
                ApplyInventory(data);
                ApplyDay(data);

                // Upgrade + quest slices are applied by their owning systems,
                // in the fixed order defined by Participants.
                foreach (var p in BuildParticipants())
                    p.Apply(data);

                SceneLoadContext.Clear();
                Debug.Log("[SaveBridge] Post-load data applied!");
            }
            catch (System.Exception e)
            {
                SceneLoadContext.Clear();
                Debug.LogError($"[SaveBridge] Failed to apply save data: {e.Message}");
            }
        }

        // -------------------- Apply helpers -------------------- //

        private void ApplyPlayerStats(SaveData data)
        {
            var player = GameManager.CurPlayer;
            if (player == null)
            {
                Debug.LogWarning("[SaveBridge] ApplyPlayerStats: CurPlayer is null");
                return;
            }

            var tube = player.OxygenTube;
            if (tube != null)
            {
                // SetOriginalMaxLength syncs both the rope and PlayerHealth's snapshot
                // so that ResetToOriginal() after the next dive restores the saved upgraded max.
                var ph = player.GetComponent<PlayerHealth>();
                if (ph != null)
                    ph.SetOriginalMaxLength(data.maxRopeLength);
                else
                    tube.MaxRopeLength = data.maxRopeLength;
            }

            var playerMoney = player.GetComponent<PlayerMoney>();
            if (playerMoney != null)
            {
                playerMoney.Use(playerMoney.CurrentMoney);
                playerMoney.AddMoneySilent(data.money);
            }
        }

        private void ApplyInventory(SaveData data)
        {
            var inv = InventoryManager.Instance;
            if (inv == null)
            {
                Debug.LogWarning("[SaveBridge] ApplyInventory: InventoryManager not found");
                return;
            }

            // Clear all existing items first
            foreach (var slot in inv.InventorySlots)
            {
                var existing = slot.GetComponentInChildren<Draggableltem>();
                if (existing != null) Destroy(existing.gameObject);
            }

            foreach (var saved in data.inventoryItems)
            {
                if (!int.TryParse(saved.itemID, out int id))
                {
                    Debug.LogWarning($"[SaveBridge] Invalid itemID: '{saved.itemID}'");
                    continue;
                }

                // Look up item from the allItems database — NOT from current inventory slots
                var item = inv.FindItemInDatabase(id);
                if (item == null)
                {
                    // LogError (not LogWarning): this means the item was silently dropped
                    // from the loaded inventory — a data-loss bug, not a minor issue.
                    // Usually caused by the item's ScriptableObject missing from
                    // InventoryManager's "Item Database -> All Items" list in the Inspector.
                    Debug.LogError($"[SaveBridge] Item ID {id} not found in allItems database — " +
                                    $"item was DROPPED from inventory on load! Add its ScriptableObject to " +
                                    $"InventoryManager's Item Database list.");
                    continue;
                }

                if (saved.slotIndex >= inv.InventorySlots.Length)
                {
                    Debug.LogWarning($"[SaveBridge] slotIndex {saved.slotIndex} out of range");
                    continue;
                }

                // Spawn with the saved stack count in one step — the item is born
                // with the correct count and a single RefreshCount(), so the count
                // label shows immediately regardless of load order.
                inv.SpawnItemAtSlot(item, saved.slotIndex, Mathf.Max(1, saved.count));
            }
        }

        private void ApplyDay(SaveData data)
        {
            if (DayManager.Instance == null) { Debug.LogWarning("[SaveBridge] ApplyDay: DayManager not found"); return; }
            DayManager.Instance.SetDay(data.currentDay);
            // Saves from before divesToday existed only carry the bool — count that as one dive.
            DayManager.Instance.SetDivesToday(data.divesToday > 0 ? data.divesToday : (data.hasDivedToday ? 1 : 0));
            DayManager.Instance.SetTotalDives(data.totalDives);
        }

        // -------------------- Public API -------------------- //

        /// <summary>
        /// Resets every persistent system to its fresh-game default — call before SaveGame()
        /// when starting a New Game. Without this, "New Game" would just snapshot whatever's
        /// currently in memory into the new slot: every one of these is a DontDestroyOnLoad
        /// singleton that survives GameManager.ToMainMenu()'s scene load untouched, so a
        /// previous playthrough's quest progress/unlocks/inventory/day count would carry over.
        /// Destroys the current player outright (rather than resetting its components
        /// individually) — GameManager.SpawnPlayer() rebuilds a fresh one, with prefab-default
        /// stats, the next time a gameplay scene loads.
        /// </summary>
        public void ResetForNewGame()
        {
            if (GameManager.CurPlayer != null)
                Destroy(GameManager.CurPlayer.gameObject);

            // Before QuestManager — clears any coroutine/lock left over from a PREVIOUS
            // interrupted attempt while activeQuestStep still points at the about-to-be-discarded
            // quest objects. QuestManager.ResetToDefault() below fires OnQuestUpdated as a side
            // effect of rebuilding, which CutsceneManager is subscribed to — with this reset
            // already done, that pass legitimately (re)starts a Cutscene-type first step (e.g.
            // Prologue) for the fresh game, instead of that trigger racing a stale one.
            CutsceneManager.Instance?.ResetToDefault();
            QuestManager.Instance?.ResetToDefault();
            FeatureUnlockService.Instance.ResetToDefault();
            InventoryManager.Instance?.ResetToDefault();
            EnemyKillTracker.Instance?.ResetAll();
            UpgradeService.Instance.ResetToDefault();

            if (DayManager.Instance != null)
            {
                DayManager.Instance.SetDay(1);
                DayManager.Instance.SetDivesToday(0);
                DayManager.Instance.SetTotalDives(0);
            }

            if (GameManager.Instance != null)
                GameManager.Instance.MiniHubAvaliable = false;
        }

        /// <summary>Saves to whichever slot was last used (see CurrentSlot).</summary>
        public void SaveGame() => SaveGame(CurrentSlot);

        /// <summary>Saves to a specific slot and remembers it as the active slot.</summary>
        public void SaveGame(int slot)
        {
            CurrentSlot = slot;
            _sessionStarted = true;

            var gm = GameManager.Instance;
            if (gm == null) { Debug.LogError("[SaveBridge] GameManager not found"); return; }

            var data = new SaveData
            {
                gameState        = gm.GameState,
                currentScene     = SceneManager.GetActiveScene().name,
                miniHubAvailable = gm.MiniHubAvaliable,
            };

            CollectPosition(data);
            CollectPlayerStats(data);
            CollectInventory(data);
            CollectDay(data);

            // Upgrade + quest slices are collected by their owning systems,
            // in the fixed order defined by Participants.
            foreach (var p in BuildParticipants())
                p.Collect(data);

            try
            {
                SaveSystem.Save(data, slot);
                Debug.Log($"[SaveBridge] Game saved to slot {slot}!");
                OnSaved?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SaveBridge] SaveGame failed (slot {slot}): {e.Message}");
            }
        }

        /// <summary>Loads whichever slot was last used (see CurrentSlot).</summary>
        public void LoadGame() => LoadGame(CurrentSlot);

        /// <summary>Loads a specific slot and remembers it as the active slot.</summary>
        public void LoadGame(int slot)
        {
            CurrentSlot = slot;
            _sessionStarted = true;

            if (!SaveSystem.HasSave(slot))
            {
                if (SaveSystem.RestoreBackup(slot))
                    Debug.Log($"[SaveBridge] Restored slot {slot} from backup, retrying load...");
                else
                {
                    Debug.LogWarning($"[SaveBridge] No save file found for slot {slot}");
                    return;
                }
            }

            var data = SaveSystem.Load(slot);
            if (data == null)
            {
                Debug.LogError($"[SaveBridge] Failed to load save data (slot {slot})");
                return;
            }

            var gm = GameManager.Instance;
            if (gm == null) { Debug.LogError("[SaveBridge] GameManager not found"); return; }

            gm.GameState        = data.gameState;
            gm.MiniHubAvaliable = data.miniHubAvailable;

            _pendingData = data;

            // Hand off the load position + flag through the shared context.
            // Position MUST be set before LoadScene() — BeginLoad does both atomically.
            SceneLoadContext.BeginLoad(new Vector3(data.posX, data.posY, data.posZ));

            SceneManager.LoadScene(data.currentScene);
            Debug.Log($"[SaveBridge] Loading scene for slot {slot}...");
        }

        /// <summary>For a save/load menu: metadata for every occupied slot.</summary>
        public List<SaveSlotMeta> GetAllSlotMeta(int maxSlots = 10) =>
            SaveSystem.GetAllSlotMeta(maxSlots);

        // -------------------- Collect helpers -------------------- //

        private void CollectPosition(SaveData data)
        {
            var player = GameManager.CurPlayer;
            if (player == null) return;

            var pos   = player.transform.position;
            data.posX = pos.x;
            data.posY = pos.y;
            data.posZ = pos.z;
        }

        private void CollectPlayerStats(SaveData data)
        {
            var player = GameManager.CurPlayer;
            if (player == null) return;

            var tube = player.OxygenTube;
            if (tube != null)
                data.maxRopeLength = tube.MaxRopeLength;

            var playerMoney = player.GetComponent<PlayerMoney>();
            if (playerMoney != null)
                data.money = playerMoney.CurrentMoney;
        }

        private void CollectInventory(SaveData data)
        {
            data.inventoryItems = new List<ItemSaveData>();
            if (InventoryManager.Instance == null) return;

            var slots = InventoryManager.Instance.InventorySlots;
            for (int i = 0; i < slots.Length; i++)
            {
                var item = slots[i].GetComponentInChildren<Draggableltem>();
                if (item?.GetItem() == null) continue;

                data.inventoryItems.Add(new ItemSaveData
                {
                    slotIndex = i,
                    itemID    = item.GetItem().ItemID.ToString(),
                    count     = item.count,
                });
            }
        }

        private void CollectDay(SaveData data)
        {
            if (DayManager.Instance == null) return;
            data.currentDay    = DayManager.Instance.CurrentDay;
            data.hasDivedToday = DayManager.Instance.HasDivedToday;
            data.divesToday    = DayManager.Instance.DivesToday;
            data.totalDives    = DayManager.Instance.TotalDives;
        }
    }
}
