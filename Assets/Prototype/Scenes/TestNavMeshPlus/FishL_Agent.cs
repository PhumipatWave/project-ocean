using UnityEngine;
using UnityEngine.AI;

public class FishL_Agent : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float speed = 2f;
    public float boostSpeed = 7f;
    public float turnSpeed = 5f;
    public float swayAmount = 0.1f;

    [Header("Wander Settings")]
    public float wanderRadius = 3f;
    public float wanderChangeInterval = 2f;
    private float wanderTimer;

    [Header("Attack Settings")]
    public float detectDistance = 5f;
    public float attackRange = 1.5f;
    public float prepareTime = 2f;
    public float attackCooldown = 2f;
    public float followAfterLost = 6f;

    private NavMeshAgent agent;

    private bool isPreparing = false;
    private bool isAttacking = false;
    private bool isFollowing = false;

    private float prepareTimer = 0f;
    private float attackTimer = 0f;
    private float followTimer = 0f;

    private Vector3 wanderTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        wanderTimer = wanderChangeInterval;
        PickNewWanderPoint();
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

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

    void StartPrepare()
    {
        isPreparing = true;
        prepareTimer = prepareTime;

        agent.isStopped = true; // stop moving while aiming
    }

    void Prepare()
    {
        prepareTimer -= Time.deltaTime;

        // Look at player
        Vector2 dir = (player.position - transform.position).normalized;
        RotateTowards(dir);

        if (prepareTimer <= 0f)
        {
            isPreparing = false;
            isAttacking = true;

            agent.isStopped = false;
            agent.speed = boostSpeed;
        }
    }

    void Attack()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            Debug.Log("Player hit by fish!");
            StopAttack();
            return;
        }

        // Add sway to make it feel alive
        Vector2 sway = new Vector2(Mathf.Sin(Time.time * 2f), Mathf.Cos(Time.time * 2f)) * swayAmount;
        Vector3 target = player.position + (Vector3)sway;

        agent.SetDestination(target);

        if (dist > detectDistance)
        {
            isAttacking = false;
            isFollowing = true;
            followTimer = followAfterLost;
        }
    }

    void StopAttack()
    {
        isAttacking = false;
        attackTimer = attackCooldown;
        agent.speed = speed;
    }

    void Follow()
    {
        agent.speed = speed;
        agent.SetDestination(player.position);

        float dist = Vector2.Distance(transform.position, player.position);

        if (followTimer <= 0f || dist > detectDistance + 10f)
        {
            isFollowing = false;
        }
    }

    void Wander()
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

    void PickNewWanderPoint()
    {
        Vector2 randomDir = Random.insideUnitCircle * wanderRadius;
        wanderTarget = transform.position + new Vector3(randomDir.x, randomDir.y, 0f);
    }

    void RotateFish()
    {
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            RotateTowards(agent.velocity.normalized);
        }
    }

    void RotateTowards(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0, 0, angle);

        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }
}
