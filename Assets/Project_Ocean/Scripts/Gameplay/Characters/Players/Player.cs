using Game.Core;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
{
    public class Player : MonoBehaviour
    {
        [Header("Player Component")]
        public InputReader input;

        [SerializeField] private Animator anim;

        [SerializeField] private AudioSource audio;
        public AudioSource Audio { get { return audio; } }

        private PlayerGunHandler gunHandler;
        public PlayerGunHandler GunHandler { get { return gunHandler; } }

        private PlayerOxygenTube oxygenTube;
        public PlayerOxygenTube OxygenTube { get { return oxygenTube; } }

        [SerializeField] private Rigidbody2D rb;
        public Rigidbody2D RB { get { return rb; } }

        [Header("Player Object")]
        [SerializeField] private Transform playerSprite;
        public Transform PlayerSprite { get { return playerSprite; } }


        [Header("Player Status")]
        private int faceDir = 1;
        public int FaceDir { get { return faceDir; } }

        [SerializeField] private float moveSpeed = 5f;
        public float MoveSpeed { get { return moveSpeed; } }

        [SerializeField] private float dashSpeed = 25f;
        public float DashSpeed { get { return dashSpeed; } }

        private bool canDash = true;

        private bool isAttacked;
        public bool IsAttacked { get { return isAttacked; } }

        private float originTime = 1f;
        private float focusTime = .6f;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponentInChildren<Animator>();

            gunHandler = GetComponentInChildren<PlayerGunHandler>();
            oxygenTube = GetComponentInChildren<PlayerOxygenTube>();

            DontDestroyOnLoad(gameObject);
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
            if (scene.name == "Graybox_Oceans")
            {
                Debug.Log($"Start scene {scene.name}");

                transform.position = new Vector3(-75f, 45f, 0f);

                Debug.Log(oxygenTube.StartPointPrefab);

                var point = Instantiate(oxygenTube.StartPointPrefab, new Vector3(-75f, 45f, 0f), Quaternion.identity);
                oxygenTube.StartPoint = point;
                oxygenTube.Rebuild();

                //StartCoroutine(SetupOnOceanRoutine());
            }
        }

        private IEnumerator SetupOnOceanRoutine()
        {
            GameObject point = null;

            // wait until found (max safety)
            while (point == null)
            {
                point = GameObject.FindGameObjectWithTag("OxygenTubePoint");
                yield return null;
            }

            oxygenTube.StartPoint = point;
            oxygenTube.Rebuild();
        }

        private void Update()
        {
            gunHandler.Aimming(ref faceDir, input.MousePos);
            AimAnim();

            if (Input.GetKeyDown(KeyCode.V) && gunHandler.CanStabAttack)
            {
                StabAnim();
                StartCoroutine(gunHandler.Stab());
            }

            //PreventMoveOutFromWater();
        }

        #region PlayerInputAction
        private void Aim(bool isAim)
        {
            if (!gunHandler.CanAttack) return;

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
                gunHandler.Shooting();
                Debug.Log("Attack");
            }
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
            MovementAnim();
            FlipSprite();
        }
        #endregion

        #region PlayerAnimation
        private void MovementAnim()
        {
            anim.SetFloat("velX", rb.linearVelocity.x);
            anim.SetFloat("velY", rb.linearVelocity.y);
        }

        private void AimAnim()
        {
            anim.SetBool("isAim", gunHandler.IsAimming);
        }

        private void StabAnim()
        {
            anim.SetTrigger("stab");
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