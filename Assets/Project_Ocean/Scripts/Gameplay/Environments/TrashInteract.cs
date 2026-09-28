using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Game.Core;

namespace Game.Gameplay
{
    public class TrashInteract : MonoBehaviour
    {
        [Header("Basic Settings")]
        [SerializeField] private float shakeAmount = 0.05f;
        [SerializeField] private int minMoney = 10;
        [SerializeField] private int maxMoney = 50;

        [Header("Item & Physics")]
        [SerializeField] private GameObject moneyPrefab;
        [SerializeField] private GameObject[] junkPrefabs;
        [SerializeField] private float throwForce = 6f;

        [Header("Dialogue Content")]
        [TextArea(3, 10)]
        [SerializeField] private string dialogueText = "I found MONEY!?";
        [SerializeField] private Sprite npcPortrait;
        [SerializeField] private float autoCloseTime = 1.5f;

        [Header("UI Reference")]
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI contentDisplay;
        [SerializeField] private GameObject portraitDisplay;

        private bool wasSearched;
        private Vector3 originalPosition;
        private PlayerMoney targetPlayerMoney;
        private Coroutine shakeCoroutine;
        private Image portraitImage;

        void Awake()
        {
            originalPosition = transform.localPosition;
            if (portraitDisplay != null) portraitImage = portraitDisplay.GetComponent<Image>();
        }

        void OnEnable() => DayManager.OnDayAdvanced += ResetForNewDay;
        void OnDisable() => DayManager.OnDayAdvanced -= ResetForNewDay;

        void ResetForNewDay() => wasSearched = false;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                targetPlayerMoney = other.GetComponent<PlayerMoney>();
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                targetPlayerMoney = null;
                if (dialogueBox != null) dialogueBox.SetActive(false);
            }
        }

        public void SearchTrash()
        {
            if (wasSearched)
            { 
                PlayerUIState.Close(); 
                return;
            }

            wasSearched = true;

            AudioManager.Instance?.PlaySFX(SoundKeys.TrashClick, 1.1f, 1.3f);

            shakeCoroutine = StartCoroutine(ShakeEffect());

            if (Random.value > 0.8f)
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
            int randomAmount = Random.Range(minMoney, maxMoney);
            targetPlayerMoney?.AddMoney(randomAmount);
            SpawnObject(moneyPrefab);

            yield return new WaitForSeconds(0.5f);

            StopEverything();
            ShowUI(string.Format(dialogueText, randomAmount));

            yield return new WaitForSeconds(autoCloseTime);

            if (dialogueBox != null)
                dialogueBox.SetActive(false);

            PlayerUIState.Close();
        }

        void ShowUI(string message)
        {
            if (dialogueBox == null) return;

            if (nameText != null)
                nameText.text = "Billy";

            if (contentDisplay != null) 
                contentDisplay.text = message;

            if (portraitDisplay != null)
            {
                portraitDisplay.SetActive(npcPortrait != null);
                if (portraitImage != null && npcPortrait != null) portraitImage.sprite = npcPortrait;
            }

            dialogueBox.SetActive(true);
        }

        void StopEverything()
        {
            if (shakeCoroutine != null) 
            { 
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null; 
            }

            transform.localPosition = originalPosition;

            AudioManager.Instance?.StopSFX(SoundKeys.TrashClick);
        }

        void SpawnObject(GameObject prefab)
        {
            if (prefab == null)
                return;

            GameObject item = Instantiate(prefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = item.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.AddForce(new Vector2(Random.Range(-0.5f, 0.5f), 1f).normalized * throwForce, ForceMode2D.Impulse);
                rb.AddTorque(Random.Range(-5f, 5f), ForceMode2D.Impulse);
            }
            Destroy(item, 3f);
        }

        IEnumerator ShakeEffect()
        {
            while (true)
            {
                transform.localPosition = originalPosition + (Vector3)Random.insideUnitCircle * shakeAmount;
                yield return null;
            }
        }

        IEnumerator StopSearchAfterTime(float t)
        {
            yield return new WaitForSeconds(t);
            StopEverything();
            PlayerUIState.Close();
        }
    }
}
