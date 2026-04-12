using UnityEngine;

public class Fish_M : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float viewAngle = 90f;
    public float speed = 2.5f;        // ว่ายไล่ผู้เล่น
    public float turnSpeed = 3f;
    public float swayAmount = 0.05f;  // sway เล็ก ๆ

    [Header("Attack Settings")]
    public float detectDistance = 6f;  // เริ่มไล่เมื่อผู้เล่นเข้ามา
    public float attackRange = 1.5f;   // ระยะกัด
    public float attackCooldown = 2f;  // เวลาระหว่างกัด
    private float attackTimer = 0f;

    [Header("Wander Settings")]
    public float wanderChangeInterval = 2f;
    private float wanderTimer = 0f;

    private Vector2 direction;
    private bool isFollowing = false;

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

        // ตรวจว่าผู้เล่นอยู่ในระยะ detectDistance
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
        Vector2 targetDirection = (player.position - transform.position).normalized;
        // หมุนไปยังผู้เล่นแบบสมูท
        direction = Vector2.Lerp(direction, targetDirection, turnSpeed * Time.deltaTime).normalized;

        // เพิ่ม sway เล็ก ๆ
        Vector2 sway = new Vector2(Mathf.Sin(Time.time), Mathf.Cos(Time.time)) * swayAmount;
        Vector2 finalDir = (direction + sway).normalized;

        transform.Translate(finalDir * speed * Time.deltaTime, Space.World);

        // ถ้าเข้าใกล้ผู้เล่นมากพอ ให้กัด
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange && attackTimer <= 0f)
        {
            Debug.Log("Player ถูกกัดโดย ChasingFish!");
            attackTimer = attackCooldown;
        }
    }

    void Wander()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            direction = Random.insideUnitCircle.normalized;
            wanderTimer = wanderChangeInterval;
        }

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