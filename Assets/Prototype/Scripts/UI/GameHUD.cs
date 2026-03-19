using System;
using TMPro;
using UnityEngine;

namespace Game.Testing
{
    public class GameHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI currentOxygenTubeLenght;
        [SerializeField] private GameObject player;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        private void Update()
        {
            if (!player.GetComponent<Player>().OxygenTube.IsPullBack)
            {
                currentOxygenTubeLenght.text = $"Length : {(int)player.GetComponent<Player>().OxygenTube.CurrentRopeLength} m.";
            }
        }
    }
}
