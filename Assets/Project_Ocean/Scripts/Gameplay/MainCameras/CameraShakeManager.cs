using Unity.Cinemachine;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class CameraShakeManager : MonoBehaviour
    {
        private CinemachineImpulseSource impulseSource;

        public static CameraShakeManager Instance;
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
            }

            Instance = this;

            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        public void SmallShake() => impulseSource.GenerateImpulse(new Vector3(0f, 1f, 0f));
        public void NormalShake() => impulseSource.GenerateImpulse(new Vector3(0f, 2.5f, 0f));
        public void HighShake() => impulseSource.GenerateImpulse(new Vector3(0f, 5f, 0f));
    }
}
