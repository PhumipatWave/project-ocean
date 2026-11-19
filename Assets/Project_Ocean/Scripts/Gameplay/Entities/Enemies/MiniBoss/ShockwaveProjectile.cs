using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>A wave that flies straight, hurts the player once, and returns to the pool on hit, wall or timeout.</summary>
    public class ShockwaveProjectile : MonoBehaviour, IPoolable
    {
        private Vector2 direction;
        private float speed;
        private float lifeLeft;
        private float damage;
        private LayerMask wallLayer;
        private bool spent;

        public void Initialize(Vector2 direction, float speed, float lifetime, float damage, LayerMask wallLayer)
        {
            this.direction = direction.normalized;
            this.speed = speed;
            this.damage = damage;
            this.wallLayer = wallLayer;
            lifeLeft = lifetime;
            spent = false;

            transform.right = this.direction;
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * (speed * Time.deltaTime));

            lifeLeft -= Time.deltaTime;
            if (lifeLeft <= 0f)
                Despawn();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (spent)
                return;

            if (other.CompareTag("Player"))
            {
                other.GetComponentInParent<Player>()?.TakeDamage(damage);
                Despawn();
                return;
            }

            if ((wallLayer.value & (1 << other.gameObject.layer)) != 0)
                Despawn();
        }

        private void Despawn()
        {
            spent = true;
            ObjectPoolManager.Instance?.Return(gameObject);
        }

        public void OnGetFromPool() => spent = false;
        public void OnReturnToPool() { }
    }
}
