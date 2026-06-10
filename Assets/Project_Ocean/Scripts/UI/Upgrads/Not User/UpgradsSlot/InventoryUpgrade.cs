using UnityEngine;
using TMPro;
using UnityEngine.UI;


namespace Game.Testing
{
    public class InventoryUpgrade : MonoBehaviour
    {
        [Header("UI")]
        public TMP_Text priceText;

        [Header("Upgrade Setting")]
        public int currentLevel = 1;
        public int maxLevel = 5;
        public int basePrice = 20;
        [SerializeField] private Image[] levelBoxes;

        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color inactiveColor = Color.gray;

        [Header("Slot")]
        public GameObject slotPrefab;
        public Transform slotParent;

        private void Start()
        {
            RefreshLevelUI();
        }

        void Update()
        {
            if (currentLevel >= maxLevel)
            {
                priceText.text = "MAX";
            }
            else
            {
                priceText.text = "$" + GetPrice();
            }

        } // Update ===

        public void UpgradeSlot()
        {
            if (currentLevel >= maxLevel)
            {
                Debug.Log("MAX LEVEL");
                return;
            }

            int price = GetPrice();

            if (!GameManager.CurPlayer.GetComponent<PlayerMoney>().Use(price))
            {
                Debug.Log("No Money to Upgrade!!");
                return;
            }

            InventoryManager.Instance.UnlockNextSlot();
            currentLevel++;
            RefreshLevelUI();

            Debug.Log("Unlocked Slot !!!");

        } // UpgradeSlot ===

        int GetPrice()
        {
            return basePrice * currentLevel;

        } // GetPrice ===

        private void RefreshLevelUI()
        {
            for (int i = 0; i < levelBoxes.Length; i++)
            {
                levelBoxes[i].color =
                    i < currentLevel
                    ? activeColor
                    : inactiveColor;
            }
        }

    }
}
