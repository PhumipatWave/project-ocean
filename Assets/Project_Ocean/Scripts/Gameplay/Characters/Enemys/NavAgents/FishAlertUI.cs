using UnityEngine;

public class FishAlertUI : MonoBehaviour
{
    [Header("Alert UI")]
    [SerializeField] private GameObject seenPlayerAlertImage;
    [SerializeField] private GameObject attackWarningImage;

    private bool wasSeeingPlayer = false;

    private void Start()
    {
        HideAll();
    }

    public void SetSeeingPlayer(bool canSeePlayer)
    {
        if (canSeePlayer && !wasSeeingPlayer)
        {
            if (seenPlayerAlertImage != null)
                seenPlayerAlertImage.SetActive(true);

            wasSeeingPlayer = true;
        }

        if (!canSeePlayer && wasSeeingPlayer)
        {
            if (seenPlayerAlertImage != null)
                seenPlayerAlertImage.SetActive(false);

            wasSeeingPlayer = false;
        }
    }

    public void ShowAttackWarning()
    {
        if (attackWarningImage != null)
            attackWarningImage.SetActive(true);
    }

    public void HideAttackWarning()
    {
        if (attackWarningImage != null)
            attackWarningImage.SetActive(false);
    }

    public void HideAll()
    {
        if (seenPlayerAlertImage != null)
            seenPlayerAlertImage.SetActive(false);

        if (attackWarningImage != null)
            attackWarningImage.SetActive(false);

        wasSeeingPlayer = false;
    }
}