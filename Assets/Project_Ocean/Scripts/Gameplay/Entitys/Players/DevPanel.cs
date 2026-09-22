using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Testing
{
    /// <summary>
    /// Dev/Cheat Panel แบบไฟล์เดียว — ไม่แก้ PlayerHealth, OxygenRope, PlayerMoney, GameManager,
    /// InventoryManager เลย ทำงานผ่าน public API ที่มีอยู่แล้วของทุกคลาสเท่านั้น
    ///
    /// God Mode         -> เรียก PlayerHealth.Heal() เติมเต็มทุกเฟรม
    /// Infinite Oxygen   -> ตั้ง OxygenRope.MaxRopeLength ให้สูงมากทุกเฟรม กัน breach/นับถอยหลัง
    /// Add Money         -> เรียก PlayerMoney.AddMoney() ตรงๆ
    /// Add Item          -> เรียก InventoryManager.Instance.AddItem(item) ตรงๆ ต่อปุ่มที่กด
    /// </summary>
    public class DevPanel : MonoBehaviour
    {
        [System.Serializable]
        private struct DevItemButton
        {
            public Button button;   // ปุ่มปลา/ไอเทมในกริด (ลากจาก Hierarchy)
            public Item item;       // Item ScriptableObject ที่ปุ่มนี้จะให้
            public int amount;      // จำนวนที่ให้ต่อการกด 1 ครั้ง (ค่าเริ่มต้น 1 ถ้าใส่ 0 หรือติดลบ)
        }

        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;   // กล่อง panel ทั้งหมด (ซ่อน/โชว์)
        [SerializeField] private Button toggleButton;    // ไอคอนมุมจอ กดเปิด-ปิด

        [Header("Cheats")]
        [SerializeField] private Toggle godModeToggle;
        [SerializeField] private Toggle infiniteOxygenToggle;
        [SerializeField] private Button addMoneyButton;
        [SerializeField] private int addMoneyAmount = 9999;
        [SerializeField] private Button skipStepButton;

        [Header("Add Item")]
        [Tooltip("ลากปุ่มแต่ละอันในกริด (เช่น Button Fish S (0)..(8)) มาคู่กับ Item ที่ต้องการให้")]
        [SerializeField] private DevItemButton[] devItemButtons;

        [Header("Settings")]
        [Tooltip("ค่าที่ใช้ตั้งเป็น MaxRopeLength ตอน Infinite Oxygen เปิดอยู่ ยิ่งสูงยิ่งชัวร์ว่าไม่มีวันเกินระยะ")]
        [SerializeField] private float infiniteRopeLength = 999999f;

        [Tooltip("ค่าที่ใช้ Heal ทุกเฟรมตอน God Mode เปิดอยู่ (Heal() เดิมจะ clamp ไม่ให้เกิน Max Health เองอยู่แล้ว)")]
        [SerializeField] private float godModeHealAmount = 999999f;

        private bool godModeOn;
        private bool infiniteOxygenOn;

        private void Start()
        {
            panelRoot.SetActive(false);

            toggleButton.onClick.AddListener(TogglePanel);
            godModeToggle.onValueChanged.AddListener(v => godModeOn = v);
            infiniteOxygenToggle.onValueChanged.AddListener(v => infiniteOxygenOn = v);
            addMoneyButton.onClick.AddListener(GiveMoney);
            skipStepButton.onClick.AddListener(SkipStep);

            if (devItemButtons != null)
            {
                foreach (DevItemButton entry in devItemButtons)
                {
                    if (entry.button == null || entry.item == null)
                        continue;

                    DevItemButton captured = entry; // avoid closure-over-loop-variable bug
                    captured.button.onClick.AddListener(() => GiveItem(captured.item, captured.amount));
                }
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f10Key.wasPressedThisFrame)
                TogglePanel();

            if (!godModeOn && !infiniteOxygenOn) return;

            Player player = GameManager.CurPlayer;
            if (player == null) return;

            if (godModeOn)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                // ใช้ Heal() ที่มีอยู่แล้ว — เติมเลือด/เชือกให้เต็มทุกเฟรมแทนการรับดาเมจจริง
                health?.Heal(godModeHealAmount);
            }

            if (infiniteOxygenOn)
            {
                OxygenRope oxygenTube = player.OxygenTube;
                if (oxygenTube != null)
                    oxygenTube.MaxRopeLength = infiniteRopeLength;
            }
        }

        private void TogglePanel()
        {
            panelRoot.SetActive(!panelRoot.activeSelf);
        }

        private void GiveMoney()
        {
            PlayerMoney money = GameManager.CurPlayer?.GetComponent<PlayerMoney>();
            if (money != null)
                money.AddMoney(addMoneyAmount);
            else
                Debug.LogWarning("[DevPanel] ไม่พบ PlayerMoney บน CurPlayer");
        }

        private void SkipStep()
        {
            if (QuestManager.Instance == null)
            {
                Debug.LogWarning("[DevPanel] ไม่พบ QuestManager.Instance");
                return;
            }

            QuestManager.Instance.DevSkipCurrentStep();
        }

        private void GiveItem(Item item, int amount)
        {
            if (item == null)
                return;

            if (InventoryManager.Instance == null)
            {
                Debug.LogWarning("[DevPanel] ไม่พบ InventoryManager.Instance");
                return;
            }

            // AddItem() เดิมเพิ่มทีละ 1 ชิ้น — เรียกซ้ำตามจำนวนที่ตั้งไว้
            // (มันจัดการ full-inventory notification และ NotifyChanged() ให้เองอยู่แล้วในทุกครั้งที่เรียก)
            int toGive = Mathf.Max(1, amount);
            for (int i = 0; i < toGive; i++)
                InventoryManager.Instance.AddItem(item);
        }
    }
}
