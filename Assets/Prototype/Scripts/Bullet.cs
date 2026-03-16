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

        [SerializeField] private float moveSpeed = 15f;
        [SerializeField] private float attackDamage = 10f;

        [SerializeField] private float currentRopeLength;
        [SerializeField] private float maxRopeLength = 15f;
        [SerializeField] private float pullBackForce = 5f;
        private bool isPullBack;
        private int segment = 20;

        private Vector2 shootDir;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            playerGun = GameObject.FindGameObjectWithTag("PlayerGun").GetComponent<PlayerGunHandler>();
            line = GetComponent<LineRenderer>();

            line.positionCount = segment;
        }

        public void Initialize(Vector2 shootDir)
        {
            this.shootDir = shootDir;
        }

        private void Update()
        {
            RopeSystemUtil.DrawRope(line, playerGun.FirePoint.position, playerGun.CurrentBulletObj.position, segment: segment);
        }

        private void FixedUpdate()
        {
            if (!isPullBack)
            {
                rb.linearVelocity = shootDir * moveSpeed;
            }

            float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, angle);

            RopeSystemUtil.LimitDistance(playerGun.FirePoint, rb.transform, 20f);
            RopeSystemUtil.BreakRopeLimit(playerGun.FirePoint.position, rb.position, ref isPullBack, ref currentRopeLength, maxRopeLength, rb, pullBackForce);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isPullBack)
            {
                if (collision.tag == "")
                {
                    Debug.Log($"Fire : {collision.name}");

                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.Log("Bullet pull");
                if (collision.tag == "CollectBox")
                {
                    Debug.Log("Collect bullet");
                    Destroy(gameObject);
                }
            }
        }
    }
}
