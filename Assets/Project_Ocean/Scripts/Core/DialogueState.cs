using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    /// <summary>
    /// Which dialogue boxes are on screen right now. NPC and PlayerSelfDialogueUI report their
    /// panel here; UI that shouldn't stack under a dialogue (see LandUI) listens to OnChanged
    /// and hides itself while IsShowing. Tracks panels, not a counter, so a panel that gets
    /// destroyed mid-dialogue (scene unload) can't leave the state stuck on.
    /// </summary>
    public static class DialogueState
    {
        private static readonly HashSet<GameObject> panels = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            panels.Clear();
            Game.Core.InputReader.IsDialogueShowing = () => IsShowing;
        }

        /// <summary>Fired whenever a dialogue panel starts or stops showing.</summary>
        public static event Action OnChanged;

        public static bool IsShowing
        {
            get
            {
                panels.RemoveWhere(p => p == null);
                return panels.Count > 0;
            }
        }

        public static void Show(GameObject panel)
        {
            if (panel != null && panels.Add(panel))
                OnChanged?.Invoke();
        }

        public static void Hide(GameObject panel)
        {
            if (panel != null && panels.Remove(panel))
                OnChanged?.Invoke();
        }

        private static bool selfDialogueShowing;
        private static int selfDialogueHideFrame = -1;

        /// <summary>True while an auto-display (self) dialogue is up, and for the rest of the frame it
        /// closes on — the press that closed it must not also start a shop/door/pickup.</summary>
        public static bool BlocksInteraction => selfDialogueShowing || Time.frameCount <= selfDialogueHideFrame;

        public static void SetSelfDialogueShowing(bool showing)
        {
            if (!showing)
                selfDialogueHideFrame = Time.frameCount;
            selfDialogueShowing = showing;
        }

        /// <summary>True if <paramref name="root"/> is, or contains, a showing dialogue panel.</summary>
        public static bool ContainsShowingPanel(Transform root)
        {
            foreach (var panel in panels)
                if (panel != null && panel.transform.IsChildOf(root))
                    return true;
            return false;
        }
    }
}
