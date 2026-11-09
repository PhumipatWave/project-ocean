using UnityEngine;
using UnityEngine.Events;

namespace Game.Gameplay
{
    /// <summary>Interact zone that fires onInteract (wired in the Inspector) when the player presses E.</summary>
    public class EventInteractable : Interactable
    {
        [Tooltip("Fired when the player presses E while inside this zone.")]
        public UnityEvent onInteract;

        protected override void Interact(Player player) => onInteract?.Invoke();
    }
}
