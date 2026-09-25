using UnityEngine;

namespace Game.Testing
{
    /// <summary>Turns money and item changes into Resource-channel notifications.</summary>
    public class ResourceNotificationPresenter : MonoBehaviour
    {
        [SerializeField] private Sprite moneyIcon;

        private void OnEnable()
        {
            PlayerMoney.OnMoneyDelta += HandleMoneyDelta;
            InventoryManager.OnItemDelta += HandleItemDelta;
        }

        private void OnDisable()
        {
            PlayerMoney.OnMoneyDelta -= HandleMoneyDelta;
            InventoryManager.OnItemDelta -= HandleItemDelta;
        }

        /// <summary>"+1 Tuna" / "-20 Money".</summary>
        public static string Format(int delta, string name) => $"{(delta > 0 ? "+" : "")}{delta} {name}";

        private void HandleMoneyDelta(int delta)
        {
            if (delta == 0) return;
            NotificationManager.Notify(Format(delta, "Money"), NotificationChannel.Resource, moneyIcon);
        }

        private void HandleItemDelta(Item item, int delta)
        {
            if (item == null || delta == 0) return;
            NotificationManager.Notify(Format(delta, item.ItemName), NotificationChannel.Resource, item.ItemVisual);
        }
    }
}
