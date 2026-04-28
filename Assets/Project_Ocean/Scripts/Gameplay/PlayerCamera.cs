using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Testing
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
            var player = GameManager.CurPlayer;

            playerCam = FindFirstObjectByType<CinemachineCamera>();

            playerCam.Follow = player.transform;
            playerCam.LookAt = player.transform;

            playerCam.Priority = 10;
        }
    }
}
