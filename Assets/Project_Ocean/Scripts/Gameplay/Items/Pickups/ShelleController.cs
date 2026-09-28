using System.Collections;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class ShelleController : MonoBehaviour, IPoolable
    {
        [SerializeField] private Animator animator;
        [SerializeField] private ItemDrop  itemDrop;

        [Header("Tick Settings")]
        [SerializeField] private float minTickInterval = 2f;
        [SerializeField] private float maxTickInterval = 6f;

        private static readonly int IsCloseTrigger = Animator.StringToHash("isClose");
        private static readonly int IsOpenTrigger  = Animator.StringToHash("isOpen");

        private bool      _isOpen;
        private bool      _collected;
        private bool      _started;
        private Coroutine _tickRoutine;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (itemDrop  == null) itemDrop  = GetComponentInChildren<ItemDrop>();
        }

        private void Start()
        {
            _started = true;
            Subscribe();
            StartTick();
        }

        // ── IPoolable ──

        public void OnGetFromPool()
        {
            Subscribe();
            if (_started) StartTick();
        }

        public void OnReturnToPool()
        {
            Unsubscribe();
            StopTick();
            _collected = false;
        }

        // ── Internal ──

        private void Subscribe()
        {
            if (itemDrop == null) return;
            itemDrop.OnCollected -= OnShellCollected;
            itemDrop.OnCollected += OnShellCollected;
        }

        private void Unsubscribe()
        {
            if (itemDrop != null) itemDrop.OnCollected -= OnShellCollected;
        }

        private void StartTick()
        {
            _collected = false;
            StopTick();
            _tickRoutine = StartCoroutine(TickRoutine());
        }

        private void StopTick()
        {
            if (_tickRoutine != null)
            {
                StopCoroutine(_tickRoutine);
                _tickRoutine = null;
            }
        }

        private IEnumerator TickRoutine()
        {
            SetOpen(Random.value > 0.5f);

            while (true)
            {
                float wait = Random.Range(minTickInterval, maxTickInterval);
                yield return new WaitForSeconds(wait);
                SetOpen(!_isOpen);
            }
        }

private void SetOpen(bool open)
        {
            _isOpen = open;
            if (itemDrop != null) itemDrop.IsCollectable = open;
            if (animator == null) return;
            if (open)
            {
                animator.ResetTrigger(IsCloseTrigger);
                animator.SetTrigger(IsOpenTrigger);
            }
            else
            {
                animator.ResetTrigger(IsOpenTrigger);
                animator.SetTrigger(IsCloseTrigger);
            }
        }

private void OnShellCollected()
        {
            _collected = true;
            StopTick();
            if (itemDrop != null) itemDrop.IsCollectable = false;
            if (animator != null)
            {
                animator.ResetTrigger(IsOpenTrigger);
                animator.SetTrigger(IsCloseTrigger);
            }
        }

        private void OnDestroy() => Unsubscribe();
    }
}
