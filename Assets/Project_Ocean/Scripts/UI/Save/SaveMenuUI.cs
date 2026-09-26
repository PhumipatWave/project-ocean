using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class SaveMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;   // the whole panel — toggled on/off
        [SerializeField] private SaveSlotUI[] slotCards; // drag SlotCard_0, SlotCard_1, ... here in order

        // True while the panel was opened by New Game with every slot full: picking a slot replaces it.
        private bool replaceMode;

        // ── Main Menu Continue / New Game (ไม่เปิด panel) ──

        /// <summary>ปุ่ม Continue — โหลด save ล่าสุดทันที</summary>
        public void ContinueGame()
        {
            // Same slot range the menu shows (slotCards.Length) — the default of 10 also picked up
            // saves in slots the menu can't display, so Continue could load a save you can't see.
            var metas = SaveSystem.GetAllSlotMeta(slotCards.Length); // เรียงใหม่สุดไว้บนสุดอยู่แล้ว
            if (metas.Count == 0)
            {
                Debug.LogWarning("[SaveMenuUI] กด Continue แต่ไม่มี save เลย");
                return;
            }
            SelectSlot(metas[0].slot, hasSave: true); // ใช้ path โหลดเดิมด้านล่าง
        }

        /// <summary>ปุ่ม New Game — หา slot ว่างเอง แล้วเข้าเกมทันที</summary>
        public void NewGame()
        {
            int slot = FindFirstEmptySlot(maxSlots: slotCards.Length);

            // Every slot is taken: let the player pick one to replace (SelectSlot confirms before wiping).
            // Never falls back to overwriting a slot on its own — that used to silently clobber slot 0.
            if (slot < 0)
            {
                Open();
                replaceMode = true;
                NotificationManager.Notify("All save slots are full — select a save to replace with a new game.");
                return;
            }

            SelectSlot(slot, hasSave: false); // ใช้ path "save ใหม่ + โหลดฉาก" เดิมด้านล่าง
        }

        // Wipes the slot's data and starts a fresh game in it. Delete() keeps a backup by default, so an
        // accidental "yes" is recoverable (LoadGame restores the backup if the slot has no file).
        private void RestartSlot(int slot)
        {
            SaveSystem.Delete(slot);
            SelectSlot(slot, hasSave: false);
        }

        private int FindFirstEmptySlot(int maxSlots)
        {
            var occupied = SaveSystem.GetAllSlotMeta(maxSlots);
            for (int slot = 0; slot < maxSlots; slot++)
            {
                if (!occupied.Exists(m => m.slot == slot))
                    return slot;
            }
            return -1; // every slot is occupied — the caller must NOT fall back to overwriting one
        }

        public void Open()
        {
            replaceMode = false; // NewGame turns it on again right after opening
            panelRoot.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            replaceMode = false;
            panelRoot.SetActive(false);
        }

        private void Refresh()
        {
            // Read slot metadata straight from SaveSystem (a static class) instead of
            // going through SaveBridge.Instance. SaveBridge might not exist yet at this
            // point (e.g. we're still in the Main Menu scene and the GameManager/SaveBridge
            // root hasn't been spawned), and GetAllSlotMeta doesn't need an instance anyway.
            // This was previously throwing a NullReferenceException here, which meant the
            // loop below never ran and NONE of the slot buttons ever got their listeners
            // wired up (only "Back" worked, since that's bound directly in the Inspector).
            var metas = SaveSystem.GetAllSlotMeta(slotCards.Length);

            for (int slot = 0; slot < slotCards.Length; slot++)
            {
                var meta = metas.Find(m => m.slot == slot);
                slotCards[slot].Setup(slot, meta, this);
            }
        }

        public void SelectSlot(int slot, bool hasSave)
        {
            if (SaveBridge.Instance == null)
            {
                // If you land here, SaveBridge (usually a child of your GameManager
                // persistent root) doesn't exist yet in this scene. Make sure that
                // root object is present/DontDestroyOnLoad'd before the Main Menu
                // shows this panel (e.g. spawn it from a boot scene, or place it
                // directly in MainMenuScene).
                Debug.LogError("[SaveMenuUI] SaveBridge.Instance is null — can't start/load game. " +
                                "Is the GameManager/SaveBridge root present in this scene?");
                return;
            }

            if (hasSave && replaceMode)
            {
                // Picked a slot to overwrite with a new game: confirm first, never wipe silently.
                if (ConfirmationDialog.Instance == null)
                {
                    NotificationManager.Notify("Can't confirm — no save was replaced.");
                    return;
                }

                ConfirmationDialog.Instance.Show(
                    "Replace this save with a new game? The old save will be erased.",
                    () => { replaceMode = false; RestartSlot(slot); });
            }
            else if (hasSave)
            {
                SaveBridge.Instance.LoadGame(slot);
            }
            else
            {
                // A load is already running: LoadingManager would ignore the scene request below, so the
                // game would be reset without ever switching scenes.
                if (LoadingManager.IsLoading) return;

                SaveBridge.Instance.ResetForNewGame();
                SaveBridge.Instance.BeginNewGameSession(slot); // file is written by the auto-save after Land loads

                // Through the loading screen, as the old New Game button did. CutsceneManager waits for
                // LoadingManager to finish before starting the prologue, so resetting first is safe.
                if (LoadingManager.Instance != null)
                    LoadingManager.Instance.StartGame();
                else
                    SceneManager.LoadScene(SceneNames.Land);
            }
        }

        public void DeleteSlot(int slot)
        {
            SaveSystem.Delete(slot);
            Refresh();
        }
    }
}