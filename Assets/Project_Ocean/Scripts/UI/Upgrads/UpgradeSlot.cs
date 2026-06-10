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

        public Color activeColor = Color.green;
        public Color inactiveColor = Color.gray;

        private void Start()
        {
            RefreshUI();
        }

        public void Upgrade()
        {
            UpgradeManager.Instance.BuyUpgrade(data);

            RefreshUI();
        }

        public void RefreshUI()
        {
            nameText.text = data.upgradeName;

            int level =
                UpgradeManager.Instance.GetLevel(data);

            if (level >= data.maxLevel)
            {
                priceText.text = "MAX";
            }
            else
            {
                priceText.text =
                    "$" + (data.basePrice * (level + 1));
            }

            for (int i = 0; i < levelBoxes.Length; i++)
            {
                levelBoxes[i].color =
                    i < level
                    ? activeColor
                    : inactiveColor;
            }
        }
    }
}