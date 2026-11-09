using Unity.Cinemachine;
using UnityEngine;
using Game.Core;

namespace Game.Gameplay
{
    public class CameraShakeManager : Singleton<CameraShakeManager>
    {
        private CinemachineImpulseSource impulseSource;

        protected override void Awake()
        {
            base.Awake();
            if (IsDuplicate) return;
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        public void SmallShake() => impulseSource.GenerateImpulse(new Vector3(0f, .5f, 0f));
        public void NormalShake() => impulseSource.GenerateImpulse(new Vector3(0f, 1.5f, 0f));
        public void HighShake() => impulseSource.GenerateImpulse(new Vector3(0f, 3f, 0f));
    }
}
