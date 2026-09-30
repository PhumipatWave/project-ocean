using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public static class PlayerRegistry
    {
        /// <summary>The local player on this machine.</summary>
        public static Player Local { get; private set; }

        /// <summary>Register the local player. Called by GameManager after instantiation.</summary>
        public static void Register(Player player)
        {
            Local = player;
        }

        /// <summary>Clear the local player reference.</summary>
        public static void Unregister(Player player)
        {
            if (Local == player)
                Local = null;
        }

        /// <summary>The local player if alive, else null.</summary>
        public static Player GetLiveLocal()
        {
            if (Local == null || Local.IsDead) return null;
            return Local;
        }
    }
}
