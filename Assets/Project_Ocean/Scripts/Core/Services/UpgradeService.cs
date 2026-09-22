using System.Collections.Generic;
using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Single source of truth for upgrade levels.
    /// Plain C# (no MonoBehaviour, no scene presence) — reachable from any scene,
    /// whether or not the upgrade panel exists or is active.
    /// UpgradeManager (Land panel UI) and SaveBridge both go through this service.
    /// </summary>
    public class UpgradeService : ISaveParticipant
    {
        private static UpgradeService instance;
        public static UpgradeService Instance => instance ??= new UpgradeService();

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

            if (GameManager.CurPlayer == null) return false;

            var money = GameManager.CurPlayer.GetComponent<PlayerMoney>();
            if (money == null) return false;

            int price = GetPrice(data);
            if (!money.Use(price))
            {
                NotificationManager.Notify("Not enough money");
                return false;
            }

            ApplyEffect(data);
            levels[data.type] = level + 1;

            if (data.type == UpgradeType.Rope)
                TutorialEvents.RaiseAction(TutorialActionId.UpgradeRope);

            return true;
        }

        /// <summary>Applies the gameplay effect of gaining ONE level of this upgrade.</summary>
        private void ApplyEffect(UpgradeData data)
        {
            switch (data.type)
            {
                case UpgradeType.Inventory:
                    if (InventoryManager.Instance != null)
                        InventoryManager.Instance.UnlockNextSlot();
                    break;

                case UpgradeType.Rope:
                    var tube = GameManager.CurPlayer != null
                        ? GameManager.CurPlayer.GetComponentInChildren<OxygenRope>()
                        : null;

                    if (tube != null)
                    {
                        tube.IncreaseMaxLength(data.value);

                        GameManager.CurPlayer.GetComponent<PlayerHealth>()
                            ?.IncreaseOriginalMaxLength(data.value);
                    }
                    break;

                case UpgradeType.NormalBullet:
                    break;
            }
        }

        /// <summary>
        /// Called by SaveBridge after a save is loaded.
        /// Restores levels and re-applies inventory slot unlocks idempotently.
        /// Rope length is NOT re-applied here — SaveBridge restores it directly
        /// via SaveData.maxRopeLength (re-applying would double the length).
        /// </summary>
        public void ApplyLoadedLevels(int inventoryLevel, int ropeLevel)
        {
            SetLevel(UpgradeType.Inventory, inventoryLevel);
            SetLevel(UpgradeType.Rope, ropeLevel);

            if (InventoryManager.Instance != null)
                InventoryManager.Instance.SetUnlockedExtraSlots(inventoryLevel);
            else
                Debug.LogWarning("[UpgradeService] InventoryManager not found — slot unlocks not restored");
        }

        /// <summary>Clears every upgrade level — called by SaveBridge.ResetForNewGame(). Doesn't
        /// need to un-apply rope length/inventory slots itself: the player gets destroyed and
        /// respawned fresh (default stats) and InventoryManager.ResetToDefault() relocks slots
        /// independently, so there's nothing left on the old levels to undo.</summary>
        public void ResetToDefault() => levels.Clear();

        // ── ISaveParticipant ──

        public void Collect(SaveData data)
        {
            data.inventoryUpgradeLevel = GetLevel(UpgradeType.Inventory);
            data.ropeUpgradeLevel      = GetLevel(UpgradeType.Rope);
        }

        public void Apply(SaveData data)
        {
            // Rope length is restored by SaveBridge via SaveData.maxRopeLength,
            // so this intentionally does not re-apply the rope effect.
            ApplyLoadedLevels(data.inventoryUpgradeLevel, data.ropeUpgradeLevel);
        }

    }
}
