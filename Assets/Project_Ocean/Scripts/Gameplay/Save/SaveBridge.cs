using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;

namespace Game.Gameplay
{
    public class SaveBridge : Singleton<SaveBridge>
    {

        /// Fired after a successful save so UI can show a "Game Saved" toast.
        public static event System.Action OnSaved;

        /// <summary>The active save slot; auto-saves always write here.</summary>
        public int CurrentSlot { get; private set; } = 0;

        private SaveData _pendingData;

        // Pressing Play on Land_Map in the Editor skips New Game/Continue; without this guard the
        // scene-load auto-save would overwrite the real save with fresh data.
        private bool _sessionStarted;

        // Order is intentional and stable: it is also the apply order on load. Built fresh each
        // call because the player and scene-level managers may not exist at save time.
        private List<ISaveParticipant> BuildParticipants()
        {
            var list = new List<ISaveParticipant>();
            var player = PlayerRegistry.Local;
            if (player != null)
            {
                var health = player.GetComponent<PlayerHealth>();
                if (health != null) list.Add(health);
                var money = player.GetComponent<PlayerMoney>();
                if (money != null) list.Add(money);
            }
            if (Inventory.Current is ISaveParticipant inventory) list.Add(inventory);
            if (DayManager.Instance != null)           list.Add(DayManager.Instance);
            list.Add(UpgradeService.Instance);
            if (QuestManager.Instance != null)         list.Add(QuestManager.Instance);
            if (EnemyKillTracker.Instance != null)     list.Add(EnemyKillTracker.Instance);
            if (FishCollectionManager.Instance != null)list.Add(FishCollectionManager.Instance);
            if (OneTimePickupTracker.Instance != null) list.Add(OneTimePickupTracker.Instance);
            // Must come after QuestManager — it re-derives unlocks from quest progress on load.
            list.Add(FeatureUnlockService.Instance);
            return list;
        }

        // Shares its GameObject; persistence comes from the GameManager root.
        protected override bool DestroyGameObjectOnDuplicate => false;

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

            // Auto-save on returning to land, one frame late so the player is already at the land spawn point.
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
            // Every sceneLoaded handler has run by next frame, so the position is the land one.
            yield return null;
            SaveGame();
        }

        private IEnumerator ApplyAfterFrame(SaveData data)
        {
            yield return null;

            try
            {
                // Position is applied on scene load (SceneLoadContext); each other slice by its owner, in BuildParticipants order.
                foreach (var p in BuildParticipants())
                    p.Apply(data);

                SceneLoadContext.Clear();
                GameLog.Info("[SaveBridge] Post-load data applied!");
            }
            catch (System.Exception e)
            {
                SceneLoadContext.Clear();
                Debug.LogError($"[SaveBridge] Failed to apply save data: {e.Message}");
            }
            finally
            {
                LoadingManager.Unblock(this); // save fully applied — loading screen may hide
            }
        }

        // -------------------- Public API -------------------- //

        /// <summary>Resets every persistent system for a New Game and destroys the player (a fresh one spawns on the next scene).</summary>
        // These managers survive the menu scene load, so without this the old run's progress would carry over.
        public void ResetForNewGame()
        {
            if (PlayerRegistry.Local != null)
            {
                // Destroy() is deferred to end of frame — unregister now so nothing later in this
                // frame (SaveGame, ResetToDefault below) still reads the previous run's player.
                var oldPlayer = PlayerRegistry.Local;
                PlayerRegistry.Unregister(oldPlayer);
                Destroy(oldPlayer.gameObject);
            }

            // Before QuestManager: its reset raises OnQuestUpdated, and CutsceneManager must not still hold the old
            // run's cutscene state when it starts the new game's first Cutscene step.
            CutsceneManager.Instance?.ResetToDefault();
            QuestManager.Instance?.ResetToDefault();
            FeatureUnlockService.Instance.ResetToDefault();
            Inventory.Current?.ResetToDefault();
            EnemyKillTracker.Instance?.ResetAll();
            FishCollectionManager.Instance?.ResetToDefault();
            OneTimePickupTracker.Instance?.ResetAll();
            UpgradeService.Instance.ResetToDefault();

            if (DayManager.Instance != null)
            {
                DayManager.Instance.SetDay(1);
                DayManager.Instance.SetDivesToday(0);
                DayManager.Instance.SetTotalDives(0);
            }
        }

        /// <summary>Makes the slot active without writing it; the auto-save after Land loads writes it from the fresh player.</summary>
        public void BeginNewGameSession(int slot)
        {
            CurrentSlot = slot;
            _sessionStarted = true;
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
            };

            CollectPosition(data);
            data.inventoryItems = new List<ItemSaveData>();

            // Every slice is collected by its owning system (see BuildParticipants).
            foreach (var p in BuildParticipants())
                p.Collect(data);

            try
            {
                SaveSystem.Save(data, slot);
                GameLog.Info($"[SaveBridge] Game saved to slot {slot}!");
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
            // A load is already running: LoadingManager would ignore this scene request, leaving
            // _pendingData/SceneLoadContext set for whatever scene loads next.
            if (LoadingManager.IsLoading) return;

            CurrentSlot = slot;
            _sessionStarted = true;

            if (!SaveSystem.HasSave(slot))
            {
                if (SaveSystem.RestoreBackup(slot))
                    GameLog.Info($"[SaveBridge] Restored slot {slot} from backup, retrying load...");
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

            gm.GameState = data.gameState;

            _pendingData = data;

            // Hand off the load position + flag through the shared context.
            // Position MUST be set before LoadScene() — BeginLoad does both atomically.
            SceneLoadContext.BeginLoad(new Vector3(data.posX, data.posY, data.posZ));

            // Through the loading screen, which stays up until ApplyAfterFrame has applied the data.
            if (LoadingManager.Instance != null)
            {
                LoadingManager.Block(this);
                LoadingManager.Instance.LoadScene(data.currentScene, LoadingType.Menu);
            }
            else
                SceneManager.LoadScene(data.currentScene);
            GameLog.Info($"[SaveBridge] Loading scene for slot {slot}...");
        }

        /// <summary>For a save/load menu: metadata for every occupied slot.</summary>
        public List<SaveSlotMeta> GetAllSlotMeta(int maxSlots = 10) =>
            SaveSystem.GetAllSlotMeta(maxSlots);

        // -------------------- Collect helpers -------------------- //

        private void CollectPosition(SaveData data)
        {
            var player = PlayerRegistry.Local;
            if (player == null) return;

            var pos   = player.transform.position;
            data.posX = pos.x;
            data.posY = pos.y;
            data.posZ = pos.z;
        }

    }
}
