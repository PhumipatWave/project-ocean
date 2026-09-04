using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("Parallax Speed (0 = Follow Cam, 1 = Static)")]
    [Tooltip("0 = วิ่งตามกล้องเป๊ะ, 0.5 = วิ่งครึ่งหนึ่ง, 0.9 = วิ่งช้ามากฉากไกล")]
    [SerializeField] private Vector2 parallaxMultiplier;

    [Header("Infinite Loop Settings")]
    [SerializeField] private bool infiniteHorizontal;
    private float totalWidth;
    private float startPosX;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    private void Awake()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            lastCameraPosition = cameraTransform.position;
        }

        startPosX = transform.position.x;

        // คำนวณความกว้างรวมของวัตถุลูกทั้งหมดอัตโนมัติ เพื่อใช้วันลูป
        SpriteRenderer[] childSprites = GetComponentsInChildren<SpriteRenderer>();
        if (childSprites.Length > 0)
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float singleSpriteWidth = 0;

            foreach (var spriteRenderer in childSprites)
            {
                if (spriteRenderer.sprite != null)
                {
                    float width = spriteRenderer.sprite.texture.width / spriteRenderer.sprite.pixelsPerUnit;
                    singleSpriteWidth = Mathf.Max(singleSpriteWidth, width);
                    minX = Mathf.Min(minX, spriteRenderer.transform.position.x);
                    maxX = Mathf.Max(maxX, spriteRenderer.transform.position.x);
                }
            }

            // ระยะห่างระหว่างตัวแรกถึงตัวสุดท้าย + ความกว้างของตัวมันเอง
            totalWidth = (maxX - minX) + singleSpriteWidth;
        }
    }

    private void LateUpdate()
    {
        if (cameraTransform == null || totalWidth <= 0) return;

        // 1. คำนวณระยะที่กล้องขยับในเฟรมนี้
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // 2. ขยับตัวแม่ตามค่าความเร็ว Parallax
        transform.position += new Vector3(
            deltaMovement.x * parallaxMultiplier.x,
            deltaMovement.y * parallaxMultiplier.y,
            0
        );

        lastCameraPosition = cameraTransform.position;

        // 3. ระบบ Infinite Loop วนลูปภาพผืนน้ำเมื่อหลุดขอบกล้อง
        if (infiniteHorizontal)
        {
            float relativeCamPos = cameraTransform.position.x * (1 - parallaxMultiplier.x);
            float dist = cameraTransform.position.x - startPosX;

            if (dist > totalWidth / 2)
            {
                startPosX += totalWidth;
            }
            else if (dist < -totalWidth / 2)
            {
                startPosX -= totalWidth;
            }
        }
    }
}
