using Game.Core;
using System;
using System.Collections;
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
        [SerializeField] private AudioClip playerDashSound;
        [SerializeField] private AudioClip playerRopeSound;
        [SerializeField] private AudioClip playerShootSound;
        public AudioClip playerCollectItemSound;

        private PlayerGunHandler gunHandler;
        public PlayerGunHandler GunHandler { get { return gunHandler; } }

        private PlayerOxygenTube oxygenTube;
        public PlayerOxygenTube OxygenTube { get { return oxygenTube; } }

        
        public event System.Action OnDamaged;
        public event System.Action OnDied;

        [Header("Player Health")]
        [SerializeField] private float maxHealth = 100f;
        private float currentHealth;
        private bool  isPlayerDead;

        public Rigidbody2D RB { get { return rb; } }

        [Header("Player Object")]
        [SerializeField] private GameObject playerSprite;
        public GameObject PlayerSprite { get { return playerSprite; } }


        [Header("Hit Flash")]
        [SerializeField] private Color playerHitColor   = new Color(0f, 0.1f, 0.5f);
        [SerializeField] private float playerFlashDuration = 0.15f;
        private SpriteRenderer playerSpriteRenderer;
        private Color          playerOriginalColor;
        private Coroutine      playerFlashCoroutine;

        [Header("Player Status")]
        private int faceDir = 1;
        public int FaceDir { get { return faceDir; } }

        [SerializeField] private float moveSpeed = 5f;
        public float MoveSpeed { get { return moveSpeed; } }

        [SerializeField] private float dashSpeed = 25f;
        public float DashSpeed { get { return dashSpeed; } }

        private bool canDash = true;
        private bool isDash;

        private bool isLand;

        private bool isAttacked;
        public bool IsAttacked { get { return isAttacked; } }

        private float originTime = 1f;
        private float focusTime = .6f;

        protected override void Awake()
        {
            // Resolve oxygenTube BEFORE base.Awake() so ProvideHealthSource
            // can pass it to PlayerHealth via SetOxygenTube.
            oxygenTube = GetComponentInChildren<PlayerOxygenTube>();

            base.Awake();

            playerSpriteRenderer = playerSprite != null ? playerSprite.GetComponentInChildren<SpriteRenderer>() : null;
            if (playerSpriteRenderer != null) playerOriginalColor = playerSpriteRenderer.color;

            audio      = GetComponent<AudioSource>();
            gunHandler = GetComponentInChildren<PlayerGunHandler>();
            DontDestroyOnLoad(gameObject);
        }

        protected override IEntityHealth ProvideHealthSource()
        {
            var playerHealth = GetOrAddComponent<PlayerHealth>();
            if (oxygenTube != null)
                playerHealth.SetOxygenTube(oxygenTube);
            return playerHealth;
        }

        private void Start()
        {
            input.OnAimed += Aim;
            input.OnMoved += Move;
            input.OnAttacked += Attack;
            input.OnDashed += Dash;
            input.OnInteracted += Interact;
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
            if (scene.name == "Land_Map")
            {
                if (SaveBridge.IsLoadingSave)
                {
                    transform.position = SaveBridge.LoadedPosition;
                    SaveBridge.ClearLoadFlag();
                }
                else
                {
                    transform.position = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").transform.position;
                }

                // Reset rope max length back to full health on returning to land
                oxygenTube.ReduceMaxRopeLenght = 0f;
                (healthSource as PlayerHealth)?.ResetToOriginal();

                gunHandler.gameObject.SetActive(false);
                isLand = true;
                entityAnimation?.PlayAnim(AnimHash.IsLand, isLand);
            }
            else if (scene.name == "Ocean_Map")
            {
                Transform spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint").transform;

                if (SaveBridge.IsLoadingSave)
                {
                    transform.position = SaveBridge.LoadedPosition;
                    SaveBridge.ClearLoadFlag();
                }
                else
                {
                    transform.position = spawnPoint.position;
                }

                gunHandler.gameObject.SetActive(true);
                isLand = false;
                entityAnimation?.PlayAnim(AnimHash.IsLand, isLand);

                Debug.Log(oxygenTube.StartPointPrefab);

                // ถ้าไม่มี StartPoint (เช่น เล่นใหม่) ให้สร้างอันใหม่
                if (oxygenTube.StartPoint == null)
                {
                    var point = Instantiate(oxygenTube.StartPointPrefab, transform.position, Quaternion.identity);
                    oxygenTube.StartPoint = point;
                }

                oxygenTube.Rebuild();
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
            /*else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
            }*/

            /*if (Input.GetKeyDown(KeyCode.V) && gunHandler.CanStabAttack)
            {
                StabAnim();
                StartCoroutine(gunHandler.Stab());
            }*/

            if (GameManager.Instance.GameState == GameState.Ocean)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    UseItem(ItemType.Building);
                }

                /*if (Input.GetKeyDown(KeyCode.T))
                    oxygenTube.TeleportToCheckPoint();*/
            }
        }

        private void UseItem(ItemType type)
        {
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
            switch (item.ItemType)
            {
                case ItemType.Normal:

                    break;

                case ItemType.KeyItem:

                    break;

                case ItemType.Consumable:
                    
                    break;

                case ItemType.Building:
                    Instantiate(item.ItemPrefab, transform.position, Quaternion.identity);
                    break;
            }
        }

        #region PlayerInputAction
        private void Aim(bool isAim)
        {
            if (!gunHandler.CanAttack || isLand) return;

            gunHandler.IsAimming = isAim;

            if (isAim)
            {
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

        private void Dash(bool isDash)
        {
            if (isLand)
                return;

            if (isDash && canDash)
            {
                StartCoroutine(DashRoutine());
                audio.PlayOneShot(playerDashSound);
            }
        }

        private IEnumerator DashRoutine()
        {
            isDash = true;
            canDash = false;

            Vector2 dir;
            if (input.MovePos.normalized != Vector2.zero)
            {
                dir = input.MovePos.normalized;
            }
            else
            {
                dir = Vector2.right * faceDir;
            }

            rb.linearVelocity = dir * dashSpeed;
            yield return new WaitForSeconds(.5f);

            isDash = false;
            rb.linearVelocity = input.MovePos.normalized * moveSpeed;

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

        private void Move(Vector2 dir)
        {
            if (gunHandler.IsAimming || isDash)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            rb.linearVelocity = dir * moveSpeed;

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
            playerSpriteRenderer.color = playerHitColor;
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

        private void StabAnim()
        {
            entityAnimation?.PlayAnim(AnimHash.Stab);
        }
        #endregion

        #region PlayerUtils
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