using UnityEngine;

namespace Game.Testing
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private LineRenderer line;
        [SerializeField] private PlayerGunHandler playerGun;
        [SerializeField] private RopeData ropeData;

        [SerializeField] private float currentRopeLength;
        [SerializeField] private float pullBackForce = 5f;

        [SerializeField] private bool isPullBack;
        public bool IsPullBack { get { return isPullBack; } }

        private float maxHealth = 100;
        [SerializeField] private float currentHealth;
        public float CurrentHealth { get { return currentHealth; } }

        private bool isHookToGun;
        public bool IsHookToGun { get { return isHookToGun; } set { isHookToGun = value; } }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerGun = GameObject.FindGameObjectWithTag("PlayerGun").GetComponent<PlayerGunHandler>();
            line = GetComponent<LineRenderer>();

            line.positionCount = ropeData.ropeSegment;

            currentHealth = maxHealth;
        }

        private void FixedUpdate()
        {
            if (playerGun.IsHooked && isHookToGun)
            {
                playerGun.AttachRope(this.gameObject, line, ropeData.ropeSegment, ref isPullBack, ref currentRopeLength, ropeData.maxRopeLength, pullBackForce);
            }
            else if (!playerGun.IsHooked)
            {
                RopeSystemUtil.ClearNormalRope(line);
            }
        }

        public void TakeDamage(float amount)
        {
            currentHealth -= amount;
        }
    }
}
