using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.Testing
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
                    // จำขนาดจริงของ Tooltip จาก Scene เอาไว้
                    Vector2 originalSize =
                        tooltipRect.rect.size;

                    tooltipRect.anchorMin =
                        new Vector2(0.5f, 0.5f);

                    tooltipRect.anchorMax =
                        new Vector2(0.5f, 0.5f);

                    tooltipRect.pivot =
                        new Vector2(0f, 1f);

                    // สำคัญมาก!
                    tooltipRect.sizeDelta =
                        originalSize;
                }
            }

            // Tooltip ห้ามบังปุ่ม Upgrade
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

            // ─────────────────────────
            // Title
            // ─────────────────────────

            if (titleText != null)
                titleText.text = data.upgradeName;

            // ─────────────────────────
            // Description
            // ─────────────────────────

            if (descriptionText != null)
                descriptionText.text = data.description;

            // ─────────────────────────
            // Level
            // ─────────────────────────

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

            // ─────────────────────────
            // Value
            // ─────────────────────────

            UpdateValue(data, currentLevel);

            // ─────────────────────────
            // Price
            // ─────────────────────────

            if (priceText != null)
            {
                if (currentLevel >= data.maxLevel)
                    priceText.text = "MAX";
                else
                    priceText.text = $"Cost: ${price}";
            }

            // ─────────────────────────
            // Unlocks
            // ─────────────────────────

            UpdateUnlocks(data, currentLevel);

            tooltip.SetActive(true);

            // รอให้ Layout คำนวณขนาดก่อนจัดตำแหน่ง
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

                    // Upgrade 1 ครั้ง = +1 Slot
                    valueText.text = "+1 Slot";
                    break;

                case UpgradeType.Rope:

                    // ความยาวเชือก
                    valueText.text =
                        $"+{data.value:0} meter";
                    break;

                case UpgradeType.NormalBullet:

                    // ความเสียหายกระสุน
                    valueText.text =
                        $"+{data.value:0} Damage";
                    break;

                default:

                    // รองรับ Upgrade ใหม่ในอนาคต
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

        // =========================================================
        // Tooltip อยู่ข้าง ๆ UpgradeSlot
        // =========================================================

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

            // =====================================================
            // UpgradeSlot → Screen Position
            // =====================================================

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

            // =====================================================
            // Tooltip ขนาดจริง
            // =====================================================

            // rect.size is in canvas units, but every position below is in screen pixels
            // (Screen.width, WorldToScreenPoint). With ScaleWithScreenSize the two only match
            // when the screen is exactly the reference resolution — on a smaller Game view the
            // tooltip looked wider than the screen and got clamped to the top-left corner.
            Vector2 tooltipSize =
                tooltipRect.rect.size * canvas.scaleFactor;

            float spacing = 12f;

            // =====================================================
            // วางขวาก่อน
            // =====================================================

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
                // ถ้าขวาไม่มีที่ → ซ้าย
                tooltipX =
                    topLeft.x -
                    tooltipSize.x -
                    spacing;
            }

            // =====================================================
            // กันหลุดซ้าย / ขวา
            // =====================================================

            tooltipX =
                Mathf.Clamp(
                    tooltipX,
                    5f,
                    Screen.width -
                    tooltipSize.x -
                    5f);

            // =====================================================
            // จัดกึ่งกลางกับ UpgradeSlot
            // =====================================================

            float tooltipTop =
                slotCenterY +
                tooltipSize.y * 0.5f;

            // =====================================================
            // กันหลุดบน
            // =====================================================

            if (tooltipTop >
                Screen.height - 5f)
            {
                tooltipTop =
                    Screen.height - 5f;
            }

            // =====================================================
            // กันหลุดล่าง
            // =====================================================

            if (tooltipTop - tooltipSize.y < 5f)
            {
                tooltipTop =
                    tooltipSize.y + 5f;
            }

            // =====================================================
            // Screen → Canvas
            // =====================================================

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