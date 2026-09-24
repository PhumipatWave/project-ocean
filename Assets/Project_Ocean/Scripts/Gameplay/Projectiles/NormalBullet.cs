using UnityEngine;

namespace Game.Testing
{
    public class NormalBullet : Bullet
    {
        // Speed × lifetime for the current upgrade level. Uses the same lifetime table the bullet
        // destroys itself with, so the laser and the real shot can never disagree.
        public override float MaxRange =>
            RangeAt(UpgradeService.Instance.GetLevel(UpgradeType.NormalBullet));

        /// <summary>Flight distance at any upgrade level — also read by UpgradeTooltip (on the prefab).</summary>
        public float RangeAt(int level) => moveSpeed * GetLifetime(level);

        protected override void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            player = GameManager.CurPlayer;

            int level = UpgradeService.Instance.GetLevel(
                UpgradeType.NormalBullet
            );

            attackDamage = GetDamage(level);

            Destroy(gameObject, GetLifetime(level));
        }

        // Public so UpgradeTooltip shows the same table the bullet actually uses.
        public static float GetDamage(int level)
        {
            switch (level)
            {
                case 1:
                    return 5f;

                case 2:
                    return 5f;

                case 3:
                    return 10f;

                case 4:
                    return 10f;

                case 5:
                    return 15f;

                default:
                    return 5f;
            }
        }

        private static float GetLifetime(int level)
        {
            switch (level)
            {
                case 1:
                    return 0.5f;

                case 2:
                    return 1.0f;

                case 3:
                    return 1.0f;

                case 4:
                    return 1.5f;

                case 5:
                    return 1.5f;

                default:
                    return 0.5f;
            }
        }
    }
}