using UnityEngine;

namespace HierarchyPlus
{
    /// <summary>
    /// DevelopmentStatusTag — Runtime component that tracks the workflow state
    /// of a GameObject for team collaboration and debugging.
    ///
    /// Fields visible in the Inspector:
    ///   Status            — Current development state (drives badge color).
    ///   AssignedDeveloper — Name of the developer responsible.
    ///   Notes             — Free-text notes shown in the Hierarchy tooltip.
    ///
    /// This is a pure data component — no runtime logic, safe to leave in builds.
    /// </summary>
    [AddComponentMenu("HierarchyPlus/Development Status Tag")]
    [DisallowMultipleComponent]
    public class DevelopmentStatusTag : MonoBehaviour
    {
        [Tooltip("Current development state of this GameObject.")]
        public DevelopmentStatus status = DevelopmentStatus.None;

        [Tooltip("Optional notes shown in the Hierarchy tooltip.")]
        [TextArea(2, 4)]
        public string notes = "";
    }
}
