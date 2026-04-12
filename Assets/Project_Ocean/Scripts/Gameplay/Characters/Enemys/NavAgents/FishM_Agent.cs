using Game.Testing;
using UnityEngine;
using UnityEngine.AI;

public class FishM_Agent : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Enemy enemy;

    [Header("Movement")]
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float turnSpeed = 5f;

    [Header("Attack Settings")]
    [SerializeField] private float detectDistance = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackTimer = 0f;

    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderChangeInterval = 2f;

    private float wanderTimer = 0f;

    private NavMeshAgent agent;
    private bool isFollowing = false;
    private Vector2 currentDirection;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();

        // IMPORTANT for 2D
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;

        wanderTimer = wanderChangeInterval;
    }

    private void Update()
    {
        if (enemy.Player == null || enemy.IsDeath) return;

        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);

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

    private void FollowAndBite()
    {
        agent.SetDestination(enemy.Player.transform.position);

        // update direction from velocity
        if (agent.velocity.magnitude > 0.1f)
            currentDirection = agent.velocity.normalized;

        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);

        if (dist <= attackRange && attackTimer <= 0f)
        {
            Debug.Log("Player ถูกกัดโดย NavMesh Fish!");
            attackTimer = attackCooldown;
        }
    }

    private void Wander()
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

    private void RotateFish()
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
