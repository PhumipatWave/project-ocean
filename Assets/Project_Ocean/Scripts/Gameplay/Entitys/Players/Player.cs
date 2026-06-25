using Game.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class Player : Entity
    {
        [Header("Player Component")]
        public InputReader input;

        [SerializeField] private AudioSource audio;
        public AudioSource Audio { get { return audio; } }

        [SerializeField] private AudioClip playerHurtSound;
        [SerializeField] private AudioClip playerRopeSound;
        [SerializeField] private AudioClip playerShootSound;
        public AudioClip playerCollectItemSound;

        private PlayerGunHandler gunHandler;
        public PlayerGunHandler GunHandler { get { return gunHandler; } }

        private OxygenRope oxygenTube;
        public OxygenRope OxygenTube { get { return oxygenTube; } }

        private readonly List<CheckPointItem> checkpoints = new();
        private Coroutine _activePullCoroutine;
        public int CheckpointCount => checkpoints.Count;
        public bool CanPlaceCheckpoint => checkpoints.Count < 3;
        public CheckPointItem LastCheckpoint => checkpoints.Count > 0 ? checkpoints[^1] : null;

        public void RegisterCheckpoint(CheckPointItem cp)
        {
            checkpoints.Add(cp);
            InventoryManager.Instance?.RefreshBuildingHints();
        }

        public void UnregisterCheckpoint(CheckPointItem cp)
        {
            checkpoints.Remove(cp);
            if (cp.SourceSlotIndex >= 0)
                InventoryManager.Instance?.ClearBuildingSlot(cp.SourceSlotIndex);
            InventoryManager.Instance?.RefreshBuildingHints();
        }

        /// Returns true if the player can place a new checkpoint from the given building slot index.
        /// Each slot may have at most one active checkpoint at a time; total is capped at 3.
        public bool CanPlaceCheckpointFromSlot(int slotIndex)
        {
            if (checkpoints.Count >= 3) return false;
            foreach (var cp in checkpoints)
                if (cp != null && cp.SourceSlotIndex == slotIndex) return false;
            return true;
        }

        public Rigidbody2D RB { get { return rb; } }

        [Header("Player Object")]
        [SerializeField] private GameObject playerSprite;
        public GameObject PlayerSprite { get { return playerSprite; } }


        [Header("Hit Flash")]
        [SerializeField] private float playerFlashDuration = 0.15f;
        private SpriteRenderer playerSpriteRenderer;
        private Color playerOriginalColor;
        private Coroutine playerFlashCoroutine;

        [Header("Player Status")]
        private int faceDir = 1;
        public int FaceDir { get { return faceDir; } }

        [SerializeField] private float moveSpeed = 5f;
        public float MoveSpeed { get { return moveSpeed; } }

        [SerializeField] private float dashSpeed = 25f;
        public float DashSpeed { get { return dashSpeed; } }

        [SerializeField] private float sprintSpeed = 12f;
        public float SprintSpeed { get { return sprintSpeed; } }

        private bool isSprinting;

        private bool isLand;

        private bool isAttacked;
        public bool IsAttacked { get { return isAttacked; } }

        private float originTime = 1f;
        private float focusTime = .6f;

        protected override void Awake()
        {
            oxygenTube = GetComponentInChildren<OxygenRope>();

            base.Awake();

            playerSpriteRenderer = playerSprite != null ? playerSprite.GetComponentInChildren<SpriteRenderer>() : null;
            if (playerSpriteRenderer != null) playerOriginalColor = playerSpriteRenderer.color;

            audio = GetComponent<AudioSource>();
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
            input.OnAimed += Aim;
            input.OnMoved += Move;
            input.OnAttacked += Attack;
            input.OnSprinted += Sprint;
            input.OnInteracted += Interact;

            // Game always starts on land; sceneLoaded for Land_Map fires before the player
            // exists, so we set the initial land state here instead of relying on that event.
            isLand = true;
            gunHandler?.gameObject.SetActive(false);
            entityAnimation?.PlayAnim(AnimHash.IsLand, true);
            oxygenTube?.ResetRope();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayerUIState.Reset();

            if (scene.name == "Land_Map")
            {
                if (SceneLoadContext.IsLoading)
                {
                    transform.position = SceneLoadContext.Position;
                    // Flag is cleared by SaveBridge after all data is applied.
                }
                else
                {
                    transform.position = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").transform.position;
                }

                // Reset rope max length back to full health on returning to land
                (healthSource as PlayerHealth)?.ResetToOriginal();
                oxygenTube?.ResetRope();

                gunHandler.gameObject.SetActive(false);
                isLand = true;
                entityAnimation?.PlayAnim(AnimHash.IsLand, isLand);

                // Bug 4: reset aim/gun state so canAttack is never stuck on land
                gunHandler.IsAimming = false;
                gunHandler.CanAttack = true;
                Time.timeScale = originTime;
                rb.linearVelocity = Vector2.zero;

                // Cancel any in-flight checkpoint pull and clear all checkpoint state
                if (_activePullCoroutine != null)
                {
                    StopCoroutine(_activePullCoroutine);
                    _activePullCoroutine = null;
                }
                foreach (var cp in checkpoints)
                    if (cp != null && cp.SourceSlotIndex >= 0)
                        InventoryManager.Instance?.ClearBuildingSlot(cp.SourceSlotIndex);
                checkpoints.Clear();
                InventoryManager.Instance?.RefreshBuildingHints();
            }
            else if (scene.name == "Ocean_Map")
            {
                Transform spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").transform;

                if (SceneLoadContext.IsLoading)
                {
                    transform.position = SceneLoadContext.Position;
                    // Flag is cleared by SaveBridge after all data is applied.
                }
                else
                {
                    transform.position = spawnPoint.position;
                }

                gunHandler.gameObject.SetActive(true);
                isLand = false;
                entityAnimation?.PlayAnim(AnimHash.IsLand, isLand);

                oxygenTube.SetStartPoint(transform.position);
            }
        }

        private void Update()
        {
            gunHandler.Aimming(ref faceDir, input.MousePos);
            AimAnim();

            // Test Input
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                gunHandler.currentAmmoType = AmmoType.Normal;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                gunHandler.currentAmmoType = AmmoType.GrapplingHook;
            }

            if (GameManager.Instance.GameState == GameState.Ocean)
            {
                // F key: consume Building item from inventory and place it in the world
                if (Input.GetKeyDown(KeyCode.F))
                {
                    UseItem(ItemType.Building);
                }

                // T key: teleport to the last placed checkpoint (free action, no item consumed)
                if (Input.GetKeyDown(KeyCode.T) && LastCheckpoint != null)
                    LastCheckpoint.PullPlayerHere(this);
            }
        }

        private void UseItem(ItemType type)
        {
            var prefab = InventoryManager.Instance.PeekItem(type);
            if (prefab != null)
            {
                var effect = prefab.ItemPrefab?.GetComponent<IItemEffect>();
                if (effect != null && !effect.CanExecute(this))
                {
                    Debug.Log($"[Player] Item use blocked by CanExecute: {type}");
                    return;
                }

                // KeepInInventory: execute without consuming — item cleared later by UnregisterCheckpoint
                if (effect != null && effect.KeepInInventory)
                {
                    effect.ExecuteItem(this);
                    return;
                }
            }

            Item itemData;
            bool success = InventoryManager.Instance.ConsumeItem(type, out itemData);

            if (!success)
            {
                Debug.Log("No item : " + type);
                return;
            }

            ExecuteItem(itemData);
        }

        private void ExecuteItem(Item item)
        {
            if (item.ItemPrefab == null)
            {
                Debug.LogWarning($"[Player] Item '{item.ItemName}' has no prefab assigned.");
                return;
            }

            var effect = item.ItemPrefab.GetComponent<IItemEffect>();
            if (effect != null)
            {
                // IItemEffect handles all spawn/teleport logic itself
                effect.ExecuteItem(this);
            }
            else
            {
                // Fallback: plain building item with no special behavior
                Instantiate(item.ItemPrefab, transform.position, Quaternion.identity);
            }
        }

        #region PlayerInputAction
        private void Aim(bool isAim)
        {
            // Bug 2: block aim entirely when any UI (e.g. inventory) is open
            // Bug 4: block aim when canAttack is false or on land
            if (isAim && (!gunHandler.CanAttack || isLand || PlayerUIState.IsLocked)) return;

            gunHandler.IsAimming = isAim;

            if (isAim)
            {
                // Bug 1: zero velocity so holding a move key doesn't keep the player moving while aiming
                rb.linearVelocity = Vector2.zero;
                Time.timeScale = focusTime;
            }
            else
            {
                Time.timeScale = originTime;
            }
        }

        private void Attack(bool isAttack)
        {
            if (!gunHandler.IsAimming) return;

            isAttacked = isAttack;
            if (isAttack)
            {
                gunHandler.CanAttack = false;
                audio.PlayOneShot(playerShootSound);
                gunHandler.Shooting();
                Debug.Log("Attack");
            }
        }

        private void Sprint(bool state)
        {
            isSprinting = state;
            Move(input.MovePos);
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
            if (gunHandler.IsAimming || PlayerUIState.IsLocked)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            float speed = isSprinting ? sprintSpeed : moveSpeed;
            rb.linearVelocity = dir * speed;

            if (rb.linearVelocity != Vector2.zero && GameManager.Instance.GameState == GameState.Ocean)
            {
                audio.PlayOneShot(playerRopeSound);
            }

            MovementAnim();
            FlipSprite();
        }

        protected override void OnDamagedHandler()
        {
            audio?.PlayOneShot(playerHurtSound);
        }

        protected override void OnDamagedVisual()
        {
            if (playerSpriteRenderer == null) return;
            if (playerFlashCoroutine != null) StopCoroutine(playerFlashCoroutine);
            playerFlashCoroutine = StartCoroutine(PlayerHitFlashRoutine());
        }

        private IEnumerator PlayerHitFlashRoutine()
        {
            playerSpriteRenderer.color = entityHealth.HitColor;
            yield return new WaitForSeconds(playerFlashDuration);
            playerSpriteRenderer.color = playerOriginalColor;
            playerFlashCoroutine = null;
        }


        protected override void OnDeathHandler()
        {
            entityDeathHandler?.HandleDeath();
        }
        #endregion

        #region PlayerAnimation
        private void MovementAnim()
        {
            entityAnimation?.PlayAnim(AnimHash.VelX, rb.linearVelocity.x);
            entityAnimation?.PlayAnim(AnimHash.VelY, rb.linearVelocity.y);
        }

        private void AimAnim()
        {
            entityAnimation?.PlayAnim(AnimHash.IsAim, gunHandler.IsAimming);
        }
        #endregion

        #region PlayerUtils
        // Bug 3: lets CheckPointItem start a pull coroutine that can be cancelled on scene transition
        public void StartPull(IEnumerator routine)
        {
            if (_activePullCoroutine != null) StopCoroutine(_activePullCoroutine);
            _activePullCoroutine = StartCoroutine(routine);
        }

        private void FlipSprite()
        {
            if (rb.linearVelocityX > .1f)
            {
                faceDir = 1;
                playerSprite.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                gunHandler.GunSprite.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else if (rb.linearVelocityX < -.1f)
            {
                faceDir = -1;
                playerSprite.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                gunHandler.GunSprite.rotation = Quaternion.Euler(0f, 180f, 0f);
            }
        }
        #endregion
    }
}
