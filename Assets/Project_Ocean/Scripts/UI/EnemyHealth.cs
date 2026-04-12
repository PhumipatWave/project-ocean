using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private Enemy enemy;
        [SerializeField] private Image healthValueImage;

        private void Update()
        {
            if (enemy.IsBullet)
            {
                healthValueImage.enabled = false;
            }

            healthValueImage.fillAmount = enemy.CurrentHealth / enemy.MaxHealth;
        }
    }
}
