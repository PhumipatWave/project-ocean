using UnityEngine;

namespace HierarchyPlus
{
    /// <summary>
    /// DevelopmentStatusTag — Runtime component that tracks the workflow state
    /// of a GameObject for team collaboration and debugging.
    ///
    /// Attach to any GameObject to mark it with a <see cref="DevelopmentStatus"/>
    /// value. The Editor overlay (HierarchyPlusDrawer) reads this component to
    /// draw a colored badge and tooltip in the Hierarchy window.
    ///
    /// Fields:
    /// <list type="bullet">
    ///   <item><term>Status</term><description>Current development state (drives badge color).</description></item>
    ///   <item><term>AssignedDeveloper</term><description>Name of the developer responsible for this object.</description></item>
    ///   <item><term>Notes</term><description>Free-text notes shown in the Hierarchy tooltip on hover.</description></item>
    /// </list>
    ///
    /// This is a pure data component — no runtime logic, safe to leave in builds.
    /// </summary>
    [AddComponentMenu("HierarchyPlus/Development Status Tag")]
    [DisallowMultipleComponent]
    public class DevelopmentStatusTag : MonoBehaviour
    {
        /// <summary>Current development state of this GameObject.</summary>
        [Tooltip("Current development state of this GameObject.")]
        public DevelopmentStatus status = DevelopmentStatus.None;

        /// <summary>
        /// Name of the developer assigned to this GameObject.
        /// Shown in the Hierarchy tooltip alongside the status and notes.
        /// </summary>
        [Tooltip("Name of the developer responsible for this GameObject. Shown in the Hierarchy tooltip.")]
        public string assignedDeveloper = "";

        /// <summary>Optional free-text notes shown in the Hierarchy tooltip.</summary>
        [Tooltip("Optional notes shown in the Hierarchy tooltip.")]
        [TextArea(2, 4)]
        public string notes = "";
    }
}
