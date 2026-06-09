using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Game.Testing
{
    public class RopeUpgrade : MonoBehaviour
    {
        [Header("Upgrade")]
        public int level = 1;
        public int maxLevel = 5;
        public int basePrice = 100; 

        [Header("Rope")]
        public float lengthIncrease = 20f; 

        [Header("UI")]
        public TMP_Text priceText;
        public TMP_Text lengthText;

        [SerializeField] private Image[] levelBoxes;

        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color inactiveColor = Color.gray;

        private void Start()
        {
            RefreshLevelUI();
        }

        void Update()
        {
            if (level >= maxLevel)
            {
                priceText.text = "MAX";
            }
            else
            {
                priceText.text = "$" + GetPrice();
            }

            lengthText.text = "Length: " + ropeMax();
        }

        public void UpgradeRope()
        {
            if (level >= maxLevel)
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

            GameManager.CurPlayer.GetComponent<PlayerOxygenTube>().IncreaseMaxLength(lengthIncrease);
            level++;
            RefreshLevelUI();

            Debug.Log("Upgraded Rope!!");
        }

        int GetPrice()
        {
            return basePrice * level;
        }

        float ropeMax()
        {
            return GameManager.CurPlayer.GetComponent<PlayerOxygenTube>().GetMaxLength();
        }

        private void RefreshLevelUI()
        {
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
