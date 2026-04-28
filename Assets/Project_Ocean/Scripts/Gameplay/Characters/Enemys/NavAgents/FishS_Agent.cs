using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FishS_Agent : MonoBehaviour
{
    [Header("Fish Settings")]
    public float speed = 2.5f;
    public float turnSpeed = 5f;
    public float groupRadius = 3f;
    public float separationDistance = 1f;
    public List<FishS_Agent> neighbors = new List<FishS_Agent>();

    [Header("Leader Settings")]
    public FishS_Agent leader; // null = leader
    public int groupID = 0;

    [Header("Sensor Settings")]
    public float viewDistance = 6f;
    public float viewAngle = 90f;

    [Header("Wander Settings")]
    public float wanderRadius = 3f;
    public float wanderInterval = 3f;

    [Header("Flocking Weights")]
    public float cohesionWeight = 1f;       // ดึงเข้าหาศูนย์กลางกลุ่ม
    public float separationWeight = 1.5f;   // ผลักออกเมื่อใกล้เกินไป
    public float alignmentWeight = 0.5f;    // หันหน้าตามทิศกลุ่ม

    [Header("Panic Flee")]
    public float panicSpeedMultiplier = 2.5f;  // speed boost ตอน panic
    public float panicDuration = 3f;           // นานแค่ไหนก่อน speed กลับปกติ
    public float panicRandomAngle = 60f;       // องศาสุ่มทิศหนี (0=ตรง, 180=สุ่มเต็ม)

    [Header("Wall Avoid")]
    public float wallDetectDistance = 2f;      // เพิ่มให้ detect ไวขึ้น
    public float wallAvoidWeight = 3f;         // น้ำหนักการหลีกกำแพง
    public LayerMask wallLayer;

    [Header("Hit Receiver")]
    public string playerBulletTag = "Bullet";
    public string playerTag = "Player";
    public bool destroyBulletOnHit = true;
    public float hitChaseDuration = 5f;

    [Header("Rotation/Flip Settings")]
    public float turnSmoothTime = 0.25f;
    public float maxTiltAngle = 360f;
    public float flipDeadZone = 0.1f;
    public float flipDelay = 0.4f;
    public float flipCooldown = 0.5f;
    public float spriteAngleOffset = 0f;

    private Vector2 currentDirection;
    private NavMeshAgent agent;
    private Vector2 wanderTarget;
    private float wanderTimer = 0f;
    private float hitChaseTimer = 0f;

    private bool facingRight = true;
    private float turnVelocity;
    private float flipTimer = 0f;
    private float flipCooldownTimer = 0f;

    private Transform playerTransform;

    private float neighborUpdateInterval = 0.2f;
    private float neighborTimer = 0f;

    private const float NAV_DESTINATION_DISTANCE = 3f;

    // Panic state
    private bool isPanicking = false;
    private float panicTimer = 0f;
    private Vector2 panicDirection = Vector2.zero;

    // Wall Bounce state
    private bool isBouncing = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;
        SetNewWanderTarget();

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    private void Update()
    {
        if (isBouncing) return; // บล็อก Update ชั่วคราวให้ WallBounceRoutine ทำงาน

        neighborTimer -= Time.deltaTime;
        if (neighborTimer <= 0f)
        {
            UpdateNeighbors();
            neighborTimer = neighborUpdateInterval;
        }

        // ── Panic timer ───────────────────────────────────────────
        if (isPanicking)
        {
            panicTimer -= Time.deltaTime;
            if (panicTimer <= 0f)
                StopPanic();
        }

        Vector2 moveDir = Vector2.zero;
        bool playerNearby = false;

        // ── Player detection → Panic Flee ─────────────────────────
        if (playerTransform != null && CanSeePlayer(playerTransform))
        {
            playerNearby = true;

            if (!isPanicking)
                StartPanic(playerTransform.position);

            moveDir += panicDirection * 3f;

            // Leader broadcast ให้ follower panic ด้วย
            if (leader == null)
            {
                foreach (var fish in neighbors)
                {
                    if (fish.leader == this && fish.groupID == groupID)
                        fish.FleeFrom(playerTransform.position);
                }
            }
        }

        // ── ยังคง panic อยู่แม้ไม่เห็น player แล้ว ────────────────
        if (!playerNearby && isPanicking)
        {
            moveDir += panicDirection * 2f;
        }

        // ── Hit reaction ─────────────────────────────────────────
        if (hitChaseTimer > 0f)
        {
            hitChaseTimer -= Time.deltaTime;
            if (playerTransform != null)
            {
                Vector2 tempFlee = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
                moveDir += tempFlee * 3f;
            }
        }

        // ── Follower follow leader หรือ wander ถ้าหัวหน้าออกฝูง ────
        if (!playerNearby && !isPanicking && leader != null)
        {
            bool leaderNearby = neighbors.Contains(leader);

            if (leaderNearby)
            {
                Vector2 toLeader = (Vector2)leader.transform.position - (Vector2)transform.position;
                moveDir += toLeader.normalized;
            }
            else
            {
                // หัวหน้าออกฝูงไปแล้ว — wander เองเหมือนปลาอิสระ
                wanderTimer -= Time.deltaTime;
                if (wanderTimer <= 0f || Vector2.Distance(transform.position, wanderTarget) < 0.5f)
                    SetNewWanderTarget();

                Vector2 toTarget = wanderTarget - (Vector2)transform.position;
                moveDir += toTarget.normalized;
            }
        }

        // ── Leader wander ─────────────────────────────────────────
        if (!playerNearby && !isPanicking && leader == null && hitChaseTimer <= 0f)
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f || Vector2.Distance(transform.position, wanderTarget) < 0.5f)
                SetNewWanderTarget();

            Vector2 toTarget = wanderTarget - (Vector2)transform.position;
            moveDir += toTarget.normalized;
        }

        // ── Flocking: Cohesion + Separation + Alignment ───────────
        moveDir += CalcFlocking();

        // ── Wall Avoidance ────────────────────────────────────────
        moveDir += WallAvoidance();

        // ── Move ──────────────────────────────────────────────────
        if (moveDir.sqrMagnitude > 0.01f)
        {
            currentDirection = moveDir.normalized;
            agent.SetDestination((Vector3)transform.position + (Vector3)currentDirection * NAV_DESTINATION_DISTANCE);
        }

        RotateFish(currentDirection);
    }

    // ─────────────────────────────────────────────────────────────
    // Panic Flee
    // ─────────────────────────────────────────────────────────────

    private void StartPanic(Vector2 playerPos)
    {
        isPanicking = true;
        panicTimer = panicDuration;

        Vector2 awayFromPlayer = ((Vector2)transform.position - playerPos).normalized;
        float randomAngle = Random.Range(-panicRandomAngle, panicRandomAngle);
        panicDirection = RotateVector(awayFromPlayer, randomAngle);

        agent.speed = speed * panicSpeedMultiplier;
    }

    private void StopPanic()
    {
        isPanicking = false;
        panicTimer = 0f;
        agent.speed = speed;
    }

    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }

    // ─────────────────────────────────────────────────────────────
    // Flocking — Boid 3 Rules
    // ─────────────────────────────────────────────────────────────

    private Vector2 CalcFlocking()
    {
        if (neighbors.Count == 0) return Vector2.zero;

        Vector2 cohesionSum   = Vector2.zero;
        Vector2 separationSum = Vector2.zero;
        Vector2 alignmentSum  = Vector2.zero;
        int separationCount   = 0;

        foreach (var neighbor in neighbors)
        {
            Vector2 toNeighbor = (Vector2)neighbor.transform.position - (Vector2)transform.position;
            float dist = toNeighbor.magnitude;

            cohesionSum += (Vector2)neighbor.transform.position;

            if (dist < separationDistance && dist > 0f)
            {
                separationSum += -toNeighbor.normalized / dist;
                separationCount++;
            }

            alignmentSum += neighbor.currentDirection;
        }

        Vector2 result = Vector2.zero;

        Vector2 centerOfMass = cohesionSum / neighbors.Count;
        Vector2 toCentre = centerOfMass - (Vector2)transform.position;
        if (toCentre.sqrMagnitude > 0.01f)
            result += toCentre.normalized * cohesionWeight;

        if (separationCount > 0)
            result += (separationSum / separationCount).normalized * separationWeight;

        Vector2 avgDir = alignmentSum / neighbors.Count;
        if (avgDir.sqrMagnitude > 0.01f)
            result += avgDir.normalized * alignmentWeight;

        return result;
    }

    // ─────────────────────────────────────────────────────────────
    // Neighbors
    // ─────────────────────────────────────────────────────────────

    private void UpdateNeighbors()
    {
        neighbors.Clear();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, groupRadius);
        foreach (var hit in hits)
        {
            if (hit != null && hit != GetComponent<Collider2D>())
            {
                FishS_Agent fish = hit.GetComponent<FishS_Agent>();
                if (fish != null && fish.groupID == groupID)
                    neighbors.Add(fish);
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Wander
    // ─────────────────────────────────────────────────────────────

    private void SetNewWanderTarget()
    {
        Vector2 randomDir = Random.insideUnitCircle * wanderRadius;
        wanderTarget = (Vector2)transform.position + randomDir;
        wanderTimer = wanderInterval;
    }

    // ─────────────────────────────────────────────────────────────
    // Sensors
    // ─────────────────────────────────────────────────────────────

    private bool CanSeePlayer(Transform target)
    {
        Vector2 toPlayer = (Vector2)target.position - (Vector2)transform.position;
        if (toPlayer.magnitude > viewDistance) return false;

        Vector2 checkDir = currentDirection.sqrMagnitude > 0.01f ? currentDirection : toPlayer.normalized;
        float angle = Vector2.Angle(checkDir, toPlayer);
        if (angle > viewAngle / 2f) return false;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer, toPlayer.magnitude, wallLayer);
        if (hit.collider != null && hit.transform != target) return false;

        return true;
    }

    // ─────────────────────────────────────────────────────────────
    // Wall Avoidance — Raycast 8 ทิศ + hit.normal + scale ตาม speed
    // ─────────────────────────────────────────────────────────────

    private Vector2 WallAvoidance()
    {
        if (currentDirection.sqrMagnitude < 0.01f) return Vector2.zero;

        Vector2 avoidDir = Vector2.zero;

        // scale detect distance ตาม speed ปัจจุบัน — ยิ่งเร็วยิ่ง detect ไกล
        float currentSpeed = agent.speed;
        float detectDist = wallDetectDistance * (currentSpeed / speed);

        // Raycast 8 ทิศรอบตัว ครอบคลุมทุกมุม
        float[] angles = { 0f, 45f, 90f, 135f, 180f, -45f, -90f, -135f };

        foreach (float a in angles)
        {
            Vector2 dir = RotateVector(currentDirection, a);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, detectDist, wallLayer);

            if (hit.collider != null)
            {
                // ใช้ hit.normal แทน Perpendicular — ผลักออกจากผนังในทิศที่ถูกต้องเสมอ
                float proximity = 1f - (hit.distance / detectDist); // ยิ่งใกล้ยิ่งผลักแรง
                avoidDir += hit.normal * proximity;
            }
        }

        return avoidDir.sqrMagnitude > 0.01f ? avoidDir.normalized * wallAvoidWeight : Vector2.zero;
    }

    // ─────────────────────────────────────────────────────────────
    // Rotation & Sprite Flip
    // ─────────────────────────────────────────────────────────────

    private void RotateFish(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.01f) return;

        if (flipCooldownTimer > 0f)
            flipCooldownTimer -= Time.deltaTime;

        bool wantsFaceRight = facingRight;
        if (dir.x >  flipDeadZone) wantsFaceRight = true;
        else if (dir.x < -flipDeadZone) wantsFaceRight = false;

        if (wantsFaceRight != facingRight)
        {
            flipTimer += Time.deltaTime;
            if (flipTimer >= flipDelay && flipCooldownTimer <= 0f)
            {
                facingRight = wantsFaceRight;
                flipTimer = 0f;
                flipCooldownTimer = flipCooldown;
                turnVelocity = 0f;
            }
        }
        else flipTimer = 0f;

        float targetAngle;
        if (facingRight)
        {
            transform.localScale = new Vector3( Mathf.Abs(transform.localScale.x),
                                                Mathf.Abs(transform.localScale.y),
                                                transform.localScale.z);
            targetAngle = Mathf.Atan2(dir.y, Mathf.Abs(dir.x)) * Mathf.Rad2Deg;
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x),
                                                Mathf.Abs(transform.localScale.y),
                                                transform.localScale.z);
            targetAngle = Mathf.Atan2(-dir.y, Mathf.Abs(dir.x)) * Mathf.Rad2Deg;
        }

        targetAngle = Mathf.Clamp(targetAngle, -maxTiltAngle, maxTiltAngle);
        targetAngle += spriteAngleOffset;

        float smoothTime = 1f / Mathf.Max(turnSpeed, 0.01f);
        float smoothAngle = Mathf.SmoothDampAngle(
            transform.eulerAngles.z,
            targetAngle,
            ref turnVelocity,
            smoothTime
        );

        transform.rotation = Quaternion.Euler(0f, 0f, smoothAngle);
    }

    // ─────────────────────────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────────────────────────

    public void FleeFrom(Vector2 targetPos)
    {
        if (!isPanicking)
            StartPanic(targetPos);

        Vector2 fleeDir = ((Vector2)transform.position - targetPos).normalized;
        currentDirection = fleeDir;
        agent.SetDestination((Vector3)transform.position + (Vector3)currentDirection * NAV_DESTINATION_DISTANCE);
        hitChaseTimer = hitChaseDuration;
    }

    // ─────────────────────────────────────────────────────────────
    // Collision
    // ─────────────────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerBulletTag))
        {
            if (destroyBulletOnHit) Destroy(collision.gameObject);
            FleeFrom(collision.transform.position);
        }

        if (collision.CompareTag(playerTag))
        {
            FleeFrom(collision.transform.position);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
{
    // ตรวจสอบว่าชนกำแพง (wallLayer) หรือไม่
    if (((1 << collision.gameObject.layer) & wallLayer) == 0) return;

    // กำหนดให้หันหน้าออกจากกำแพง (Reflect + Random)
    Vector2 normal = collision.contacts[0].normal;
    currentDirection = Vector2.Reflect(currentDirection, normal).normalized;

    // ถ้ากำลัง panic อยู่ ให้สุ่มทิศทางหนีใหม่
    if (isPanicking)
    {
        float randomAngle = Random.Range(-panicRandomAngle, panicRandomAngle);
        panicDirection = RotateVector(currentDirection, randomAngle).normalized;
    }

    // เรียก Coroutine ปรับทิศทางออกจากกำแพง
    StopAllCoroutines();
    StartCoroutine(WallBounceRoutine());
}

private System.Collections.IEnumerator WallBounceRoutine()
{
    isBouncing = true;

    int attempts = 0;
    bool freeFromWall = false;

    while (attempts < 10 && !freeFromWall)
    {
        // ตรวจสอบว่าทิศทางปัจจุบันชนกำแพงหรือไม่
        RaycastHit2D hit = Physics2D.Raycast(transform.position, currentDirection, wallDetectDistance, wallLayer);
        if (hit.collider == null)
        {
            freeFromWall = true; // ออกจากกำแพงได้แล้ว
            break;
        }

        // หมุนทิศทางออกกำแพงมากขึ้น
        float angle = Random.Range(45f, 135f);
        currentDirection = RotateVector(currentDirection, angle).normalized;

        attempts++;
        yield return null;
    }

    // อัปเดต NavMeshAgent destination หลังปรับทิศแล้ว
    agent.SetDestination((Vector3)transform.position + (Vector3)currentDirection * NAV_DESTINATION_DISTANCE);

    isBouncing = false; // ปล่อยให้ Update ทำงานต่อ
}
    

    

}
