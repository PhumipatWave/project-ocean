using UnityEngine;
using UnityEngine.AI;

public class FishAgent : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float speed = 2f;
    public float boostSpeed = 7f;
    public float changeDirectionTime = 3f;
    public float turnSpeed = 3f;

    [Header("Movement Area")]
    public Vector2 areaSize = new Vector2(10f, 10f);
    public Transform areaCenter;

    private Vector3 currentTarget;

    [Header("Detection")]
    public float detectDistance = 3f;
    public float viewAngle = 90f;

    [Header("Flee")]
    public float fleeDuration = 4f;

    [Header("Flocking")]
    public float neighborRadius = 2f;
    public float separationDistance = 0.5f;
    public float flockWeight = 1f;

    [Header("Leader")]
    public bool isLeader = false;

    [Header("Group")]
    public int groupID = 0;

    private NavMeshAgent agent;

    private Vector2 direction;
    private float timer;

    private bool isFleeing = false;
    private float fleeTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        PickRandomPointInArea();
    }

    void Update()
    {
        Debug.Log(agent.hasPath);
        Debug.Log(agent.velocity);

        if (player == null) return;

        if (isLeader)
            LeaderLogic();
        else
            FollowerLogic();

        RotateFish();
    }

    void OnDrawGizmos()
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

    // Leader
    void LeaderLogic()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        Vector2 toPlayer = (player.position - transform.position).normalized;
        float angle = Vector2.Angle(direction, toPlayer);

        if (!isFleeing && dist < detectDistance && angle < viewAngle / 2f)
        {
            StartFlee();
            BroadcastFlee();
        }

        if (isFleeing)
        {
            fleeTimer -= Time.deltaTime;
            MoveInDirection((transform.position - player.position).normalized, boostSpeed);

            if (fleeTimer <= 0f)
                isFleeing = false;
        }
        else
        {
            Wander();
        }
    }

    // Follower
    void FollowerLogic()
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

    // Start flee
    void StartFlee()
    {
        isFleeing = true;
        fleeTimer = fleeDuration;
    }

    // Wander
    void Wander()
    {
        timer += Time.deltaTime;

        // 🔥 Get flock influence
        Vector2 flockDir = GetFlockDirection();

        // Combine random + flock
        Vector2 finalDir = direction + flockDir * flockWeight;
        finalDir.Normalize();

        // 🔥 Convert direction → position
        Vector3 rawTarget = transform.position + (Vector3)finalDir * 2f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rawTarget, out hit, 2f, NavMesh.AllAreas))
        {
            currentTarget = hit.position;
        }

        // 🔥 Only update destination when needed
        if (!agent.hasPath || agent.remainingDistance < 0.2f || timer >= changeDirectionTime)
        {
            agent.speed = speed;
            agent.SetDestination(currentTarget);
            timer = 0f;
        }

        direction = finalDir;
    }

    // CORE: Move using NavMesh
    void MoveInDirection(Vector2 dir, float moveSpeed)
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

    // Rotation from velocity
    void RotateFish()
    {
        Vector2 vel = agent.velocity;

        if (vel.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, 5f * Time.deltaTime);
        }
    }

    // Leader finder
    FishAgent GetLeader()
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag("Fish");

        foreach (GameObject f in fishes)
        {
            FishAgent fish = f.GetComponent<FishAgent>();
            if (fish != null && fish.isLeader && fish.groupID == groupID)
                return fish;
        }

        return null;
    }

    // Broadcast flee
    void BroadcastFlee()
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag("Fish");

        foreach (GameObject f in fishes)
        {
            FishAgent fish = f.GetComponent<FishAgent>();

            if (fish != null && !fish.isLeader && fish.groupID == groupID)
                fish.StartFlee();
        }
    }

    // Flocking
    Vector2 GetFlockDirection()
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag("Fish");

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

    void PickRandomPointInArea()
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
