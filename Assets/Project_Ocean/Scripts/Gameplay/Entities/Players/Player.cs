using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    [RequireComponent(typeof(PlayerMovement), typeof(PlayerSceneMode))]
    public class Player : Entity
    {
        /// <summary>Raised when the player takes damage (visual feedback moment).</summary>
        public static event System.Action OnPlayerDamaged;

        [Header("Player Component")]
        public InputReader input;

        private PlayerGunHandler gunHandler;
        public PlayerGunHandler GunHandler { get { return gunHandler; } }

        private OxygenRope oxygenTube;
        public OxygenRope OxygenTube { get { return oxygenTube; } }

        private PlayerMovement movement;
        public PlayerMovement Movement { get { return movement; } }

        public Rigidbody2D RB { get { return rb; } }

        [Header("Player Object")]
        [SerializeField] private GameObject playerSprite;
        public GameObject PlayerSprite { get { return playerSprite; } }

        public int FaceDir => movement.FaceDir;
        public float DashSpeed => movement.DashSpeed;

        protected override void Awake()
        {
            oxygenTube = GetComponentInChildren<OxygenRope>();

            base.Awake();

            movement = GetComponent<PlayerMovement>();
            gunHandler = GetComponentInChildren<PlayerGunHandler>();

            DontDestroyOnLoad(gameObject);
        }

        protected override IEntityHealth ProvideHealthSource()
        {
            var playerHealth = GetComponent<PlayerHealth>();
            if (oxygenTube != null)
                playerHealth.SetOxygenRope(oxygenTube);
            return playerHealth;
        }

        private void Start()
        {
            input.OnAttacked += gunHandler.HandleAttackInput;
            input.OnUseItem += HandleUseItem;
        }

        // InputReader is a ScriptableObject and outlives a player destroyed by ResetForNewGame.
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (gunHandler != null) input.OnAttacked -= gunHandler.HandleAttackInput;
            input.OnUseItem -= HandleUseItem;
        }

        private void Update()
        {
            int faceDir = movement.FaceDir;
            gunHandler.UpdateAim(ref faceDir, input.MousePos, input.AimStick, input.UsingGamepadAim);
            movement.FaceDir = faceDir;

            entityAnimation?.PlayAnim(AnimHash.IsAim, gunHandler.IsAiming);
        }

        private void HandleUseItem()
        {
            if (!FeatureUnlockService.Instance.IsUnlocked(GameFeature.UseItem)) return;
            if (GameSession.State == GameState.Ocean)
                PlayerItemUse.Use(this, ItemType.Building);
        }

        protected override void OnDamagedHandler()
        {
            AudioManager.Instance?.PlaySFX(SoundKeys.Hurt);
        }

        // The flash itself runs in PlayerHealth (its flashTarget is the Visual sprite).
        protected override void OnDamagedVisual() => OnPlayerDamaged?.Invoke();
    }
}
