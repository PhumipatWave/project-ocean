using UnityEngine;

public class Bird : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float speed = 2f;

    private Transform targetDestination;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // เริ่มต้นให้เดินไปหาจุด A
        targetDestination = pointA;
    }

    void Update()
    {
        Move();
        FlipCharacter();
    }

    void Move()
    {
        // เคลื่อนที่เข้าหาจุดหมายด้วยความเร็วคงที่
        transform.position = Vector3.MoveTowards(transform.position, targetDestination.position, speed * Time.deltaTime);

        // ถ้าเดินถึงจุดหมาย (ระยะห่างน้อยกว่า 0.1) ให้สลับไปอีกจุดหนึ่ง
        if (Vector2.Distance(transform.position, targetDestination.position) < 0.1f)
        {
            targetDestination = (targetDestination == pointA) ? pointB : pointA;
        }
    }

    void FlipCharacter()
    {
        // เช็คตำแหน่งจุดหมายเทียบกับตัวละครเพื่อหันหน้าให้ถูกทาง
        if (targetDestination.position.x > transform.position.x)
        {
            spriteRenderer.flipX = false; // หันขวา
        }
        else if (targetDestination.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true; // หันซ้าย
        }
    }
}