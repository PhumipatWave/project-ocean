using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Dev/Cheat Panel — ทำงานผ่าน public API ของ PlayerHealth, OxygenRope, PlayerMoney, GameManager,
    /// InventoryManager
    ///
    /// God Mode         -> เรียก PlayerHealth.Heal() เติมเต็มทุกเฟรม
    /// Infinite Oxygen   -> ตั้ง OxygenRope.IgnoreRopeLimit ทุกเฟรม ข้าม clamp ระยะ + breach/นับถอยหลัง (ไม่แตะ MaxRopeLength จึงไม่เซฟค่าผิด)
    /// Add Money         -> เรียก PlayerMoney.AddMoney() ตรงๆ
    /// Add Item          -> เรียก Inventory.Current.AddItem(item) ตรงๆ ต่อปุ่มที่กด
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

        [Header("Spawn Any Item")]
        [Tooltip("Filled at runtime from InventoryManager.AllItems.")]
        [SerializeField] private TMPro.TMP_Dropdown itemDropdown;
        [SerializeField] private Button spawnItemButton;
        [SerializeField] private int spawnAmount = 1;

        private readonly System.Collections.Generic.List<Item> dropdownItems = new();

        [Header("Upgrade (free, +1 level per click)")]
        [Tooltip("One entry per upgrade type — drag the UpgradeData assets (Slot / Rope / Gun). Order = dropdown order.")]
        [SerializeField] private UpgradeData[] upgrades;
        [SerializeField] private TMPro.TMP_Dropdown upgradeDropdown;
        [SerializeField] private Button upgradeButton;

        [Header("Settings")]
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

            if (spawnItemButton != null)
                spawnItemButton.onClick.AddListener(SpawnSelectedItem);

            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(UpgradeOneStep);

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

            Player player = GameManager.CurPlayer;
            if (player == null) return;

            // Set every frame (also when off) so switching the toggle off restores the limit.
            OxygenRope oxygenTube = player.OxygenTube;
            if (oxygenTube != null)
                oxygenTube.IgnoreRopeLimit = infiniteOxygenOn;

            if (godModeOn)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                // ใช้ Heal() ที่มีอยู่แล้ว — เติมเลือด/เชือกให้เต็มทุกเฟรมแทนการรับดาเมจจริง
                health?.Heal(godModeHealAmount);
            }
        }

        private void TogglePanel()
        {
            panelRoot.SetActive(!panelRoot.activeSelf);

            if (panelRoot.activeSelf && dropdownItems.Count == 0)
                FillItemDropdown();

            if (panelRoot.activeSelf)
                RefreshUpgradeDropdown(); // levels may have changed since it was last open (real purchases, load)
        }

        // Option text = "<name>  Lv current/max", so the current level is always visible in the dropdown.
        private void RefreshUpgradeDropdown()
        {
            if (upgradeDropdown == null || upgrades == null || upgrades.Length == 0)
                return;

            int selected = upgradeDropdown.value;
            var options = new System.Collections.Generic.List<string>();
            foreach (UpgradeData u in upgrades)
                options.Add(u == null
                    ? "(missing UpgradeData)"
                    : $"{u.upgradeName}  Lv {UpgradeService.Instance.GetLevel(u.type)}/{u.maxLevel}");

            upgradeDropdown.ClearOptions();
            upgradeDropdown.AddOptions(options);
            upgradeDropdown.SetValueWithoutNotify(Mathf.Clamp(selected, 0, options.Count - 1)); // keep the selection
            upgradeDropdown.RefreshShownValue();
        }

        private void UpgradeOneStep()
        {
            int index = upgradeDropdown != null ? upgradeDropdown.value : -1;
            if (upgrades == null || index < 0 || index >= upgrades.Length)
                return;

            UpgradeData data = upgrades[index];
            if (!UpgradeService.Instance.DevGrantNextLevel(data, out string reason))
                Debug.LogWarning($"[DevPanel] Upgrade '{(data != null ? data.upgradeName : "?")}' not applied: {reason}");

            RefreshUpgradeDropdown();
        }

        // Filled on open, not Start: InventoryManager may not exist yet when this panel starts.
        private void FillItemDropdown()
        {
            if (itemDropdown == null || Inventory.Current?.AllItems == null)
                return;

            dropdownItems.Clear();
            var options = new System.Collections.Generic.List<string>();
            foreach (Item item in Inventory.Current.AllItems)
            {
                if (item == null) continue;
                dropdownItems.Add(item);
                options.Add($"{item.ItemID} - {item.ItemName}");
            }

            itemDropdown.ClearOptions();
            itemDropdown.AddOptions(options);
        }

        private void SpawnSelectedItem()
        {
            if (dropdownItems.Count == 0)
                FillItemDropdown();

            int index = itemDropdown != null ? itemDropdown.value : -1;
            if (index < 0 || index >= dropdownItems.Count)
                return;

            GiveItem(dropdownItems[index], spawnAmount);
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

            if (Inventory.Current == null)
            {
                Debug.LogWarning("[DevPanel] ไม่พบ Inventory.Current");
                return;
            }

            // AddItem() เดิมเพิ่มทีละ 1 ชิ้น — เรียกซ้ำตามจำนวนที่ตั้งไว้
            // (มันจัดการ full-inventory notification และ NotifyChanged() ให้เองอยู่แล้วในทุกครั้งที่เรียก)
            int toGive = Mathf.Max(1, amount);
            for (int i = 0; i < toGive; i++)
                Inventory.Current.AddItem(item);
        }
    }
}
