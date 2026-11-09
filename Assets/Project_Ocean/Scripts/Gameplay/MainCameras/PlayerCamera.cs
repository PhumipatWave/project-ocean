using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;

namespace Game.Gameplay
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera playerCam;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var player = PlayerRegistry.Local;
            if (player == null) return;

            if (playerCam == null)
                playerCam = FindFirstObjectByType<CinemachineCamera>();

            if (playerCam == null) return;

            playerCam.Follow = player.transform;
            playerCam.LookAt = player.transform;
            playerCam.Priority = 10;
        }
    }
}
