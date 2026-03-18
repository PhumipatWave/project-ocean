using Game.Core;
using System;
using System.Collections;
using UnityEngine;

namespace Game.Testing
{
    public class Player : MonoBehaviour
    {
        [Header("Player Component")]
        public InputReader input;
        private PlayerGunHandler gunHandler;

        [SerializeField] private Rigidbody2D rb;
        public Rigidbody2D RB { get { return rb; } }

        [Header("Player Object")]
        [SerializeField] private Transform playerSprite;


        [Header("Player Status")]
        private int faceDir = 1;

        [SerializeField] private float moveSpeed = 5f;
        public float MoveSpeed { get { return moveSpeed; } }
        [SerializeField] private float dashSpeed = 25f;

        private bool canDash = true;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            gunHandler = GetComponentInChildren<PlayerGunHandler>();
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
            gunHandler.Aimming(faceDir, input.MousePos);
            PreventMoveOutFromWater();
        }

        #region PlayerInputAction
        private void Aim(bool isAim)
        {
            if (!gunHandler.CanAttack) return;

            gunHandler.IsAimming = isAim;
        }

        private void Attack(bool isAttack)
        {
            if (!gunHandler.IsAimming) return;

            if (isAttack)
            {
                StartCoroutine(AttackRoutine());
            }
        }

        private IEnumerator AttackRoutine()
        {
            gunHandler.CanAttack = false;
            gunHandler.Shooting();
            Debug.Log("Attack");

            yield return new WaitForSeconds(2.25f);
            gunHandler.CanAttack = true;
        }

        private void Dash(bool isDash)
        {
            if (isDash && canDash)
            {
                StartCoroutine(DashRoutine());
            }
        }

        private IEnumerator DashRoutine()
        {
            canDash = false;
            Vector2 dir = input.MovePos.normalized;

            rb.linearVelocity = dir * dashSpeed;
            yield return new WaitForSeconds(0.25f);

            rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(2.25f);
            canDash = true;
        }

        private void Interact(bool isInteract)
        {
            if (isInteract)
            {
                Debug.Log("Interact");
            }
        }

        private void PreventMoveOutFromWater()
        {
            if (transform.position.y > 0)
            {
                transform.position = new Vector2(transform.position.x, 0);
            }
        }

        private void Move(Vector2 dir)
        {
            if (gunHandler.IsAimming)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

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
                gunHandler.GunSprite.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (rb.linearVelocityX < -.1f)
            {
                faceDir = -1;
                playerSprite.rotation = Quaternion.Euler(0f, 180f, 0f);
                gunHandler.GunSprite.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
        #endregion
    }
}