using UnityEngine;

public class Fish : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float viewAngle = 90f;
    public float speed = 2f;
    public float boostSpeed = 7f;
    public float changeDirectionTime = 3f;
    public float turnSpeed = 3f;

    [Header("Detection")]
    public float detectDistance = 3f;

    [Header("Flee")]
    public float fleeDuration = 4f;

    [Header("Flocking")]
    public float neighborRadius = 2f;
    public float separationDistance = 0.5f;
    public float flockWeight = 1f;

    [Header("Leader")]
    public bool isLeader = false;

    [Header("Group")]
    public int groupID = 0; // ⭐ เพิ่มตรงนี้

    private Vector2 direction;
    private Vector2 targetDirection;
    private float timer;

    private bool isFleeing = false;
    private float fleeTimer = 0f;

    void Start()
    {
        PickRandomDirection();
    }

    void Update()
    {
        if (player == null) return;

        if (isLeader)
        {
            LeaderLogic();
        }
        else
        {
            FollowerLogic();
        }

        RotateFish();
    }

    // 🧠 Leader คุมทั้งฝูง
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
            Flee();

            fleeTimer -= Time.deltaTime;
            if (fleeTimer <= 0f)
            {
                isFleeing = false;
            }
        }
        else
        {
            Wander();
        }
    }

    // 🐟 ตัวตาม
    void FollowerLogic()
    {
        Fish leader = GetLeader();

        if (leader != null && leader.isFleeing)
        {
            direction = Vector2.Lerp(direction, leader.direction, turnSpeed * Time.deltaTime);
            transform.Translate(direction * boostSpeed * Time.deltaTime, Space.World);
        }
        else
        {
            Wander();
        }
    }

    // 🔴 เริ่มหนี
    void StartFlee()
    {
        isFleeing = true;
        fleeTimer = fleeDuration;

        targetDirection = (transform.position - player.position).normalized;
        targetDirection += Random.insideUnitCircle * 0.3f;
        targetDirection.Normalize();
    }

    // 🔴 หนี
    void Flee()
    {
        Vector2 sway = new Vector2(
            Mathf.Sin(Time.time * 4f),
            Mathf.Cos(Time.time * 4f)
        ) * 0.3f;

        Vector2 finalTarget = targetDirection + sway;
        finalTarget.Normalize();

        direction = Vector2.Lerp(direction, finalTarget, turnSpeed * Time.deltaTime);

        transform.Translate(direction * boostSpeed * Time.deltaTime, Space.World);
    }

    // 🟢 ว่าย + ฝูง
    void Wander()
    {
        timer += Time.deltaTime;

        if (timer >= changeDirectionTime)
        {
            PickRandomDirection();
            timer = 0f;
        }

        Vector2 flockDir = GetFlockDirection();
        direction += flockDir * flockWeight;
        direction.Normalize();

        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    // 🐟 หา Leader (เฉพาะกลุ่มเดียวกัน)
    Fish GetLeader()
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag("Fish");

        foreach (GameObject f in fishes)
        {
            Fish fish = f.GetComponent<Fish>();

            if (fish.isLeader && fish.groupID == groupID)
                return fish;
        }

        return null;
    }

    // 📢 สั่งหนี (เฉพาะกลุ่มเดียวกัน)
    void BroadcastFlee()
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag("Fish");

        foreach (GameObject f in fishes)
        {
            Fish fish = f.GetComponent<Fish>();

            if (!fish.isLeader && fish.groupID == groupID)
            {
                fish.StartFlee();
            }
        }
    }

    // 🐟 ระบบฝูง (เฉพาะกลุ่มเดียวกัน)
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

            Fish fish = f.GetComponent<Fish>();

            // ⭐ กรองกลุ่ม
            if (fish.groupID != groupID) continue;

            float dist = Vector2.Distance(transform.position, f.transform.position);

            if (dist < neighborRadius)
            {
                center += (Vector2)f.transform.position;
                align += fish.direction;

                if (dist < separationDistance)
                {
                    avoid += (Vector2)(transform.position - f.transform.position);
                }

                count++;
            }
        }

        if (count == 0) return Vector2.zero;

        center /= count;

        Vector2 cohesion = (center - (Vector2)transform.position);
        Vector2 result = cohesion + align + avoid;

        return result.normalized;
    }

    // 🐟 หมุน
    void RotateFish()
    {
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 5f * Time.deltaTime);
        }
    }

    // 🔄 สุ่ม
    void PickRandomDirection()
    {
        direction = Random.insideUnitCircle.normalized;
    }
}