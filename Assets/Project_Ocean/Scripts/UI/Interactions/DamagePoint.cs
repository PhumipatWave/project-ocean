using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.Testing
{
    public class DamagePoint : MonoBehaviour, IDamagePopup
    {
        [SerializeField] private TextMeshProUGUI damagePointText;

        private void Awake()
        {
            damagePointText.enabled = false;
        }

        public void ShowDeath()
        {
            damagePointText.text = "Death";
            damagePointText.color = Color.red;
            damagePointText.enabled = true;
        }

        public void ShowDamagePoint(string damageText)
        {
            damagePointText.text = damageText;
            StartCoroutine(ShowDamagePointRoutine());
        }

        private IEnumerator ShowDamagePointRoutine()
        {
            damagePointText.enabled = true;
            yield return new WaitForSeconds(.45f);

            damagePointText.text = "";
            damagePointText.enabled = false;
        }
    }
}
