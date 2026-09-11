using Game.Testing;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Scale")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float speed = 10f;

    [Header("Button Text")]
    [SerializeField] private TMP_Text buttonText;

    [Header("Text")]
    [SerializeField] private string hoverText = "Upgrade";

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * speed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // ขยายปุ่ม
        targetScale = originalScale * hoverScale;

        // เปลี่ยนข้อความเป็น Upgrade
        if (buttonText != null)
        {
            buttonText.text = hoverText;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // กลับขนาดเดิม
        targetScale = originalScale;

        // กลับไปแสดงราคา
        RefreshPrice();
    }

    public void RefreshPrice()
    {
        if (buttonText == null)
            return;

        UpgradeSlot upgradeSlot = GetComponentInParent<UpgradeSlot>();

        if (upgradeSlot == null)
            return;

        buttonText.text = upgradeSlot.GetCurrentPriceText();
    }
}