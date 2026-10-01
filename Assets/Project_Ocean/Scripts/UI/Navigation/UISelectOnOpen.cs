using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>Gives a panel a selected button when it opens, so a gamepad can navigate it.</summary>
    public class UISelectOnOpen : MonoBehaviour
    {
        // Open panels, newest last; only the top one keeps the selection alive.
        private static readonly List<UISelectOnOpen> open = new List<UISelectOnOpen>();

        [Tooltip("Left empty: the first interactable Selectable in this panel.")]
        [SerializeField] private Selectable firstSelected;

        [Tooltip("Pressed on Cancel (Esc / gamepad East) for panels UIManager doesn't close itself.")]
        [SerializeField] private Button cancelButton;

        private GameObject previous;
        private GameObject lastSelectedHere;

        private void OnEnable()
        {
            previous = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            open.Add(this);
            StartCoroutine(SelectNextFrame());
        }

        private void OnDisable()
        {
            open.Remove(this);
            var es = EventSystem.current;
            if (es == null) return;

            // Hand the selection back to the panel underneath (e.g. closing a confirm dialog).
            GameObject current = es.currentSelectedGameObject;
            if (current != null && !current.transform.IsChildOf(transform)) return;
            es.SetSelectedGameObject(previous != null && previous.activeInHierarchy ? previous : null);
        }

        // Child buttons can still be inactive on the frame the panel opens.
        private IEnumerator SelectNextFrame()
        {
            yield return null;
            Select(firstSelected);
        }

        private void Update()
        {
            if (open.Count == 0 || open[open.Count - 1] != this) return;

            var es = EventSystem.current;
            if (es == null) return;

            if (cancelButton != null && cancelButton.IsInteractable() && !UIManagerHandlesBack() && WasPressed(es, m => m.cancel))
            {
                cancelButton.onClick.Invoke();
                return;
            }

            GameObject current = es.currentSelectedGameObject;
            bool alive = current != null && current.activeInHierarchy;
            if (alive && current.transform.IsChildOf(transform))
            {
                lastSelectedHere = current;
            }
            else if (alive)
            {
                // Automatic navigation can step out of the panel (e.g. onto world-space interact buttons).
                Select(lastSelectedHere != null ? lastSelectedHere.GetComponent<Selectable>() : firstSelected);
                return;
            }
            else if (WasPressed(es, m => m.move) || WasPressed(es, m => m.submit))
            {
                // Selection was lost to a mouse click or a disabled button; restore it on gamepad input.
                Select(lastSelectedHere != null ? lastSelectedHere.GetComponent<Selectable>() : firstSelected);
                return;
            }

            // Mouse users get hover highlights; a stale gamepad highlight would look like a second cursor.
            if (current != null && WasPressed(es, m => m.point)) es.SetSelectedGameObject(null);
        }

        // UIManager already closes its windows and PlayerUIState panels on Cancel; pressing here too would close two.
        private static bool UIManagerHandlesBack() =>
            UIManager.Instance != null && (UIManager.Instance.IsUIOpen || Game.Core.PlayerUIState.IsLocked);

        private void Select(Selectable preferred)
        {
            var es = EventSystem.current;
            if (es == null || !isActiveAndEnabled) return;

            Selectable target = IsUsable(preferred) ? preferred : FirstUsable();
            if (target != null) es.SetSelectedGameObject(target.gameObject);
        }

        private Selectable FirstUsable()
        {
            foreach (Selectable s in GetComponentsInChildren<Selectable>())
                if (IsUsable(s)) return s;
            return null;
        }

        private static bool IsUsable(Selectable s) =>
            s != null && s.isActiveAndEnabled && s.IsInteractable() && s.navigation.mode != Navigation.Mode.None;

        private static bool WasPressed(EventSystem es, System.Func<InputSystemUIInputModule, UnityEngine.InputSystem.InputActionReference> pick)
        {
            var module = es.currentInputModule as InputSystemUIInputModule;
            var action = module != null ? pick(module)?.action : null;
            return action != null && action.WasPerformedThisFrame();
        }
    }
}
