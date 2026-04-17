using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class GameHUD : MonoBehaviour
    {
        [Header("Rope Length")]
        [SerializeField] private TextMeshProUGUI currentOxygenTubeLenght;
        [SerializeField] private GameObject player;

        [Header("Black Fade")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private float fadeDuration = 1f;

        public static GameHUD instance {  get; private set; }
        private void Awake()
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        private void Update()
        {
            if (GameManager.Instance.GameState != GameState.Ocean)
            {
                currentOxygenTubeLenght.enabled = false;
            }
            else
            {
                currentOxygenTubeLenght.enabled = true;
            }

            currentOxygenTubeLenght.text = $"Length : {(int)player.GetComponent<PlayerOxygenTube>().CurrentLength} m.";
        }

        public void FadeIn()
        {
            StartCoroutine(FadeRoutine());
        }

        private IEnumerator FadeRoutine()
        {
            fadeImage.enabled = true;

            yield return new WaitForSeconds(3f);
            fadeImage.enabled = false;
        }
    }
}
