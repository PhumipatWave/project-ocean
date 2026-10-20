using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core
{
    public enum UpgradeType
    {
        Inventory,
        Rope,
        NormalBullet
    }

    [Serializable]
    public class UpgradeUnlock
    {
        [Tooltip("Unlock Name such as Laser, Thermal Vision")]
        public string unlockName;

        [Tooltip("Upgrade to Level ??")]
        public int unlockLevel = 1;
    }

    [CreateAssetMenu(menuName = "Game/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Basic Info")]
        public string upgradeName;

        [TextArea]
        public string description;

        public UpgradeType type;

        [Header("Upgrade Settings")]
        public int maxLevel = 6;

        [Header("Upgrade Prices")]
        public int[] prices;

        [Header("Upgrade Value")]
        [Tooltip("Amount added per upgrade level, e.g. Rope +3 m.")]
        public float value;

        [Header("Unlocks")]
        [Tooltip("What this upgrade unlocks.")]
        public List<UpgradeUnlock> unlocks = new List<UpgradeUnlock>();
    }
}