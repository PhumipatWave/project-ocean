using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;

namespace Game.Gameplay
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

        // Order is intentional and stable: it is also the apply order on load. Built fresh each
        // call because the player and scene-level managers may not exist at save time.
        private List<ISaveParticipant> BuildParticipants()
        {
            var list = new List<ISaveParticipant>();
            var player = GameManager.CurPlayer;
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
            if (OneTimePickupTracker.Instance != null) list.Add(OneTimePickupTracker.Instance);
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
                // Every slice is applied by its owning system, in the fixed order of BuildParticipants.
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
            {
                // Destroy() is deferred to end of frame — unregister now so nothing later in this
                // frame (SaveGame, ResetToDefault below) still reads the previous run's player.
                var oldPlayer = GameManager.CurPlayer;
                PlayerRegistry.Unregister(oldPlayer);
                Destroy(oldPlayer.gameObject);
            }

            // Before QuestManager — clears any coroutine/lock left over from a PREVIOUS
            // interrupted attempt while activeQuestStep still points at the about-to-be-discarded
            // quest objects. QuestManager.ResetToDefault() below fires OnQuestUpdated as a side
            // effect of rebuilding, which CutsceneManager is subscribed to — with this reset
            // already done, that pass legitimately (re)starts a Cutscene-type first step (e.g.
            // Prologue) for the fresh game, instead of that trigger racing a stale one.
            CutsceneManager.Instance?.ResetToDefault();
            QuestManager.Instance?.ResetToDefault();
            FeatureUnlockService.Instance.ResetToDefault();
            Inventory.Current?.ResetToDefault();
            EnemyKillTracker.Instance?.ResetAll();
            OneTimePickupTracker.Instance?.ResetAll();
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

        /// <summary>
        /// New Game entry point: makes <paramref name="slot"/> the active slot WITHOUT writing it yet.
        /// The fresh player doesn't exist until the next scene loads, so saving now would snapshot
        /// stale/default values (this used to leak the previous run's money and rope length into
        /// the new slot). The auto-save that runs after Land loads writes it from the real player.
        /// </summary>
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
                miniHubAvailable = gm.MiniHubAvaliable,
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

            gm.GameState        = data.gameState;
            gm.MiniHubAvaliable = data.miniHubAvailable;

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
            var player = GameManager.CurPlayer;
            if (player == null) return;

            var pos   = player.transform.position;
            data.posX = pos.x;
            data.posY = pos.y;
            data.posZ = pos.z;
        }

    }
}
