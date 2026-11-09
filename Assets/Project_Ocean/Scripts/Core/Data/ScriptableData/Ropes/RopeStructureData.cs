using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(fileName = "RopeData", menuName = "ScriptableObjects/Player/Rope Data")]
    public class RopeStructureData : ScriptableObject
    {
        public float maxRopeLength = 15;
        public int ropeSegment = 20;
    }
}
