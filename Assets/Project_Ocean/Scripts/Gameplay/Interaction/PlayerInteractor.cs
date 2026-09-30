using System.Collections.Generic;
using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>The only listener for the Interact press: sends it to the lock holder or the closest ready interactable.</summary>
    [RequireComponent(typeof(Player))]
    public class PlayerInteractor : MonoBehaviour
    {
        private readonly List<Interactable> inRange = new();
        private Player player;
        private Interactable focus;

        private void Awake() => player = GetComponent<Player>();

        private void OnEnable()
        {
            if (player.input != null) player.input.OnInteracted += HandleInteract;
        }

        private void OnDisable()
        {
            if (player.input != null) player.input.OnInteracted -= HandleInteract;
        }

        internal void Add(Interactable target)
        {
            if (!inRange.Contains(target)) inRange.Add(target);
        }

        internal void Remove(Interactable target)
        {
            inRange.Remove(target);
            if (focus == target) focus = null;
        }

        private static bool Blocked =>
            CutsceneGate.IsPlaying || LoadingManager.IsLoading || DialogueState.BlocksInteraction;

        private Interactable PickTarget(bool includeLocked)
        {
            foreach (Interactable i in inRange)
                if (i.HoldsLock) return i;

            return InteractionTargeting.PickClosest((Vector2)transform.position, inRange,
                i => (Vector2)i.transform.position,
                i => includeLocked ? i.CanInteract : i.IsReady);
        }

        private void HandleInteract(bool pressed)
        {
            if (!pressed || Blocked) return;

            // A feature-locked target still gets the press when nothing ready is in range, so it can show the locked toast.
            (PickTarget(false) ?? PickTarget(true))?.Press(player);
        }

        private void Update()
        {
            Interactable next = Blocked ? null : PickTarget(false);
            if (next == focus) return;

            if (focus != null) focus.SetIconVisible(false);
            focus = next;
            if (focus != null) focus.SetIconVisible(true);
        }
    }
}
