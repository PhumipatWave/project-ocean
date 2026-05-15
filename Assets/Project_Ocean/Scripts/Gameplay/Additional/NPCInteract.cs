using UnityEngine;
using UnityEngine.UI;
using TMPro; // ถ้าใช้ TextMeshPro อย่าลืมใส่บรรทัดนี้

public class NPCInteract : MonoBehaviour
{
    [Header("NPC Settings")]
    [TextArea(3, 10)]
    [SerializeField] private string dialogueText = "..."; 
    [SerializeField] private Sprite npcPortrait; 
    [SerializeField] private string playerTag = "Player";

    [Header("UI Reference")]
    [SerializeField] private GameObject dialogueBox; 
    [SerializeField] private TextMeshProUGUI contentDisplay; 
    [SerializeField] private GameObject portraitDisplay;

    private bool isPlayerInRange = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            if (dialogueBox != null)
            {
                dialogueBox.SetActive(false);
                if (portraitDisplay != null) portraitDisplay.SetActive(false);
                Time.timeScale = 1f; // คืนเวลาให้เกมเดินต่อเมื่อเดินหนี
            }
        }
    }

    void Update()
    {
        // เช็คว่าอยู่ในระยะ และกดคลิกซ้าย (0) หรือคลิกขวา (1)
        if (isPlayerInRange && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            ToggleDialogue();
        }
    }

    void ToggleDialogue()
    {
        if (dialogueBox == null) return;

        bool isOpening = !dialogueBox.activeSelf;

        if (isOpening)
        {
            // 1. ใส่ข้อมูลก่อน
            if (contentDisplay != null) contentDisplay.text = dialogueText;
            if (portraitDisplay != null && npcPortrait != null)
            {
                Image portraitImage = portraitDisplay.GetComponent<Image>();
                if (portraitImage != null) portraitImage.sprite = npcPortrait;
            }

            // 2. เปิด UI
            dialogueBox.SetActive(true);
            if (portraitDisplay != null) portraitDisplay.SetActive(true);

            // 3. หยุดเวลาเป็นลำดับสุดท้าย
            Time.timeScale = 0f;
        }
        else
        {
            dialogueBox.SetActive(false);
            if (portraitDisplay != null) portraitDisplay.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}