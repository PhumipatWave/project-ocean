using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Testing
{
    public class UpgradeSlot :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        public UpgradeData data;

        public TMP_Text nameText;
        public TMP_Text priceText;

        public Image[] levelBoxes;

        public Color activeColor = Color.green;
        public Color inactiveColor = Color.gray;

        [SerializeField] private UpgradeManager upgradeManager;

        private void Start()
        {
            RefreshUI();
        }

        public void Upgrade()
        {
            if (upgradeManager == null || data == null)
                return;

            int level = upgradeManager.GetLevel(data);

            if (level >= data.maxLevel)
                return;

            int price = upgradeManager.GetPrice(data);

            ConfirmationDialog.Instance?.Show(
                $"Upgrade {data.upgradeName} for ${price}?",
                () =>
                {
                    upgradeManager.BuyUpgrade(data);
                    RefreshUI();
                }
            );
        }

        public void RefreshUI()
        {
            if (data == null)
                return;

            nameText.text = data.upgradeName;

            int level = upgradeManager != null
                ? upgradeManager.GetLevel(data)
                : 0;

            for (int i = 0; i < levelBoxes.Length; i++)
            {
                levelBoxes[i].color =
                    i < level
                        ? activeColor
                        : inactiveColor;
            }

            if (level >= data.maxLevel)
            {
                priceText.text = "MAX";
            }
            else
            {
                priceText.text =
                    "$" + upgradeManager.GetPrice(data);
            }
        }

        public string GetCurrentPriceText()
        {
            if (data == null ||
                upgradeManager == null)
                return "";

            int level =
                upgradeManager.GetLevel(data);

            if (level >= data.maxLevel)
                return "MAX";

            return "$" +
                   upgradeManager.GetPrice(data);
        }

        // =========================================================
        // HOVER TOOLTIP
        // =========================================================

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            if (upgradeManager == null ||
                data == null)
                return;

            RectTransform target =
                GetComponent<RectTransform>();

            UpgradeTooltip.Instance?.Show(
                data,
                upgradeManager,
                target);
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            UpgradeTooltip.Instance?.Hide();
        }
    }
}