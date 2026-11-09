using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class Bird : MonoBehaviour
    {
        [Header("Patrol Settings")]
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float speed = 2f;

        private Transform targetDestination;
        private SpriteRenderer spriteRenderer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            targetDestination = pointA;
        }

        void Update()
        {
            Move();
            FlipCharacter();
        }

        void Move()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetDestination.position, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetDestination.position) < 0.1f)
                targetDestination = targetDestination == pointA ? pointB : pointA;
        }

        void FlipCharacter()
        {
            if (targetDestination.position.x > transform.position.x)
                spriteRenderer.flipX = false;
            else if (targetDestination.position.x < transform.position.x)
                spriteRenderer.flipX = true;
        }
    }
}
