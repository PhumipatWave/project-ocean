using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class FishS_Agent : MonoBehaviour
{
    [Header("Fish Settings")]
    public float speed = 2f;
    public float turnSpeed = 5f;
    public float groupRadius = 3f;
    public float separationDistance = 1f;
    public List<FishS_Agent> neighbors = new List<FishS_Agent>();

    [Header("Sensor Settings")]
    public float viewDistance = 6f;
    public float viewAngle = 90f;

    [Header("Wander Settings")]
    public float wanderRadius = 3f;
    public float wanderInterval = 3f;

    [Header("Wall Avoid")]
    public float wallDetectDistance = 1f;
    public LayerMask wallLayer;

    private Vector2 currentDirection;
    private NavMeshAgent agent;

    // Wander
    private Vector2 wanderTarget;
    private float wanderTimer = 0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;
        SetNewWanderTarget();
    }

    private void Update()
    {
        // หาเพื่อนในกลุ่ม
        neighbors.Clear();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, groupRadius);
        foreach (var hit in hits)
        {
            if (hit != null && hit != GetComponent<Collider2D>())
            {
                FishS_Agent fish = hit.GetComponent<FishS_Agent>();
                if (fish != null)
                    neighbors.Add(fish);
            }
        }

        Vector2 moveDir = Vector2.zero;

        // ตรวจจับ Player อัตโนมัติ
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        bool playerNearby = false;
        if (playerObj != null && CanSeePlayer(playerObj.transform))
        {
            Vector2 fleeDir = ((Vector2)transform.position - (Vector2)playerObj.transform.position).normalized;
            moveDir += fleeDir * 2f; // หนี Player
            playerNearby = true;
        }

        // Wander แบบสุ่ม ถ้า Player ไม่อยู่ใกล้
        if (!playerNearby)
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f || Vector2.Distance(transform.position, wanderTarget) < 0.5f)
            {
                SetNewWanderTarget();
            }

            Vector2 toTarget = wanderTarget - (Vector2)transform.position;
            moveDir += toTarget.normalized;
        }

        // Cohesion + Separation
        Vector2 cohesion = Vector2.zero;
        Vector2 separation = Vector2.zero;
        foreach (var neighbor in neighbors)
        {
            Vector2 toNeighbor = (Vector2)neighbor.transform.position - (Vector2)transform.position;
            cohesion += toNeighbor;
            if (toNeighbor.magnitude < separationDistance)
                separation -= toNeighbor;
        }

        if (neighbors.Count > 0)
        {
            cohesion /= neighbors.Count;
            moveDir += -cohesion.normalized;
            moveDir += separation.normalized;
        }

        // Wall Avoid
        Vector2 wallDir = WallAvoidance();
        moveDir += wallDir;

        // Move
        if (moveDir.sqrMagnitude > 0.01f)
        {
            currentDirection = moveDir.normalized;
            agent.SetDestination((Vector3)transform.position + (Vector3)currentDirection);
        }

        RotateFish(currentDirection);
    }

    private void SetNewWanderTarget()
    {
        Vector2 randomDir = Random.insideUnitCircle * wanderRadius;
        wanderTarget = (Vector2)transform.position + randomDir;
        wanderTimer = wanderInterval;
    }

    private bool CanSeePlayer(Transform playerTransform)
    {
        Vector2 toPlayer = (Vector2)playerTransform.position - (Vector2)transform.position;
        if (toPlayer.magnitude > viewDistance) return false;

        float angle = Vector2.Angle(currentDirection, toPlayer);
        if (angle > viewAngle / 2f) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer, toPlayer.magnitude, wallLayer);
        if (hit.collider != null && hit.transform != playerTransform) return false;

        return true;
    }

    private Vector2 WallAvoidance()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, currentDirection, wallDetectDistance, wallLayer);
        if (hit.collider != null)
        {
            return Vector2.Perpendicular(currentDirection).normalized;
        }
        return Vector2.zero;
    }

    private void RotateFish(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.01f) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), turnSpeed * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 forward = transform.right;
        float halfView = viewAngle / 2f;
        Vector3 left = Quaternion.Euler(0f, 0f, halfView) * forward;
        Vector3 right = Quaternion.Euler(0f, 0f, -halfView) * forward;
        Gizmos.DrawLine(transform.position, transform.position + left * viewDistance);
        Gizmos.DrawLine(transform.position, transform.position + right * viewDistance);
    }
}