using UnityEngine;

namespace Game.Testing
{
    public class NormalBullet : Bullet
    {
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

        private float GetDamage(int level)
        {
            switch (level)
            {
                case 1:
                    return 5f;

                case 2:
                    return 5f;

                case 3:
                    return 5f;

                case 4:
                    return 10f;

                case 5:
                    return 15f;

                default:
                    return 5f;
            }
        }

        private float GetLifetime(int level)
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