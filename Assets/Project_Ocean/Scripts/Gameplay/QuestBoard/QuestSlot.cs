using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Game.Testing
{
    [RequireComponent(typeof(Button))]
    public class QuestSlot : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private GameObject statusIcon; // ไอคอนดาว หรือ เครื่องหมายตกใจ

        private Button slotButton;
        private Quest currentQuest;
        private Action<Quest> onSlotClickedCallback;

        private void Awake()
        {
            slotButton = GetComponent<Button>();
            slotButton.onClick.AddListener(HandleSlotClick);
        }

        /// <summary>
        /// อัปเดตข้อมูลและแสดงผล Slot เควส
        /// </summary>
        public void SetupSlot(Quest quest, Action<Quest> onClickedAction)
        {
            currentQuest = quest;
            onSlotClickedCallback = onClickedAction;

            gameObject.SetActive(true);

            if (questNameText != null)
            {
                questNameText.text = quest.QuestName; // ใช้ Property เดิมของคุณ
            }

            if (statusIcon != null)
            {
                statusIcon.SetActive(true);
                // ต่อยอดต่อ: สามารถเขียน Logic เปลี่ยนรูปไอคอนตรงนี้ตามประเภทเควสได้ เช่น quest.Type
            }
        }

        /// <summary>
        /// ซ่อน Slot เมื่อไม่มีข้อมูลเควสให้แสดงในหน้านั้น
        /// </summary>
        public void ClearSlot()
        {
            gameObject.SetActive(false);
            currentQuest = null;
            onSlotClickedCallback = null;
        }

        private void HandleSlotClick()
        {
            if (currentQuest != null)
            {
                onSlotClickedCallback?.Invoke(currentQuest);
            }
        }
    }
}