using UnityEngine;

public class AggressiveFish : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float viewAngle = 90f;
    public float speed = 2f;
    public float boostSpeed = 7f;
    public float turnSpeed = 3f;
    public float swayAmount = 0.1f;

    [Header("Wander Settings")]
    public float wanderChangeInterval = 2f; // เปลี่ยนทิศทางทุก 2 วิ
    private float wanderTimer = 0f;

    [Header("Attack Settings")]
    public float detectDistance = 5f;
    public float attackRange = 1.5f;
    public float prepareTime = 2f;
    public float attackCooldown = 2f;
    public float followAfterLost = 6f;

    
    private Vector2 direction;
    private Vector2 targetDirection;

    private bool isPreparing = false;
    private bool isAttacking = false;
    private bool isFollowing = false;
    private float prepareTimer = 0f;
    private float attackTimer = 0f;
    private float followTimer = 0f;

    void Start()
    {
        direction = Random.insideUnitCircle.normalized;
        wanderTimer = wanderChangeInterval;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // ลด cooldown
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
        if (followTimer > 0f) followTimer -= Time.deltaTime;

        // เริ่ม Phase เตรียมโจมตี ถ้าอยู่ใน detectDistance และเกิน attackRange
        if (!isPreparing && !isAttacking && dist < detectDistance && dist > attackRange && attackTimer <= 0f)
        {
            StartPrepare();
        }

        // ถ้าผู้เล่นหลุด ให้ตามต่ออีก followAfterLost วิ
        if (!isPreparing && !isAttacking && dist > detectDistance && followTimer <= 0f)
        {
            isFollowing = true;
            followTimer = followAfterLost;
            targetDirection = (player.position - transform.position).normalized;
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
        targetDirection = (player.position - transform.position).normalized;
    }

    void Prepare()
    {
        // หมุนไปหาผู้เล่น
        direction = Vector2.Lerp(direction, targetDirection, turnSpeed * Time.deltaTime).normalized;
        prepareTimer -= Time.deltaTime;

        if (prepareTimer <= 0f)
        {
            isPreparing = false;
            isAttacking = true;
        }
    }

    void Attack()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        // ถ้าผู้เล่นอยู่ใกล้เกินไป ให้ยกเลิก Attack
        if (dist <= attackRange)
        {
            isAttacking = false;
            attackTimer = attackCooldown;
            return;
        }

        // เพิ่ม sway เล็ก ๆ
        Vector2 sway = new Vector2(Mathf.Sin(Time.time * 2f), Mathf.Cos(Time.time * 2f)) * swayAmount;
        Vector2 finalTarget = (targetDirection + sway).normalized;
        direction = Vector2.Lerp(direction, finalTarget, turnSpeed * Time.deltaTime).normalized;

        transform.Translate(direction * boostSpeed * Time.deltaTime, Space.World);

        // ตรวจระยะโจมตี
        if (dist < attackRange)
        {
            Debug.Log("Player ถูกโจมตีโดยปลา!");
            attackTimer = attackCooldown;
            isAttacking = false;
        }

        // ถ้าผู้เล่นหลุดออก detectDistance
        if (dist > detectDistance)
        {
            isAttacking = false;
            isFollowing = true;
            followTimer = followAfterLost;
        }
    }

    void Follow()
    {
        targetDirection = (player.position - transform.position).normalized;
        direction = Vector2.Lerp(direction, targetDirection, turnSpeed * Time.deltaTime).normalized;

        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        float dist = Vector2.Distance(transform.position, player.position);
        if (followTimer <= 0f || dist > detectDistance + 10f)
        {
            isFollowing = false;
        }
    }

    void Wander()
    {
        // เปลี่ยนทิศทางแบบสุ่มทุก wanderChangeInterval
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            direction = Random.insideUnitCircle.normalized;
            wanderTimer = wanderChangeInterval;
        }

        // เพิ่ม sway เล็ก ๆ เพื่อให้เหมือนว่ายน้ำ
        Vector2 sway = new Vector2(Mathf.Sin(Time.time), Mathf.Cos(Time.time)) * swayAmount;
        Vector2 finalDir = (direction + sway).normalized;

        transform.Translate(finalDir * speed * Time.deltaTime, Space.World);
    }

    void RotateFish()
    {
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), 5f * Time.deltaTime);
        }
    }
}