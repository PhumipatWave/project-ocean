using UnityEngine;

public class WaterShadowWiggle : MonoBehaviour
{
    [Header("Wiggle Settings")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float moveAmount = 0.08f;

    [Header("Scale Pulse Settings")]
    [SerializeField] private float scaleSpeed = 2f;
    [SerializeField] private float scaleAmount = 0.05f; // ยืดหดนิดเดียวพอพิกเซลไม่เพี้ยน

    private Vector3 startPosition;
    private Vector3 startScale;

    private void Awake()
    {
        startPosition = transform.localPosition;
        startScale = transform.localScale;
    }

    private void Update()
    {
        // 1. คำนวณการพริ้วซ้ายขวาตามกระแสน้ำ
        float moveX = Mathf.Sin(Time.time * moveSpeed) * moveAmount;
        transform.localPosition = new Vector3(startPosition.x + moveX, startPosition.y, startPosition.z);

        // 2. คำนวณการยืดหดขยายตัวของแสงเงาใต้น้ำ (สร้างความฟุ้งวูบวาบ)
        float pulse = Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;
        transform.localScale = new Vector3(startScale.x + pulse, startScale.y + (pulse * 0.5f), startScale.z);
    }
}
