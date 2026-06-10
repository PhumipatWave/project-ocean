using UnityEngine;
using UnityEngine.Events;

namespace Game.Testing
{
    public class DayManager : MonoBehaviour
    {
        public static DayManager Instance { get; private set; }

        public int CurrentDay { get; private set; } = 1;

        [Header("Events")]
        public UnityEvent<int> onNewDay;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>เรียกจาก SaveBridge หลัง load เสร็จ</summary>
        public void SetDay(int day)
        {
            CurrentDay = Mathf.Max(1, day);
        }

        /// <summary>เรียกจากเตียง — ขึ้นวันใหม่</summary>
        public void AdvanceDay()
        {
            CurrentDay++;
            Debug.Log($"[DayManager] Day {CurrentDay} begins");

            onNewDay?.Invoke(CurrentDay);

            // ✅ เปลี่ยนจาก ?. เงียบๆ → log เตือนถ้าหา EnemySpawnManager ไม่เจอ
            // เพราะ EnemySpawnManager ไม่ได้ DontDestroyOnLoad จะหายหลัง load scene
            if (EnemySpawnManager.Instance != null)
                EnemySpawnManager.Instance.ResetAllEnemies();
            else
                Debug.LogWarning("[DayManager] EnemySpawnManager not found — enemies not reset");

            // Auto-save
            if (SaveBridge.Instance != null)
                SaveBridge.Instance.SaveGame();
            else
                Debug.LogWarning("[DayManager] SaveBridge not found — auto-save skipped");
        }
    }
}
