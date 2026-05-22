using UnityEngine;
using UnityEngine.UI;

namespace Game.Testing
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
