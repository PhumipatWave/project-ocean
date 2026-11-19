using UnityEngine;

namespace HierarchyPlus
{
    /// <summary>
    /// HierarchyCategoryTag — Runtime component that tags a GameObject with a
    /// HierarchyCategory value.
    ///
    /// Attach this to any GameObject to give it a category. The Editor overlay
    /// (HierarchyPlusDrawer) reads this component to decide which icon and
    /// background color to draw in the Hierarchy window.
    ///
    /// This is a pure data component — it holds no logic.
    /// It is safe to leave on GameObjects in builds (no runtime overhead).
    /// </summary>
    [AddComponentMenu("HierarchyPlus/Category Tag")]
    [DisallowMultipleComponent]
    public class HierarchyCategoryTag : MonoBehaviour
    {
        [Tooltip("The category this GameObject belongs to. " +
                 "Drives the icon and color shown in the Hierarchy window.")]
        public HierarchyCategory category = HierarchyCategory.None;
    }
}
