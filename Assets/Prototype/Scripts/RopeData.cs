using UnityEngine;

namespace Game.Testing
{
    [CreateAssetMenu(fileName = "RopeData", menuName = "Data/RopeData")]
    public class RopeData : ScriptableObject
    {
        public float maxRopeLength = 15;
        public int ropeSegment = 20;
    }
}
