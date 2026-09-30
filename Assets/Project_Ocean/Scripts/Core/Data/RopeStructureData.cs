using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(fileName = "RopeData", menuName = "Data/RopeData")]
    public class RopeStructureData : ScriptableObject
    {
        public float maxRopeLength = 15;
        public int ropeSegment = 20;
    }
}
