using System.Collections;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class FishAlertUI : MonoBehaviour
    {
        #region FieldProperties
        [Header("Alert UI")]
        [SerializeField] private GameObject seenPlayerAlertImage;
        [SerializeField] private GameObject attackWarningImage;
        [SerializeField] private float alertDisplayTime = 1.25f;

        private bool wasSeeingPlayer;
        private Coroutine alertHideCoroutine;
        private Coroutine attackHideCoroutine;
        #endregion

        private void Start()
        {
            HideAll();
        }

        public void SetSeeingPlayer(bool canSeePlayer)
        {
            if (canSeePlayer && !wasSeeingPlayer)
            {
                wasSeeingPlayer = true;
                ShowAlert();
            }

            if (!canSeePlayer && wasSeeingPlayer)
            {
                wasSeeingPlayer = false;
                if (seenPlayerAlertImage != null)
                    seenPlayerAlertImage.SetActive(false);

                if (alertHideCoroutine != null)
                {
                    StopCoroutine(alertHideCoroutine);
                    alertHideCoroutine = null;
                }
            }
        }

        public void ShowAttackWarning()
        {
            if (attackWarningImage == null) return;

            attackWarningImage.SetActive(true);

            if (attackHideCoroutine != null)
                StopCoroutine(attackHideCoroutine);

            attackHideCoroutine = StartCoroutine(HideAttackAfterDelay());
        }

        public void ShowFleeWarning()
        {
            if (!wasSeeingPlayer)
            {
                wasSeeingPlayer = true;
                ShowAlert();
            }
        }

        public void HideAttackWarning()
        {
            if (attackWarningImage != null)
                attackWarningImage.SetActive(false);

            if (attackHideCoroutine != null)
            {
                StopCoroutine(attackHideCoroutine);
                attackHideCoroutine = null;
            }
        }

        public void HideAll()
        {
            if (seenPlayerAlertImage != null)
                seenPlayerAlertImage.SetActive(false);

            if (attackWarningImage != null)
                attackWarningImage.SetActive(false);

            if (alertHideCoroutine != null)
            {
                StopCoroutine(alertHideCoroutine);
                alertHideCoroutine = null;
            }

            if (attackHideCoroutine != null)
            {
                StopCoroutine(attackHideCoroutine);
                attackHideCoroutine = null;
            }

            wasSeeingPlayer = false;
        }

        private void ShowAlert()
        {
            if (seenPlayerAlertImage != null)
                seenPlayerAlertImage.SetActive(true);

            if (alertHideCoroutine != null)
                StopCoroutine(alertHideCoroutine);

            alertHideCoroutine = StartCoroutine(HideAlertAfterDelay());
        }

        private IEnumerator HideAlertAfterDelay()
        {
            yield return new WaitForSeconds(alertDisplayTime);
            if (seenPlayerAlertImage != null)
                seenPlayerAlertImage.SetActive(false);
            alertHideCoroutine = null;
        }

        private IEnumerator HideAttackAfterDelay()
        {
            yield return new WaitForSeconds(alertDisplayTime);
            if (attackWarningImage != null)
                attackWarningImage.SetActive(false);
            attackHideCoroutine = null;
        }
    }
}
