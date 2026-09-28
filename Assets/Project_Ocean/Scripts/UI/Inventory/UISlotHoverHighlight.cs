using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
public class UISlotHoverHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image targetImage;

    public Color hoverColor = Color.yellow;
    public Color normalColor = Color.white;

    private void Start()
    {
        if (targetImage != null)
            targetImage.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetImage != null)
            targetImage.color = normalColor;
    }
}
}