using UnityEngine;

namespace Game.Testing
{
    public class Environment : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private LineRenderer line;
        [SerializeField] private PlayerGunHandler playerGun;
        [SerializeField] private RopeData ropeData;

        [SerializeField] private float currentRopeLength;
        [SerializeField] private float pullBackForce = 5f;

        [SerializeField] private bool isPullBack;
        public bool IsPullBack { get { return isPullBack; } }

        private bool isHookToGun;
        public bool IsHookToGun { get { return isHookToGun; } set { isHookToGun = value; } }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>(); 
            playerGun = GameObject.FindGameObjectWithTag("PlayerGun").GetComponent<PlayerGunHandler>();
            line = GetComponent<LineRenderer>();

            line.positionCount = ropeData.ropeSegment;
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
    }
}
