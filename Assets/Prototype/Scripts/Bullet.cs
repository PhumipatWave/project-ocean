using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;

namespace Game.Testing
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private LineRenderer line;
        [SerializeField] private PlayerGunHandler playerGun;
        [SerializeField] private RopeData ropeData;

        [SerializeField] private float moveSpeed = 15f;
        [SerializeField] private float attackDamage = 10f;

        [SerializeField] private float currentRopeLength;
        [SerializeField] private float pullBackForce = 5f;

        private bool isPullBack;
        public bool IsPullBack { get { return isPullBack; } }

        private Vector2 shootDir;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerGun = GameObject.FindGameObjectWithTag("PlayerGun").GetComponent<PlayerGunHandler>();
            line = GetComponent<LineRenderer>();

            line.positionCount = ropeData.ropeSegment;
        }

        public void Initialize(Vector2 shootDir)
        {
            this.shootDir = shootDir;
        }

        private void FixedUpdate()
        {
            if (!isPullBack)
            {
                rb.linearVelocity = shootDir * moveSpeed;
            }
            else
            {
                rb.linearVelocity = -shootDir * moveSpeed;
            }

            float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle);

            playerGun.AttachRope(this.gameObject, line, ropeData.ropeSegment, ref isPullBack, ref currentRopeLength, ropeData.maxRopeLength, pullBackForce);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isPullBack)
            {
                switch (collision.tag)
                {
                    case "Enemy":
                        Debug.Log($"Fire : {collision.name}");
                        collision.GetComponent<Enemy>().IsHooked();
                        Destroy(gameObject);
                        break;
                }
            }
        }
    }
}
