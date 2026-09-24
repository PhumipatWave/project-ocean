using UnityEngine;

public class PlayerSprintVFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem sprintEffect;

    private void Awake()
    {
        if (sprintEffect == null)
            sprintEffect = GetComponent<ParticleSystem>();

        StopEffect();
    }

    public void SetSprinting(bool isSprinting)
    {
        if (sprintEffect == null)
            return;

        if (isSprinting)
        {
            if (!sprintEffect.isPlaying)
                sprintEffect.Play();
        }
        else
        {
            StopEffect();
        }
    }

    private void StopEffect()
    {
        sprintEffect.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );
    }
}