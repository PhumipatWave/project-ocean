using UnityEngine;
using UnityEngine.AI;

public class FishM_Agent : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float speed = 2.5f;
    public float turnSpeed = 5f;

    [Header("Attack Settings")]
    public float detectDistance = 6f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    private float attackTimer = 0f;

    [Header("Wander Settings")]
    public float wanderRadius = 3f;
    public float wanderChangeInterval = 2f;
    private float wanderTimer = 0f;

    private NavMeshAgent agent;
    private bool isFollowing = false;
    private Vector2 currentDirection;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // IMPORTANT for 2D
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;

        wanderTimer = wanderChangeInterval;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // cooldown
        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        // detect logic
        if (dist < detectDistance)
            isFollowing = true;
        else if (dist > detectDistance + 5f)
            isFollowing = false;

        if (isFollowing)
            FollowAndBite();
        else
            Wander();

        RotateFish();
    }

    void FollowAndBite()
    {
        agent.SetDestination(player.position);

        // update direction from velocity
        if (agent.velocity.magnitude > 0.1f)
            currentDirection = agent.velocity.normalized;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange && attackTimer <= 0f)
        {
            Debug.Log("Player ถูกกัดโดย NavMesh Fish!");
            attackTimer = attackCooldown;
        }
    }

    void Wander()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f || agent.remainingDistance < 0.2f)
        {
            Vector2 randomPos = Random.insideUnitCircle * wanderRadius;
            Vector3 target = transform.position + new Vector3(randomPos.x, randomPos.y, 0);

            agent.SetDestination(target);
            wanderTimer = wanderChangeInterval;
        }

        if (agent.velocity.magnitude > 0.1f)
            currentDirection = agent.velocity.normalized;
    }

    void RotateFish()
    {
        if (currentDirection != Vector2.zero)
        {
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRot,
                turnSpeed * Time.deltaTime
            );
        }
    }
}
