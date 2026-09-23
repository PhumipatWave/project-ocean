using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class SaveMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;   // the whole panel — toggled on/off
        [SerializeField] private SaveSlotUI[] slotCards; // drag SlotCard_0, SlotCard_1, ... here in order

        // ── Main Menu Continue / New Game (ไม่เปิด panel) ──

        /// <summary>ปุ่ม Continue — โหลด save ล่าสุดทันที</summary>
        public void ContinueGame()
        {
            var metas = SaveSystem.GetAllSlotMeta(); // เรียงใหม่สุดไว้บนสุดอยู่แล้ว
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
            SelectSlot(slot, hasSave: false); // ใช้ path "save ใหม่ + โหลดฉาก" เดิมด้านล่าง
        }

        private int FindFirstEmptySlot(int maxSlots)
        {
            var occupied = SaveSystem.GetAllSlotMeta(maxSlots);
            for (int slot = 0; slot < maxSlots; slot++)
            {
                if (!occupied.Exists(m => m.slot == slot))
                    return slot;
            }
            return 0; // เต็มหมดทุกช่อง — fallback ทับ slot 0
        }

        public void Open()
        {
            panelRoot.SetActive(true);
            Refresh();
        }

        public void Close() => panelRoot.SetActive(false);

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

            if (hasSave)
            {
                SaveBridge.Instance.LoadGame(slot);
            }
            else
            {
                SaveBridge.Instance.ResetForNewGame();
                SaveBridge.Instance.BeginNewGameSession(slot); // file is written by the auto-save after Land loads
                SceneManager.LoadScene("Land_Map"); // เปลี่ยนเป็นชื่อ scene แรกจริงของคุณ
            }
        }

        public void DeleteSlot(int slot)
        {
            SaveSystem.Delete(slot);
            Refresh();
        }
    }
}