using Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Gameplay
{
    /// <summary>Places the player and switches land/ocean setup when a map scene loads.</summary>
    public class PlayerSceneMode : MonoBehaviour
    {
        private Player player;
        private PlayerMovement movement;
        private PlayerHealth playerHealth;

        private void Awake()
        {
            player = GetComponent<Player>();
            movement = GetComponent<PlayerMovement>();
            playerHealth = GetComponent<PlayerHealth>();
        }

        private void Start()
        {
            // Game always starts on land; Land_Map's sceneLoaded fires before the player exists.
            movement.SetLand(true);
            player.GunHandler?.gameObject.SetActive(false);
            player.OxygenTube?.ResetRope();
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            PlayerUIState.Reset();

            if (scene.name == "Land_Map")
                EnterLand();
            else if (scene.name == "Ocean_Map")
                EnterOcean();
        }

        private void EnterLand()
        {
            // SaveBridge clears SceneLoadContext after all data is applied.
            transform.position = SceneLoadContext.IsLoading
                ? SceneLoadContext.Position
                : GameObject.FindGameObjectWithTag(Tags.PlayerSpawnPoint).transform.position;

            // Rope max length follows health, so restore both on returning to land.
            playerHealth?.ResetToOriginal();
            player.OxygenTube?.ResetRope();

            player.GunHandler.gameObject.SetActive(false);
            movement.SetLand(true);

            // Leaving mid-aim must not strand canAttack=false or slowed time on land.
            player.GunHandler.ResetAim();
            movement.Stop();
        }

        private void EnterOcean()
        {
            Transform spawnPoint = GameObject.FindGameObjectWithTag(Tags.PlayerSpawnPoint).transform;
            transform.position = SceneLoadContext.IsLoading ? SceneLoadContext.Position : spawnPoint.position;

            player.GunHandler.gameObject.SetActive(true);
            movement.SetLand(false);

            // Rope anchor is separate from player spawn; falls back to spawn point if scene has none.
            var ropeAnchor = GameObject.FindGameObjectWithTag(Tags.OxygenTubePoint);
            player.OxygenTube.SetStartPoint(ropeAnchor != null ? ropeAnchor.transform.position : spawnPoint.position);
        }
    }
}
