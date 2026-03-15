using Game.Core;
using UnityEngine;

namespace Game.Testing
{
    public class Player : MonoBehaviour
    {
        public InputReader input;

        [SerializeField] private Rigidbody2D rb;
        public Rigidbody2D RB { get { return rb; } }

        [SerializeField] private Transform playerSprite;
        [SerializeField] private Transform gunSprite;

        private int oxygen;
        private int maxOxygen = 100;
        private int faceDir = 1;

        [SerializeField] private float moveSpeed = 5f;
        public float MoveSpeed { get { return moveSpeed; } }

        private float attackDamage = 10f;

        private bool isAimming;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            input.OnAimed += Aim;
            input.OnMoved += Move;
            input.OnAttacked += Attack;
            input.OnDashed += Dash;
            input.OnInteracted += Interact;
        }

        private void Update()
        {
            Aimming();
        }

        #region PlayerInputAction
        private void Aim(bool isAim)
        {
            isAimming = isAim;
        }

        private void Attack(bool isAttack)
        {
            if (isAttack)
            {
                Debug.Log("Attack");
            }
        }

        private void Dash(bool isDash)
        {
            if (isDash)
            {
                Debug.Log("Dash");
            }
        }

        private void Interact(bool isInteract)
        {
            if (isInteract)
            {
                Debug.Log("Interact");
            }
        }

        private void Move(Vector2 dir)
        {
            rb.linearVelocity = dir * moveSpeed;

            FlipSprite();
        }
        #endregion

        #region PlayerUtils
        private void FlipSprite()
        {
            if (rb.linearVelocityX > .1f)
            {
                faceDir = 1;
                playerSprite.rotation = Quaternion.Euler(0f, 0f, 0f);
                gunSprite.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (rb.linearVelocityX < -.1f)
            {
                faceDir = -1;
                playerSprite.rotation = Quaternion.Euler(0f, 180f, 0f);
                gunSprite.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }

        private void Aimming()
        {
            if (isAimming)
            {
                Vector2 aimScreenPos = input.MousePos;
                Vector2 aimWorldPos = Camera.main.ScreenToWorldPoint(aimScreenPos);

                Vector2 dir = (aimWorldPos - (Vector2)transform.position).normalized;
                float angle = Mathf.Atan2(aimWorldPos.y, aimWorldPos.x) * Mathf.Rad2Deg;

                //bool isLeft = angle > 90 || angle < -90;
                float clampAngle;

                if (faceDir == 1)
                {
                    clampAngle = Mathf.Clamp(angle, -45f, 45f);
                }
                else
                {
                    if (angle > 0f)
                    {
                        clampAngle = Mathf.Clamp(angle, 135f, 225f);
                    }
                    else
                    {
                        clampAngle = Mathf.Clamp(angle, -180f, -135f);
                    }
                }

                gunSprite.eulerAngles = new Vector3(0, 0, clampAngle);
            }
            else
            {
                if (faceDir == 1)
                {
                    gunSprite.rotation = Quaternion.Euler(0f, 0f, 0f);
                }
                else
                {
                    gunSprite.rotation = Quaternion.Euler(0f, 180f, 0f);
                }
            }
        }
        #endregion
    }
}