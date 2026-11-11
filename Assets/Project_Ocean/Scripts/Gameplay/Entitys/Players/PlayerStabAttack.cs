using System.Collections;
using UnityEngine;

namespace Game.Testing
{
    public class PlayerStabAttack : MonoBehaviour
    {
        [Header("Hitbox")]
        [SerializeField] private GameObject stabHitbox;

        [Header("Settings")]
        [SerializeField] private float activeTime = 0.15f;
        [SerializeField] private float cooldown = 0.5f;

        private bool canAttack = true;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.V) && canAttack)
            {
                StartCoroutine(Stab());
            }
        }

        private IEnumerator Stab()
        {
            canAttack = false;

            stabHitbox.SetActive(true);

            yield return new WaitForSeconds(activeTime);

            stabHitbox.SetActive(false);

            yield return new WaitForSeconds(cooldown);

            canAttack = true;
        }
    }
}