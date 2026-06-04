using Unity.Cinemachine;
using UnityEngine;

namespace Game.Testing
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

        public void SmallShake() => impulseSource.GenerateImpulse(.3f);
        public void NormalShake() => impulseSource.GenerateImpulse(1f);
        public void HighShake() => impulseSource.GenerateImpulse(2f);
    }
}
