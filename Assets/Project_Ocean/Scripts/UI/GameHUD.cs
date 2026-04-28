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
        [SerializeField] private TextMeshProUGUI currentOxygenTubeLength;
        [SerializeField] private GameObject player;

        [SerializeField] private TextMeshProUGUI dangerLengthText;
        [SerializeField] private GameObject dangerLengthPanel;

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

            dangerLengthPanel.SetActive(false);
        }

        private void Update()
        {
            if (GameManager.Instance.GameState != GameState.Ocean)
            {
                currentOxygenTubeLength.enabled = false;
            }
            else
            {
                currentOxygenTubeLength.enabled = true;
            }

            currentOxygenTubeLength.color = Color.white;
            currentOxygenTubeLength.text = $"Length : {(int)player.GetComponent<PlayerOxygenTube>().CurrentLength} m.";

            if (player.GetComponent<PlayerOxygenTube>().CurrentLength >= player.GetComponent<PlayerOxygenTube>().MaxRopeLength - 20)
            {
                dangerLengthPanel.SetActive(true);
                dangerLengthText.text = Mathf.Abs((player.GetComponent<PlayerOxygenTube>().CurrentLength - player.GetComponent<PlayerOxygenTube>().MaxRopeLength)).ToString("F2");

                currentOxygenTubeLength.color = Color.red;
            }
            else
            {
                dangerLengthPanel.SetActive(false);
            }
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
