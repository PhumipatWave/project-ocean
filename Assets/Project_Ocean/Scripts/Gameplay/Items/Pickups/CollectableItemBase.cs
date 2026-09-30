using Game.Core;

namespace Game.Gameplay
{
    /// <summary>A world pickup: Interact collects it; no UI lock and no tutorial gating.</summary>
    public abstract class CollectableItemBase : Interactable
    {
        /// External components (e.g. ShellPickup) can set this to false to block collection.
        public bool IsCollectable { get; set; } = true;

        protected override bool TakesPlayerLock => false;

        protected override bool IsFeatureUnlocked => FeatureUnlockService.Instance.IsUnlocked(GameFeature.InteractItem);

        public override bool CanInteract => IsCollectable;

        protected override void Interact(Player player) => TryCollect();

        public abstract void TryCollect();
    }
}
