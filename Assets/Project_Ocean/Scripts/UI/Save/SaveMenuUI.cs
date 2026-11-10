using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class SaveMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;   // the whole panel — toggled on/off
        [SerializeField] private SaveSlotUI[] slotCards; // drag SlotCard_0, SlotCard_1, ... here in order
        [SerializeField] private Button continueButton;

        // True while the panel was opened by New Game with every slot full: picking a slot replaces it.
        private bool replaceMode;

        /// <summary>Continue button: loads the newest save straight away.</summary>
        public void ContinueGame()
        {
            // Same slot range the menu shows (slotCards.Length) — the default of 10 also picked up
            // saves in slots the menu can't display, so Continue could load a save you can't see.
            var metas = SaveSystem.GetAllSlotMeta(slotCards.Length); // newest first
            if (metas.Count == 0)
            {
                Debug.LogWarning("[SaveMenuUI] Continue pressed but there are no saves");
                return;
            }
            SelectSlot(metas[0].slot, hasSave: true);
        }

        /// <summary>New Game button: picks an empty slot and starts straight away.</summary>
        public void NewGame()
        {
            int slot = FindFirstEmptySlot(maxSlots: slotCards.Length);

            // Every slot is taken: let the player pick one to replace (SelectSlot confirms first); never overwrite silently.
            if (slot < 0)
            {
                OpenPanel(replace: true);
                NotificationManager.Notify("All save slots are full — select a save to replace with a new game.");
                return;
            }

            SelectSlot(slot, hasSave: false);
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

        public void Open() => OpenPanel(replace: false);

        // Set before Refresh: the cards pick Select vs Load/Delete from it.
        private void OpenPanel(bool replace)
        {
            replaceMode = replace;
            panelRoot.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            replaceMode = false;
            panelRoot.SetActive(false);
            RefreshContinueButton();
        }

        /// <summary>Continue only works when at least one slot the menu shows has a save.</summary>
        public void RefreshContinueButton()
        {
            if (continueButton != null)
                continueButton.interactable = SaveSystem.GetAllSlotMeta(slotCards.Length).Count > 0;
        }

        private void Refresh()
        {
            // Read from SaveSystem, not SaveBridge: SaveBridge may not exist yet in the main menu.
            var metas = SaveSystem.GetAllSlotMeta(slotCards.Length);

            for (int slot = 0; slot < slotCards.Length; slot++)
            {
                var meta = metas.Find(m => m.slot == slot);
                slotCards[slot].Setup(slot, meta, this, replaceMode);
            }
        }

        public void SelectSlot(int slot, bool hasSave)
        {
            if (SaveBridge.Instance == null)
            {
                // SaveBridge lives on the persistent GameManager root, which must exist before this panel is used.
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
                // Opened in-game (Land home panel): loading drops unsaved progress, so ask first.
                bool inGame = SceneNames.IsGameplay(SceneManager.GetActiveScene().name);
                if (inGame && ConfirmationDialog.Instance != null)
                    ConfirmationDialog.Instance.Show(
                        "Load this save? Unsaved progress will be lost.",
                        () => { Close(); SaveBridge.Instance.LoadGame(slot); });
                else
                    SaveBridge.Instance.LoadGame(slot);
            }
            else
            {
                // A load is already running: LoadingManager would ignore the scene request below, so the
                // game would be reset without ever switching scenes.
                if (LoadingManager.IsLoading) return;

                SaveBridge.Instance.ResetForNewGame();
                SaveBridge.Instance.BeginNewGameSession(slot); // file is written by the auto-save after Land loads

                // Load through the loading screen; CutsceneManager waits for it before the prologue, so resetting first is safe.
                if (LoadingManager.Instance != null)
                    LoadingManager.Instance.StartGame();
                else
                    SceneManager.LoadScene(SceneNames.Land);
            }
        }

        public void DeleteSlot(int slot)
        {
            // Never delete without asking.
            if (ConfirmationDialog.Instance == null)
            {
                NotificationManager.Notify("Can't confirm — no save was deleted.");
                return;
            }

            ConfirmationDialog.Instance.Show(
                $"Delete save slot {slot + 1}? The save will be erased.",
                () =>
                {
                    SaveSystem.Delete(slot);
                    Refresh();
                    RefreshContinueButton();
                });
        }
    }
}