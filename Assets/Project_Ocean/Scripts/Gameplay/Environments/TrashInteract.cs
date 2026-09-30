using UnityEngine;
using System.Collections;
using Game.Core;

namespace Game.Gameplay
{
    public class TrashInteract : Interactable
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
        [SerializeField] private string speakerName = "Billy";
        [TextArea(3, 10)]
        [SerializeField] private string dialogueText = "I found MONEY!?";
        [SerializeField] private Sprite npcPortrait;
        [SerializeField] private float autoCloseTime = 1.5f;

        [Header("UI Reference")]
        [Tooltip("InteractPopupView on the land dialogue panel (an IInteractPopupView).")]
        [SerializeField] private MonoBehaviour popupView;

        private bool wasSearched;
        private Vector3 originalPosition;
        private Coroutine shakeCoroutine;

        private IInteractPopupView Popup => popupView as IInteractPopupView;

        void Awake() => originalPosition = transform.localPosition;

        protected override void OnEnable()
        {
            base.OnEnable();
            DayManager.OnDayAdvanced += ResetForNewDay;
        }

        protected override void OnDisable()
        {
            DayManager.OnDayAdvanced -= ResetForNewDay;
            base.OnDisable();
        }

        void ResetForNewDay() => wasSearched = false;

        protected override void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player")) Popup?.Hide();
            base.OnTriggerExit2D(other);
        }

        protected override void Interact(Player player)
        {
            if (wasSearched)
            {
                ReleasePlayerLock();
                return;
            }

            wasSearched = true;

            AudioManager.Instance?.PlaySFX(SoundKeys.TrashClick, 1.1f, 1.3f);

            shakeCoroutine = StartCoroutine(ShakeEffect());

            if (Random.value > 0.8f)
                StartCoroutine(MoneySequence(player.GetComponent<PlayerMoney>()));
            else
            {
                if (junkPrefabs.Length > 0)
                    SpawnObject(junkPrefabs[Random.Range(0, junkPrefabs.Length)]);
                StartCoroutine(StopSearchAfterTime(0.3f));
            }
        }

        IEnumerator MoneySequence(PlayerMoney money)
        {
            int randomAmount = Random.Range(minMoney, maxMoney);
            money?.AddMoney(randomAmount);
            SpawnObject(moneyPrefab);

            yield return new WaitForSeconds(0.5f);

            StopEverything();
            Popup?.Show(speakerName, string.Format(dialogueText, randomAmount), npcPortrait);

            yield return new WaitForSeconds(autoCloseTime);

            Popup?.Hide();
            ReleasePlayerLock();
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
            ReleasePlayerLock();
        }
    }
}
