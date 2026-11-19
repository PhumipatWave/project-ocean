using UnityEngine;
using System.Collections;
using Game.Core;

namespace Game.Gameplay
{
    [RequireComponent(typeof(Animator))]
    public class SimpleAnimLoop : MonoBehaviour
    {
        [Header("Wait Time Settings")]
        [SerializeField] private float minWaitTime = 5f;
        [SerializeField] private float maxWaitTime = 10f;

        private Animator anim;

        void Awake()
        {
            anim = GetComponent<Animator>();
        }

        void Start()
        {
            if (anim != null)
            {
                StartCoroutine(ToggleAnimRoutine());
            }
        }

        IEnumerator ToggleAnimRoutine()
        {
            anim.enabled = false;
            yield return new WaitForSeconds(1f);

            while (true)
            {
                anim.enabled = true;
                anim.Play(0, 0, 0f);

                yield return null; // state info is only valid a frame after Play
                float animLength = anim.GetCurrentAnimatorStateInfo(0).length;
                yield return new WaitForSeconds(animLength);

                // Hold on the last frame before disabling, or the sprite blanks out while paused.
                anim.Play(0, 0, 0.99f);
                yield return new WaitForEndOfFrame();

                anim.enabled = false;

                float waitTime = Random.Range(minWaitTime, maxWaitTime);
                yield return new WaitForSeconds(waitTime);
            }
        }
    }
}