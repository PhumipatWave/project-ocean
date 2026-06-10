using UnityEngine;

namespace Game.Testing
{
    public class EnemySpawnManager : MonoBehaviour
    {
        public static EnemySpawnManager Instance { get; private set; }

        [SerializeField] private GameObject[] enemyPrefabs;

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            // ✅ เพิ่ม DontDestroyOnLoad เพื่อให้ Instance ไม่หายหลัง LoadScene
            // ตอนที่ DayManager (ซึ่ง DontDestroyOnLoad) เรียก ResetAllEnemies()
            DontDestroyOnLoad(gameObject);
        }

        public void ResetAllEnemies()
        {
            // ลบ enemy ที่มีอยู่ทั้งหมด
            foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
                Destroy(e);

            // Spawn ใหม่จาก spawn points
            var spawnPoints = GameObject.FindGameObjectsWithTag("EnemySpawnPoint");

            if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("[EnemySpawnManager] No enemy prefabs assigned");
                return;
            }

            foreach (var point in spawnPoints)
            {
                var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                Instantiate(prefab, point.transform.position, point.transform.rotation);
            }

            Debug.Log($"[EnemySpawnManager] Spawned {spawnPoints.Length} enemies");
        }
    }
}
