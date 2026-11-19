using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.UI
{
    public class FishCollectionSlot : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image fishImage;
        [SerializeField] private Image lockImage;
        [SerializeField] private TMP_Text fishName;
        [SerializeField] private TMP_Text fishSize;

        [Header("Unknown Fish")]
        [SerializeField] private Sprite unknownFishSprite;
        [SerializeField] private string unknownName = "???";
        [SerializeField] private string unknownSize = "???";

        /// <summary>
        /// Setup this slot using an Item from the Fish Collection catalog.
        /// </summary>
        public void Setup(Item fishItem, bool isCollected)
        {
            if (fishItem == null)
            {
                Clear();
                return;
            }

            if (isCollected)
            {
                ShowCollected(fishItem);
            }
            else
            {
                ShowUnknown();
            }
        }

        private void ShowCollected(Item fishItem)
        {
            if (fishImage != null)
            {
                fishImage.sprite = fishItem.ItemVisual;
                fishImage.enabled = fishItem.ItemVisual != null;
            }

            if (fishName != null)
                fishName.text = fishItem.ItemName;

            if (fishSize != null)
                fishSize.text = GetFishSizeText(fishItem);

            if (lockImage != null)
                lockImage.gameObject.SetActive(false);
        }

        private void ShowUnknown()
        {
            if (fishImage != null)
            {
                if (unknownFishSprite != null)
                {
                    fishImage.sprite = unknownFishSprite;
                    fishImage.enabled = true;
                }
                else
                {
                    fishImage.sprite = null;
                    fishImage.enabled = false;
                }
            }

            if (fishName != null)
                fishName.text = unknownName;

            if (fishSize != null)
                fishSize.text = unknownSize;

            if (lockImage != null)
                lockImage.gameObject.SetActive(true);
        }

        private string GetFishSizeText(Item fishItem)
        {
            if (fishItem == null)
                return "";

            return fishItem.FishSize switch
            {
                FishSize.Small => "Small",
                FishSize.Medium => "Medium",
                FishSize.Large => "Large",
                _ => ""
            };
        }

        public void Clear()
        {
            if (fishImage != null)
            {
                fishImage.sprite = null;
                fishImage.enabled = false;
            }

            if (lockImage != null)
                lockImage.gameObject.SetActive(true);

            if (fishName != null)
                fishName.text = unknownName;

            if (fishSize != null)
                fishSize.text = unknownSize;
        }
    }
}