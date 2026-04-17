using Game.Testing;
using UnityEngine;
using UnityEngine.AI;

public class FishL_Agent : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Enemy enemy;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float boostSpeed = 7f;
    [SerializeField] private float turnSpeed = 5f;
    [SerializeField] private float swayAmount = 0.1f;

    [Header("Wander Settings")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderChangeInterval = 2f;

    private float wanderTimer;

    [Header("Attack Settings")]
    [SerializeField] private float detectDistance = 5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float prepareTime = 2f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float followAfterLost = 6f;

    private NavMeshAgent agent;

    private bool isPreparing = false;

    private bool isAttacking = false;
    public bool IsAttacking { get { return isAttacking; } }

    private bool isFollowing = false;

    private float prepareTimer = 0f;
    private float attackTimer = 0f;
    private float followTimer = 0f;

    private Vector3 wanderTarget;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        wanderTimer = wanderChangeInterval;
        PickNewWanderPoint();
    }

    private void Update()
    {
        if (enemy.Player == null || enemy.IsDeath) return;

        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);

        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
        if (followTimer > 0f) followTimer -= Time.deltaTime;

        // Start prepare
        if (!isPreparing && !isAttacking && dist < detectDistance && dist > attackRange && attackTimer <= 0f)
        {
            StartPrepare();
        }

        // Lost player → follow
        if (!isPreparing && !isAttacking && dist > detectDistance && followTimer <= 0f)
        {
            isFollowing = true;
            followTimer = followAfterLost;
        }

        if (isPreparing)
            Prepare();
        else if (isAttacking)
            Attack();
        else if (isFollowing)
            Follow();
        else
            Wander();

        RotateFish();
    }

    private void StartPrepare()
    {
        isPreparing = true;
        prepareTimer = prepareTime;

        agent.isStopped = true; // stop moving while aiming
    }

    private void Prepare()
    {
        prepareTimer -= Time.deltaTime;

        // Look at player
        Vector2 dir = (enemy.Player.transform.position - transform.position).normalized;
        RotateTowards(dir);

        if (prepareTimer <= 0f)
        {
            isPreparing = false;
            isAttacking = true;

            agent.isStopped = false;
            agent.speed = boostSpeed;
        }
    }

    private void Attack()
    {
        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);

        if (dist <= attackRange)
        {
            Debug.Log("Player hit by fish!");
            StopAttack();
            return;
        }

        // Add sway to make it feel alive
        Vector2 sway = new Vector2(Mathf.Sin(Time.time * 2f), Mathf.Cos(Time.time * 2f)) * swayAmount;
        Vector3 target = enemy.Player.transform.position + (Vector3)sway;

        agent.SetDestination(target);

        if (dist > detectDistance)
        {
            isAttacking = false;
            isFollowing = true;
            followTimer = followAfterLost;
        }
    }

    private void StopAttack()
    {
        isAttacking = false;
        attackTimer = attackCooldown;
        agent.speed = speed;
    }

    private void Follow()
    {
        agent.speed = speed;
        agent.SetDestination(enemy.Player.transform.position);

        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);

        if (followTimer <= 0f || dist > detectDistance + 10f)
        {
            isFollowing = false;
        }
    }

    private void Wander()
    {
        agent.speed = speed;

        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f || Vector2.Distance(transform.position, wanderTarget) < 0.5f)
        {
            PickNewWanderPoint();
            wanderTimer = wanderChangeInterval;
        }

        agent.SetDestination(wanderTarget);
    }

    private void PickNewWanderPoint()
    {
        Vector2 randomDir = Random.insideUnitCircle * wanderRadius;
        wanderTarget = transform.position + new Vector3(randomDir.x, randomDir.y, 0f);
    }

    private void RotateFish()
    {
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            RotateTowards(agent.velocity.normalized);
        }
    }

    private void RotateTowards(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }
}
