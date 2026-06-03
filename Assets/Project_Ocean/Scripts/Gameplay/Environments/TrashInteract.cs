using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Game.Testing;

public class TrashInteract : MonoBehaviour
{
    [Header("Basic Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float shakeAmount = 0.05f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;

    [Header("Item & Physics")]
    [SerializeField] private GameObject moneyPrefab;
    [SerializeField] private GameObject[] junkPrefabs;
    [SerializeField] private float throwForce = 6f;

    [Header("Dialogue Content")]
    [TextArea(3, 10)]
    [SerializeField] private string dialogueText = "I found MONEY!?";
    [SerializeField] private Sprite npcPortrait;
    [SerializeField] private float autoCloseTime = 1.5f; // ตั้งเวลาให้ปิดเองตรงนี้

    [Header("UI Reference")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI contentDisplay;
    [SerializeField] private GameObject portraitDisplay;

    private bool isPlayerInRange = false;
    private bool wasSearched = false;
    private Vector3 originalPosition;
    private PlayerMoney targetPlayerMoney;
    private Coroutine shakeCoroutine;

    void Awake() { originalPosition = transform.localPosition; }

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
            if (dialogueBox != null) dialogueBox.SetActive(false);
        }
    }

    void Update()
    {
        // คุ้ยได้ครั้งเดียวตอนกดคลิก
        if (isPlayerInRange && !wasSearched && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
        {
            SearchTrash();
        }
    }

    void SearchTrash()
    {
        wasSearched = true;

        if (audioSource != null)
        {
            audioSource.pitch = Random.Range(1.1f, 1.3f); // สุ่มเสียงให้ไวขึ้น
            audioSource.Play();
        }

        shakeCoroutine = StartCoroutine(ShakeEffect());

        if (Random.value > 0.7f)
            StartCoroutine(MoneySequence());
        else
        {
            if (junkPrefabs.Length > 0)
                SpawnObject(junkPrefabs[Random.Range(0, junkPrefabs.Length)]);
            StartCoroutine(StopSearchAfterTime(0.3f));
        }
    }

    IEnumerator MoneySequence()
    {
        int randomAmount = Random.Range(50, 200);
        if (targetPlayerMoney != null) targetPlayerMoney.AddMoney(randomAmount);
        SpawnObject(moneyPrefab);

        // 1. รอจังหวะสั่นและไอเทมกระเด็นสักครู่ (0.5 วินาที)
        yield return new WaitForSeconds(0.5f);

        // 2. หยุดการสั่นและเสียง เพื่อเตรียมโชว์ข้อความ
        StopEverything();

        // 3. แสดง UI ข้อความเจอเงิน
        ShowUI(string.Format(dialogueText, randomAmount));

        // 4. หน่วงเวลาไว้ 1.5 วินาทีตามที่ต้องการ เพื่อให้ผู้เล่นอ่านทัน
        yield return new WaitForSeconds(1.5f);

        // 5. ปิด UI อัตโนมัติ
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    void ShowUI(string message)
    {
        if (dialogueBox == null) return;

        if (contentDisplay != null) contentDisplay.text = message;

        if (portraitDisplay != null)
        {
            portraitDisplay.SetActive(npcPortrait != null);
            Image img = portraitDisplay.GetComponent<Image>();
            if (img != null && npcPortrait != null) img.sprite = npcPortrait;
        }

        dialogueBox.SetActive(true);
    }

    void StopEverything()
    {
        if (shakeCoroutine != null) { StopCoroutine(shakeCoroutine); shakeCoroutine = null; }
        transform.localPosition = originalPosition;
        if (audioSource != null) audioSource.Stop();
    }

   
    void SpawnObject(GameObject prefab)
    {
        if (prefab == null) return;
        GameObject item = Instantiate(prefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(new Vector2(Random.Range(-0.5f, 0.5f), 1f).normalized * throwForce, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-5f, 5f), ForceMode2D.Impulse);
        }
        Destroy(item, 3.0f);
    }

    IEnumerator ShakeEffect() { while (true) { transform.localPosition = originalPosition + (Vector3)Random.insideUnitCircle * shakeAmount; yield return null; } }
    IEnumerator StopSearchAfterTime(float t) { yield return new WaitForSeconds(t); StopEverything(); }
}