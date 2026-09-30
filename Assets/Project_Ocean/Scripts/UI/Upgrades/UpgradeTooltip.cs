using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class UpgradeTooltip : MonoBehaviour
    {
        public static UpgradeTooltip Instance;

        [Header("Main")]
        [SerializeField] private GameObject tooltip;

        [Header("Texts")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private TMP_Text priceText;

        [Header("Gun Stats")]
        [Tooltip("NormalBullet prefab — the gun tooltip reads its range per level. Unassigned = damage only.")]
        [SerializeField] private NormalBullet normalBulletPrefab;

        [Header("Unlock")]
        [SerializeField] private Transform unlockSection;
        [SerializeField] private GameObject unlockRowTemplate;

        [Header("Position")]
        [SerializeField] private float spacing = 12f;

        private readonly List<GameObject> spawnedRows = new();

        private Canvas canvas;
        private RectTransform targetRect;

        private void Awake()
        {
            Instance = this;

            canvas = GetComponentInParent<Canvas>();

            if (tooltip != null)
            {
                tooltip.SetActive(false);

                RectTransform tooltipRect =
                    tooltip.GetComponent<RectTransform>();

                if (tooltipRect != null)
                {
                    Vector2 originalSize =
                        tooltipRect.rect.size;

                    tooltipRect.anchorMin =
                        new Vector2(0.5f, 0.5f);

                    tooltipRect.anchorMax =
                        new Vector2(0.5f, 0.5f);

                    tooltipRect.pivot =
                        new Vector2(0f, 1f);

                    // Changing anchors resets sizeDelta; restore the size authored in the scene.
                    tooltipRect.sizeDelta =
                        originalSize;
                }
            }

            // The tooltip must never take clicks meant for the upgrade button under it.
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            if (unlockRowTemplate != null)
                unlockRowTemplate.SetActive(false);
        }

        private void Update()
        {
            if (tooltip == null || !tooltip.activeSelf)
                return;

            if (targetRect != null)
                FollowTarget();
        }

        public void Show(
            UpgradeData data,
            UpgradeManager manager,
            RectTransform target)
        {
            if (data == null ||
                manager == null ||
                target == null ||
                tooltip == null)
                return;

            targetRect = target;

            int currentLevel = manager.GetLevel(data);
            int price = manager.GetPrice(data);


            if (titleText != null)
                titleText.text = data.upgradeName;


            if (descriptionText != null)
                descriptionText.text = data.description;


            if (levelText != null)
            {
                if (currentLevel >= data.maxLevel)
                {
                    levelText.text = $"Level {currentLevel} / MAX";
                }
                else
                {
                    levelText.text =
                        $"Level {currentLevel} → {currentLevel + 1}";
                }
            }


            UpdateValue(data, currentLevel);


            if (priceText != null)
            {
                if (currentLevel >= data.maxLevel)
                    priceText.text = "MAX";
                else
                    priceText.text = $"Cost: ${price}";
            }


            UpdateUnlocks(data, currentLevel);

            tooltip.SetActive(true);

            // Layout must size the tooltip before it is positioned.
            Canvas.ForceUpdateCanvases();

            FollowTarget();
        }

        private void UpdateValue(UpgradeData data,int currentLevel)
        {
            if (valueText == null)
                return;

            if (currentLevel >= data.maxLevel)
            {
                valueText.text = "MAX";
                return;
            }

            switch (data.type)
            {
                case UpgradeType.Inventory:

                    valueText.text = "+1 Slot";
                    break;

                case UpgradeType.Rope:

                    valueText.text =
                        $"+{data.value:0} meter";
                    break;

                case UpgradeType.NormalBullet:

                    // Gun gains aren't flat per level, so read NormalBullet's tables, not data.value.
                    valueText.text =
                        $"Damage {NormalBullet.GetDamage(currentLevel):0} > {NormalBullet.GetDamage(currentLevel + 1):0}";

                    if (normalBulletPrefab != null)
                        valueText.text +=
                            $"\nRange {normalBulletPrefab.RangeAt(currentLevel):0.#}m > {normalBulletPrefab.RangeAt(currentLevel + 1):0.#}m";
                    break;

                default:

                    valueText.text =
                        $"+{data.value:0}";
                    break;
            }
        }

        private void UpdateUnlocks(
            UpgradeData data,
            int currentLevel)
        {
            ClearUnlockRows();

            if (unlockSection == null)
                return;

            if (data.unlocks == null ||
                data.unlocks.Count == 0)
                return;

            foreach (UpgradeUnlock unlock in data.unlocks)
            {
                if (unlock == null ||
                    unlockRowTemplate == null)
                    continue;

                GameObject row = Instantiate(
                    unlockRowTemplate,
                    unlockSection);

                row.SetActive(true);

                spawnedRows.Add(row);

                TMP_Text text =
                    row.GetComponentInChildren<TMP_Text>();

                if (text == null)
                    continue;

                if (currentLevel >= unlock.unlockLevel)
                {
                    text.text =
                        $"Unlock {unlock.unlockName}";
                }
                else
                {
                    text.text =
                        $"Unlock {unlock.unlockName} • Level {unlock.unlockLevel}";
                }
            }
        }

        private void ClearUnlockRows()
        {
            for (int i = 0; i < spawnedRows.Count; i++)
            {
                if (spawnedRows[i] != null)
                    Destroy(spawnedRows[i]);
            }

            spawnedRows.Clear();
        }

        public void Hide()
        {
            targetRect = null;

            if (tooltip != null)
                tooltip.SetActive(false);

            ClearUnlockRows();
        }


        private void FollowTarget()
        {
            if (tooltip == null ||
                canvas == null ||
                targetRect == null)
                return;

            RectTransform tooltipRect =
                tooltip.GetComponent<RectTransform>();

            RectTransform canvasRect =
                canvas.GetComponent<RectTransform>();

            if (tooltipRect == null ||
                canvasRect == null)
                return;

            Camera cam =
                canvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : canvas.worldCamera;


            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);

            Vector2 bottomLeft =
                RectTransformUtility.WorldToScreenPoint(
                    cam,
                    corners[0]);

            Vector2 topLeft =
                RectTransformUtility.WorldToScreenPoint(
                    cam,
                    corners[1]);

            Vector2 topRight =
                RectTransformUtility.WorldToScreenPoint(
                    cam,
                    corners[2]);

            Vector2 bottomRight =
                RectTransformUtility.WorldToScreenPoint(
                    cam,
                    corners[3]);

            float slotCenterY =
                (topLeft.y + bottomLeft.y) * 0.5f;


            // rect.size is in canvas units but positions below are screen pixels; scale it, or the tooltip is wrong
            // at any resolution other than the reference one.
            Vector2 tooltipSize =
                tooltipRect.rect.size * canvas.scaleFactor;

            float spacing = 12f;


            float rightX =
                topRight.x + spacing;

            bool fitRight =
                rightX + tooltipSize.x <=
                Screen.width - 5f;

            float tooltipX;

            if (fitRight)
            {
                tooltipX = rightX;
            }
            else
            {
                tooltipX =
                    topLeft.x -
                    tooltipSize.x -
                    spacing;
            }


            tooltipX =
                Mathf.Clamp(
                    tooltipX,
                    5f,
                    Screen.width -
                    tooltipSize.x -
                    5f);


            float tooltipTop =
                slotCenterY +
                tooltipSize.y * 0.5f;


            if (tooltipTop >
                Screen.height - 5f)
            {
                tooltipTop =
                    Screen.height - 5f;
            }


            if (tooltipTop - tooltipSize.y < 5f)
            {
                tooltipTop =
                    tooltipSize.y + 5f;
            }


            Vector2 screenPosition =
                new Vector2(
                    tooltipX,
                    tooltipTop);

            Vector2 localPosition;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    cam,
                    out localPosition))
            {
                tooltipRect.anchoredPosition =
                    localPosition;
            }
        }
    }
}