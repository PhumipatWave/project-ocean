using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Scene-local manager — place this GameObject in the Land scene hierarchy only.
    /// No DontDestroyOnLoad. No singleton.
    /// Subscribes to DayManager.OnEnemyReset so it resets enemies each new day
    /// without DayManager needing to know this class exists.
    /// </summary>
    public class EnemySpawnManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] enemyPrefabs;

        private void OnEnable()
        {
            DayManager.OnEnemyReset += ResetAllEnemies;
        }

        private void OnDisable()
        {
            DayManager.OnEnemyReset -= ResetAllEnemies;
        }

        public void ResetAllEnemies()
        {
            // Destroy all existing enemies in the scene
            foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
                Destroy(e);

            // Re-spawn from tagged spawn points
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
