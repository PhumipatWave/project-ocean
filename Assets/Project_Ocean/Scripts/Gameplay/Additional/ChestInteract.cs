using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Game.Testing;
using UnityEngine.SceneManagement;

public class ChestInteract : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private int moneyAmount = 100;
    private bool wasOpened = false;
    private bool isPlayerInRange = false;
    private PlayerMoney targetPlayerMoney;

    [Header("First Interaction (ชุดที่ 1)")]
    [TextArea(3, 10)][SerializeField] private string firstDialogue;
    [SerializeField] private Sprite firstPortrait;
    [SerializeField] private Sprite firstExtraSprite;

    [Header("Subsequent Interaction (ชุดที่ 2)")]
    [TextArea(3, 10)][SerializeField] private string secondDialogue;
    [SerializeField] private Sprite secondPortrait;
    [SerializeField] private Sprite secondExtraSprite;

    [Header("UI Reference")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI contentDisplay;
    [SerializeField] private GameObject portraitDisplay;
    [SerializeField] private GameObject extraImageObject;

    // ใช้ Static List เพื่อจำว่ากล่องใบไหนถูกเปิดไปแล้วบ้างใน "รอบการเล่นนี้"
    // ค่านี้จะหายไปเมื่อปิดเกม หรือ Compile Code ใหม่
    private static List<string> globalOpenedChests = new List<string>();
    private string uniqueID;

    void Awake()
    {
        // สร้าง ID ที่อ้างอิงจาก ชื่อฉาก + ชื่อ Object + ตำแหน่ง เพื่อไม่ให้ซ้ำกับกล่องใบอื่น
        uniqueID = SceneManager.GetActiveScene().name + "_" + gameObject.name + "_" + transform.position.ToString();

        // เช็คว่าใน List ส่วนกลางมี ID ของเราอยู่หรือยัง
        if (globalOpenedChests.Contains(uniqueID))
        {
            wasOpened = true;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = true;
            targetPlayerMoney = other.GetComponent<PlayerMoney>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInRange = false;
            targetPlayerMoney = null;
            CloseUI();
        }
    }

    void Update()
    {
        if (isPlayerInRange && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            if (!dialogueBox.activeSelf) OpenUI();
            else CloseUI();
        }
    }

    void OpenUI()
    {
        if (dialogueBox == null) return;

        string currentText;
        Sprite currentPortrait;
        Sprite currentExtraSprite;

        if (!wasOpened)
        {
            wasOpened = true;

            // เพิ่ม ID เข้าไปใน List ส่วนกลางเพื่อบอกว่ากล่องนี้ถูกเปิดแล้วนะ
            if (!globalOpenedChests.Contains(uniqueID))
            {
                globalOpenedChests.Add(uniqueID);
            }

            if (targetPlayerMoney != null) targetPlayerMoney.AddMoney(moneyAmount);

            currentText = string.Format(firstDialogue, moneyAmount);
            currentPortrait = firstPortrait;
            currentExtraSprite = firstExtraSprite;
        }
        else
        {
            // ถ้าเคยเปิดไปแล้วใน Session นี้ จะเข้าเงื่อนไขนี้ตลอด
            currentText = secondDialogue;
            currentPortrait = secondPortrait;
            currentExtraSprite = secondExtraSprite;
        }

        if (contentDisplay != null) contentDisplay.text = currentText;

        if (portraitDisplay != null)
        {
            portraitDisplay.SetActive(currentPortrait != null);
            Image img = portraitDisplay.GetComponent<Image>();
            if (img != null && currentPortrait != null) img.sprite = currentPortrait;
        }

        if (extraImageObject != null)
        {
            extraImageObject.SetActive(currentExtraSprite != null);
            Image extraImg = extraImageObject.GetComponent<Image>();
            if (extraImg != null && currentExtraSprite != null)
            {
                extraImg.sprite = currentExtraSprite;
            }
        }

        dialogueBox.SetActive(true);
        Time.timeScale = 0f;
    }

    void CloseUI()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (extraImageObject != null) extraImageObject.SetActive(false);
        Time.timeScale = 1f;
    }
}