using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>
    /// Centralised animator parameter hashes shared across all entities.
    /// Add a new entry here whenever a new animator parameter is needed.
    /// Using hashes instead of strings avoids GC alloc and string comparison at runtime.
    /// </summary>
    public static class AnimHash
    {
        // ── Shared ───────────────────────────────────────────────────────────
        public static readonly int TakeDamage = Animator.StringToHash("takeDamage");
        public static readonly int IsDeath    = Animator.StringToHash("isDeath");

        // ── Enemy ────────────────────────────────────────────────────────────
        public static readonly int IsAttack   = Animator.StringToHash("isAttack");

        // ── Player ───────────────────────────────────────────────────────────
        public static readonly int VelX       = Animator.StringToHash("velX");
        public static readonly int VelY       = Animator.StringToHash("velY");
        public static readonly int IsAim      = Animator.StringToHash("isAim");
        public static readonly int IsLand     = Animator.StringToHash("isLand");
        public static readonly int Stab       = Animator.StringToHash("stab");
    }
}
