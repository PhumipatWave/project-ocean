using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Game.Core;

namespace Game.Gameplay
{
    [Serializable]
    public class SceneVolumeWeights
    {
        [Tooltip("Scene whose 'Volume'-tagged Volumes use these weights.")]
        public string sceneName;
        [Range(0f, 1f)] public float morning = 1f;
        [Range(0f, 1f)] public float afternoon = 0.6f;
        [Range(0f, 1f)] public float evening = 0.25f;

        public float For(TimePeriod period) => period switch
        {
            TimePeriod.Afternoon => afternoon,
            TimePeriod.Evening   => evening,
            _                    => morning,
        };
    }

    /// <summary>Sets every 'Volume'-tagged Volume's weight from the current TimePeriod, per scene.</summary>
    public class TimePeriodVolumeController : MonoBehaviour
    {
        private const string VolumeTag = "Volume";

        [SerializeField] private List<SceneVolumeWeights> sceneWeights = new()
        {
            new SceneVolumeWeights { sceneName = "Land_Map",  morning = 0f, afternoon = 0.6f, evening = 1f },
            new SceneVolumeWeights { sceneName = "Ocean_Map", morning = 1f, afternoon = 0.6f, evening = 0.25f },
        };

        private void OnEnable()
        {
            DayManager.OnTimePeriodChanged += Apply;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            ApplyCurrent();
        }

        private void OnDisable()
        {
            DayManager.OnTimePeriodChanged -= Apply;
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        // A scene can load mid-day, so its volumes need the current period, not just the next change.
        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyCurrent();

        private void ApplyCurrent()
        {
            if (DayManager.Instance != null)
                Apply(DayManager.Instance.CurrentPeriod);
        }

        private void Apply(TimePeriod period)
        {
            foreach (var go in GameObject.FindGameObjectsWithTag(VolumeTag))
            {
                if (!go.TryGetComponent<Volume>(out var volume)) continue;

                var weights = sceneWeights.Find(w => w.sceneName == go.scene.name);
                if (weights != null)
                    volume.weight = weights.For(period);
            }
        }
    }
}
