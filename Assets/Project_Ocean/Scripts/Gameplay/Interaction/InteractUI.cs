using UnityEngine;
using UnityEngine.Events;

namespace Game.Gameplay
{
    /// <summary>
    /// Generic interact zone: pressing E fires onInteract. Wire it in the Inspector to
    /// ShopManager.Instance.OpenShopWindow(), UpgradeManager.OpenFromShop(), NPC.Interact(), etc.
    /// </summary>
    public class InteractUI : Interactable
    {
        [Tooltip("Fired when the player presses E while inside this zone.")]
        public UnityEvent onInteract;

        protected override void Interact(Player player) => onInteract?.Invoke();
    }
}
