using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    /// <summary>
    /// Dev/Cheat Panel แบบไฟล์เดียว — ไม่แก้ PlayerHealth, OxygenRope, PlayerMoney, GameManager เลย
    /// ทำงานผ่าน public API ที่มีอยู่แล้วของทุกคลาสเท่านั้น
    ///
    /// God Mode         -> เรียก PlayerHealth.Heal() เติมเต็มทุกเฟรม
    /// Infinite Oxygen   -> ตั้ง OxygenRope.MaxRopeLength ให้สูงมากทุกเฟรม กัน breach/นับถอยหลัง
    /// Add Money         -> เรียก PlayerMoney.AddMoney() ตรงๆ
    /// </summary>
    public class DevPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;   // กล่อง panel ทั้งหมด (ซ่อน/โชว์)
        [SerializeField] private Button toggleButton;    // ไอคอนมุมจอ กดเปิด-ปิด

        [Header("Cheats")]
        [SerializeField] private Toggle godModeToggle;
        [SerializeField] private Toggle infiniteOxygenToggle;
        [SerializeField] private Button addMoneyButton;
        [SerializeField] private int addMoneyAmount = 9999;

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
        }

        private void Update()
        {
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
    }
}
