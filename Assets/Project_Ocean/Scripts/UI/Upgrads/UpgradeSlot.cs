using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class UpgradeSlot : MonoBehaviour
    {
        public UpgradeData data;

        public TMP_Text nameText;
        public TMP_Text priceText;

        public Image[] levelBoxes;

        public Color activeColor   = Color.green;
        public Color inactiveColor = Color.gray;

        [SerializeField] private UpgradeManager upgradeManager;

        private void Start()
        {
            RefreshUI();
        }

        public void Upgrade()
        {
            if (upgradeManager == null) return;
            upgradeManager.BuyUpgrade(data);
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (data == null) return;

            nameText.text = data.upgradeName;

            int level = upgradeManager != null ? upgradeManager.GetLevel(data) : 0;

            priceText.text = level >= data.maxLevel
                ? "MAX"
                : "$" + (data.basePrice * (level + 1));

            for (int i = 0; i < levelBoxes.Length; i++)
                levelBoxes[i].color = i < level ? activeColor : inactiveColor;
        }
    }
}
