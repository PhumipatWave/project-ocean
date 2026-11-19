using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>Base for quest markers: refreshes on quest updates (and optionally every frame) and shows or hides itself.</summary>
    public abstract class QuestMarkerBase : MonoBehaviour
    {
        [SerializeField] protected GameObject markerUI;

        // Proximity markers must re-check as the player walks; quest-only markers don't.
        protected virtual bool PollEveryFrame => true;

        protected virtual void OnEnable()
        {
            QuestManager.OnQuestUpdated += Refresh;
            Refresh();
        }

        protected virtual void OnDisable() => QuestManager.OnQuestUpdated -= Refresh;

        private void Update()
        {
            if (PollEveryFrame)
                Refresh();
        }

        protected void Refresh() => SetVisible(ShouldShow());

        protected abstract bool ShouldShow();

        protected virtual void SetVisible(bool visible)
        {
            if (markerUI != null)
                markerUI.SetActive(visible);
        }
    }
}
