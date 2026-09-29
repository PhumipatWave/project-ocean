using System.Collections.Generic;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Single source of truth for upgrade levels.
    /// Plain C# (no MonoBehaviour, no scene presence) — reachable from any scene,
    /// whether or not the upgrade panel exists or is active.
    /// UpgradeManager (Land panel UI) and SaveBridge both go through this service; effects are applied by listeners of OnLevelGranted.
    /// </summary>
    public class UpgradeService : ISaveParticipant
    {
        private static UpgradeService instance;
        public static UpgradeService Instance => instance ??= new UpgradeService();

        /// <summary>Raised when one level is granted, before it counts; receivers apply their own effect.</summary>
        public static event System.Action<UpgradeData> OnLevelGranted;

        /// <summary>Raised after a save's upgrade levels are restored, so receivers re-apply stored effects.</summary>
        public static event System.Action OnLevelsLoaded;

        // One entry per UpgradeType (there is exactly one UpgradeData asset per type).
        private readonly Dictionary<UpgradeType, int> levels = new Dictionary<UpgradeType, int>();

        public int GetLevel(UpgradeType type)
        {
            return levels.TryGetValue(type, out int level) ? level : 0;
        }

        public void SetLevel(UpgradeType type, int level)
        {
            levels[type] = Mathf.Max(0, level);
        }

        public int GetPrice(UpgradeData data)
        {
            if (data == null || data.prices == null)
                return 0;

            int level = GetLevel(data.type);

            if (level < 0 || level >= data.prices.Length)
                return 0;

            return data.prices[level];
        }

        /// <summary>
        /// Attempts to buy the next level of an upgrade:
        /// checks max level and money, applies the gameplay effect, increments the level.
        /// Returns true if the purchase succeeded. Shows a "Not enough money" toast
        /// when the purchase fails specifically due to insufficient funds.
        /// </summary>
        public bool TryBuyUpgrade(UpgradeData data)
        {
            if (data == null) return false;

            int level = GetLevel(data.type);
            if (level >= data.maxLevel) return false;

            if (PlayerRegistry.Local == null) return false;

            var money = PlayerRegistry.Local.GetComponent<PlayerMoney>();
            if (money == null) return false;

            int price = GetPrice(data);
            if (!money.Use(price))
            {
                NotificationManager.Notify("Not enough money");
                return false;
            }

            GrantLevel(data);
            return true;
        }

        /// <summary>Gives ONE level of this upgrade: applies the effect, raises the level, and raises
        /// the tutorial action. The shared tail of a purchase — TryBuyUpgrade and the dev button both
        /// end here, so their effects can never drift apart.</summary>
        private void GrantLevel(UpgradeData data)
        {
            OnLevelGranted?.Invoke(data);
            levels[data.type] = GetLevel(data.type) + 1;

            if (data.type == UpgradeType.Rope)
                TutorialEvents.RaiseAction(TutorialActionId.UpgradeRope);
        }

        /// <summary>Dev/testing (DevPanel): gives one level for FREE. Returns false with a reason instead
        /// of raising the level with no effect — e.g. Rope with no player, or Inventory with no manager.</summary>
        public bool DevGrantNextLevel(UpgradeData data, out string failReason)
        {
            failReason = null;

            if (data == null) { failReason = "no upgrade selected"; return false; }
            if (GetLevel(data.type) >= data.maxLevel) { failReason = "already at max level"; return false; }

            if (data.type == UpgradeType.Rope &&
                (PlayerRegistry.Local == null || PlayerRegistry.Local.GetComponentInChildren<OxygenRope>() == null))
            { failReason = "no player / oxygen rope in this scene yet"; return false; }

            if (data.type == UpgradeType.Inventory && Inventory.Current == null)
            { failReason = "InventoryManager not found"; return false; }

            GrantLevel(data);
            return true;
        }

        /// <summary>
        /// Called after a save is loaded: restores levels, then lets receivers re-apply stored effects
        /// (inventory slots). Rope length is NOT re-applied — PlayerHealth restores it from
        /// SaveData.maxRopeLength (re-applying would double the length).
        /// </summary>
        public void ApplyLoadedLevels(int inventoryLevel, int ropeLevel)
        {
            SetLevel(UpgradeType.Inventory, inventoryLevel);
            SetLevel(UpgradeType.Rope, ropeLevel);
            OnLevelsLoaded?.Invoke();
        }

        /// <summary>Clears every upgrade level — called by SaveBridge.ResetForNewGame(). Doesn't
        /// need to un-apply rope length/inventory slots itself: the player gets destroyed and
        /// respawned fresh (default stats) and InventoryManager.ResetToDefault() relocks slots
        /// independently, so there's nothing left on the old levels to undo.</summary>
        public void ResetToDefault() => levels.Clear();

        // ── ISaveParticipant ──

        public void Collect(SaveData data)
        {
            data.inventoryUpgradeLevel    = GetLevel(UpgradeType.Inventory);
            data.ropeUpgradeLevel         = GetLevel(UpgradeType.Rope);
            data.normalBulletUpgradeLevel = GetLevel(UpgradeType.NormalBullet);
        }

        public void Apply(SaveData data)
        {
            // Rope length is restored by PlayerHealth via SaveData.maxRopeLength,
            // so this intentionally does not re-apply the rope effect.
            ApplyLoadedLevels(data.inventoryUpgradeLevel, data.ropeUpgradeLevel);

            // The gun level has no stored effect to re-apply — bullets and the laser read the level directly.
            SetLevel(UpgradeType.NormalBullet, data.normalBulletUpgradeLevel);
        }

    }
}
