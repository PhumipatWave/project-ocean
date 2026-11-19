using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    /// <summary>Moves the player from input and owns land/ocean speed, sprint and facing.</summary>
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Move Speed - Land")]
        [SerializeField] private float landMoveSpeed = 5f;
        [SerializeField] private float landDashSpeed = 25f;
        [SerializeField] private float landSprintSpeed = 12f;

        [Header("Move Speed - Ocean")]
        [SerializeField] private float oceanMoveSpeed = 8f;
        [SerializeField] private float oceanDashSpeed = 25f;
        [SerializeField] private float oceanSprintSpeed = 10f;

        private Player player;
        private Rigidbody2D rb;
        private EntityAnimation entityAnimation;
        private PlayerSprintVFX playerSprintVFX;

        private bool isSprinting;
        private bool wasMoving;
        private bool moveTutorialWasRequired;

        public bool IsLand { get; private set; }
        public int FaceDir { get; set; } = 1;

        public float MoveSpeed => IsLand ? landMoveSpeed : oceanMoveSpeed;
        public float DashSpeed => IsLand ? landDashSpeed : oceanDashSpeed;
        public float SprintSpeed => IsLand ? landSprintSpeed : oceanSprintSpeed;

        private void Awake()
        {
            player = GetComponent<Player>();
            rb = GetComponent<Rigidbody2D>();
            entityAnimation = GetComponent<EntityAnimation>();
            playerSprintVFX = GetComponentInChildren<PlayerSprintVFX>(true);
        }

        private void OnEnable() => player.input.OnSprinted += Sprint;
        private void OnDisable() => player.input.OnSprinted -= Sprint;

        // Re-applied every physics step: collisions can zero one velocity axis, and the
        // OnMoved event won't fire again while the same keys stay held.
        private void FixedUpdate()
        {
            ApplyMovement(player.input.MovePos);
        }

        public void SetLand(bool isLand)
        {
            IsLand = isLand;
            entityAnimation?.PlayAnim(AnimHash.IsLand, isLand);
        }

        public void Stop()
        {
            rb.linearVelocity = Vector2.zero;
        }

        private void Sprint(bool state)
        {
            isSprinting = state;

            if (!state)
                playerSprintVFX?.SetSprinting(false);
        }

        private void ApplyMovement(Vector2 dir)
        {
            // Re-baseline when the Move tutorial step appears, so movement already held
            // before the prompt doesn't count as a fresh press.
            bool moveTutorialRequired = TutorialGate.RequiredAction == TutorialActionId.Move;
            if (moveTutorialRequired && !moveTutorialWasRequired)
                wasMoving = dir != Vector2.zero;
            moveTutorialWasRequired = moveTutorialRequired;

            if (player.GunHandler.IsAiming || PlayerUIState.IsLocked)
            {
                Stop();
                playerSprintVFX?.SetSprinting(false);
                // Track raw held input so a key held through the lock isn't a fresh press on release.
                wasMoving = dir != Vector2.zero;
                return;
            }

            float speed = isSprinting ? SprintSpeed : MoveSpeed;
            rb.linearVelocity = dir * speed;

            bool isMoving = rb.linearVelocity != Vector2.zero;
            bool shouldShowSprintVFX = IsLand && isSprinting && isMoving;

            if (shouldShowSprintVFX)
                playerSprintVFX?.SetDirection(dir);

            playerSprintVFX?.SetSprinting(shouldShowSprintVFX);

            // Edge-triggered: play once when movement starts, not every physics tick.
            if (isMoving && !wasMoving)
            {
                if (GameSession.State == GameState.Ocean)
                    AudioManager.Instance?.PlaySFX(SoundKeys.RopeMove);

                if (TutorialGate.RequiredAction == TutorialActionId.Move)
                    GameLog.Info($"[Player] Raising Move tutorial action — dir={dir}, wasMoving was false this frame");
                TutorialEvents.RaiseAction(TutorialActionId.Move);
            }
            wasMoving = isMoving;

            entityAnimation?.PlayAnim(AnimHash.VelX, rb.linearVelocity.x);
            entityAnimation?.PlayAnim(AnimHash.VelY, rb.linearVelocity.y);
            FlipSprite();
        }

        private void FlipSprite()
        {
            if (rb.linearVelocityX > .1f)
            {
                FaceDir = 1;
                player.PlayerSprite.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                player.GunHandler.GunSprite.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (rb.linearVelocityX < -.1f)
            {
                FaceDir = -1;
                player.PlayerSprite.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                player.GunHandler.GunSprite.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
    }
}
