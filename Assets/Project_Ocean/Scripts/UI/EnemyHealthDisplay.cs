using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Gameplay;

namespace Game.UI
{
    public class EnemyHealthDisplay : MonoBehaviour
    {
        [SerializeField] private Enemy enemy;
        [SerializeField] private Image healthValueImage;

        private void Update()
        {
            healthValueImage.fillAmount = enemy.CurrentHealth / enemy.MaxHealth;
        }
    }
}
