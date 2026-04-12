using Game.Testing;
using UnityEngine;
using UnityEngine.AI;

public class FishAgent : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Enemy enemy;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float boostSpeed = 7f;
    [SerializeField] private float changeDirectionTime = 3f;
    [SerializeField] private float turnSpeed = 3f;

    [Header("Movement Area")]
    [SerializeField] private Vector2 areaSize = new Vector2(10f, 10f);
    [SerializeField] private Transform areaCenter;

    private Vector3 currentTarget;

    [Header("Detection")]
    [SerializeField] private float detectDistance = 3f;
    [SerializeField] private float viewAngle = 90f;

    [Header("Flee")]
    [SerializeField] private float fleeDuration = 4f;

    [Header("Flocking")]
    [SerializeField] private float neighborRadius = 2f;
    [SerializeField] private float separationDistance = 0.5f;
    [SerializeField] private float flockWeight = 1f;

    [Header("Group")]
    [SerializeField] private string groupTagName = "EnemyGroup";

    [SerializeField] private bool isLeader = false;
    [SerializeField] private int groupID = 0;

    private NavMeshAgent agent;

    private Vector2 direction;
    private float timer;

    private bool isFleeing = false;
    private float fleeTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemy = GetComponent<Enemy>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

    }

    private void Start()
    {
        PickRandomPointInArea();
    }

    private void Update()
    {
        Debug.Log(agent.hasPath);
        Debug.Log(agent.velocity);

        if (enemy.Player == null || enemy.IsDeath) 
            return;

        if (isLeader)
            LeaderLogic();
        else
            FollowerLogic();

        RotateFish();
    }

    private void OnDrawGizmos()
    {
        // Draw movement area
        Gizmos.color = Color.cyan;

        Vector3 center = areaCenter != null ? areaCenter.position : Vector3.zero;

        Gizmos.DrawWireCube(center, new Vector3(areaSize.x, areaSize.y, 0));

        // Draw current target
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(currentTarget, 0.2f);

        // Draw direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * 2f);
    }

    private void LeaderLogic()
    {
        float dist = Vector2.Distance(transform.position, enemy.Player.transform.position);
        Vector2 toPlayer = (enemy.Player.transform.position - transform.position).normalized;
        float angle = Vector2.Angle(direction, toPlayer);

        if (!isFleeing && dist < detectDistance && angle < viewAngle / 2f)
        {
            StartFlee();
            BroadcastFlee();
        }

        if (isFleeing)
        {
            fleeTimer -= Time.deltaTime;
            MoveInDirection((transform.position - enemy.Player.transform.position).normalized, boostSpeed);

            if (fleeTimer <= 0f)
                isFleeing = false;
        }
        else
        {
            Wander();
        }
    }

    private void FollowerLogic()
    {
        FishAgent leader = GetLeader();

        if (leader != null && leader.isFleeing)
        {
            direction = Vector2.Lerp(direction, leader.direction, turnSpeed * Time.deltaTime);
            MoveInDirection(direction, boostSpeed);
        }
        else
        {
            Wander();
        }
    }

    private void StartFlee()
    {
        isFleeing = true;
        fleeTimer = fleeDuration;
    }

    private void Wander()
    {
        timer += Time.deltaTime;

        // Get flock influence
        Vector2 flockDir = GetFlockDirection();

        // Combine random + flock
        Vector2 finalDir = direction + flockDir * flockWeight;
        finalDir.Normalize();

        // Convert direction → position
        Vector3 rawTarget = transform.position + (Vector3)finalDir * 2f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rawTarget, out hit, 2f, NavMesh.AllAreas))
        {
            currentTarget = hit.position;
        }

        // Only update destination when needed
        if (!agent.hasPath || agent.remainingDistance < 0.2f || timer >= changeDirectionTime)
        {
            agent.speed = speed;
            agent.SetDestination(currentTarget);
            timer = 0f;
        }

        direction = finalDir;
    }

    private void MoveInDirection(Vector2 dir, float moveSpeed)
    {
        agent.speed = moveSpeed;

        Vector3 rawTarget = transform.position + (Vector3)dir * 1.5f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rawTarget, out hit, 2f, NavMesh.AllAreas))
        {
            currentTarget = hit.position;

            // Only update if needed
            if (!agent.hasPath || agent.remainingDistance < 0.2f)
            {
                agent.SetDestination(currentTarget);
            }
        }
    }

    private void RotateFish()
    {
        Vector2 vel = agent.velocity;

        if (vel.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, 5f * Time.deltaTime);
        }
    }

    private FishAgent GetLeader()
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag(groupTagName);

        foreach (GameObject f in fishes)
        {
            FishAgent fish = f.GetComponent<FishAgent>();
            if (fish != null && fish.isLeader && fish.groupID == groupID)
                return fish;
        }

        return null;
    }

    private void BroadcastFlee()
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag(groupTagName);

        foreach (GameObject f in fishes)
        {
            FishAgent fish = f.GetComponent<FishAgent>();

            if (fish != null && !fish.isLeader && fish.groupID == groupID)
                fish.StartFlee();
        }
    }

    private Vector2 GetFlockDirection()
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag(groupTagName);

        Vector2 center = Vector2.zero;
        Vector2 avoid = Vector2.zero;
        Vector2 align = Vector2.zero;

        int count = 0;

        foreach (GameObject f in fishes)
        {
            if (f == gameObject) continue;

            FishAgent fish = f.GetComponent<FishAgent>();
            if (fish == null || fish.groupID != groupID) continue;

            float dist = Vector2.Distance(transform.position, f.transform.position);

            if (dist < neighborRadius)
            {
                center += (Vector2)f.transform.position;
                align += fish.direction;

                if (dist < separationDistance)
                    avoid += (Vector2)(transform.position - f.transform.position);

                count++;
            }
        }

        if (count == 0) return Vector2.zero;

        center /= count;
        Vector2 cohesion = (center - (Vector2)transform.position);

        return (cohesion + align + avoid).normalized;
    }

    private void PickRandomPointInArea()
    {
        Vector3 center = areaCenter != null ? areaCenter.position : transform.position;

        for (int i = 0; i < 10; i++)
        {
            float randomX = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
            float randomY = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);

            Vector3 randomPoint = new Vector3(
                center.x + randomX,
                center.y + randomY,
                0f
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 3f, NavMesh.AllAreas))
            {
                currentTarget = hit.position;
                return;
            }
        }

        // fallback
        currentTarget = transform.position;
    }
}
