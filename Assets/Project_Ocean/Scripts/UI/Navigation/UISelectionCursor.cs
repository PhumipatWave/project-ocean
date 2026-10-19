using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>Draws a frame around the selected UI element so gamepad players can see where they are.</summary>
    public class UISelectionCursor : MonoBehaviour
    {
        [SerializeField] private Sprite frameSprite;
        [SerializeField] private Color color = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private float padding = 6f;
        [SerializeField] private float moveSpeed = 18f;
        // Above ConfirmDialog (999), below LoadingCanvas (2000) so the frame never draws over a scene load.
        [SerializeField] private int sortingOrder = 1500;

        private RectTransform frame;
        private readonly Vector3[] corners = new Vector3[4];
        private bool wasVisible;

        // Mouse users click what they see; the frame only helps the device that moves a selection.
        private bool gamepadActive;

        private void Awake()
        {
            var canvasGo = new GameObject("SelectionCursorCanvas", typeof(Canvas));
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var frameGo = new GameObject("Frame", typeof(RectTransform), typeof(Image));
            frameGo.transform.SetParent(canvasGo.transform, false);
            var image = frameGo.GetComponent<Image>();
            image.sprite = frameSprite;
            image.type = Image.Type.Sliced;
            image.color = color;
            image.raycastTarget = false;

            frame = (RectTransform)frameGo.transform;
            frame.anchorMin = frame.anchorMax = frame.pivot = Vector2.zero;
            frame.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            UpdateGamepadActive();

            var es = EventSystem.current;
            GameObject selected = es != null ? es.currentSelectedGameObject : null;
            Rect target = default;
            bool visible = gamepadActive && selected != null && selected.activeInHierarchy && TryGetScreenRect(selected, out target);

            if (!visible)
            {
                if (wasVisible) frame.gameObject.SetActive(false);
                wasVisible = false;
                return;
            }

            Vector2 pos = target.min - Vector2.one * padding;
            Vector2 size = target.size + Vector2.one * (padding * 2f);

            if (!wasVisible)
            {
                // Appear on the target instead of sliding in from wherever it was last hidden.
                frame.gameObject.SetActive(true);
                frame.anchoredPosition = pos;
                frame.sizeDelta = size;
            }
            else
            {
                // Unscaled: the pause menu sets timeScale to 0.
                float t = 1f - Mathf.Exp(-moveSpeed * Time.unscaledDeltaTime);
                frame.anchoredPosition = Vector2.Lerp(frame.anchoredPosition, pos, t);
                frame.sizeDelta = Vector2.Lerp(frame.sizeDelta, size, t);
            }
            wasVisible = true;
        }

        private void UpdateGamepadActive()
        {
            Mouse mouse = Mouse.current;
            if (mouse != null && (mouse.delta.ReadValue().sqrMagnitude > 4f || mouse.leftButton.wasPressedThisFrame))
            {
                gamepadActive = false;
                return;
            }

            Gamepad pad = Gamepad.current;
            if (pad == null || gamepadActive)
                return;

            // Not wasUpdatedThisFrame: a DualSense reports every frame even when untouched.
            if (pad.leftStick.ReadValue().sqrMagnitude > 0.25f || pad.rightStick.ReadValue().sqrMagnitude > 0.25f)
            {
                gamepadActive = true;
                return;
            }

            foreach (InputControl control in pad.allControls)
            {
                if (control is ButtonControl button && !button.synthetic && button.wasPressedThisFrame)
                {
                    gamepadActive = true;
                    return;
                }
            }
        }

        private bool TryGetScreenRect(GameObject go, out Rect rect)
        {
            rect = default;
            if (!(go.transform is RectTransform rt)) return false;

            Canvas canvas = rt.GetComponentInParent<Canvas>();
            if (canvas == null) return false;
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            rt.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(cam, corners[2]);
            rect = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            return true;
        }
    }
}
