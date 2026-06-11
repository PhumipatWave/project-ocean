using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// HierarchyPlusCore — Entry point for the HierarchyPlus editor extension.
    ///
    /// Phase 1 : InitializeOnLoad, subscribe hierarchyWindowItemOnGUI.
    /// Phase 5 : hierarchyChanged → ComponentMinimap.InvalidateCache().
    /// Phase 6 : Second hierarchyWindowItemOnGUI subscription that calls
    ///           ComponentMinimap.FlushHoverState() after every repaint pass,
    ///           using instanceID == 0 as a sentinel for "end of frame".
    ///           This drives the ComponentTooltipWindow show/hide logic.
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyPlusCore
    {
        static HierarchyPlusCore()
        {
            // Main drawing callback — fires per visible item every repaint.
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyPlusDrawer.OnHierarchyItemGUI;

            // Phase 6: second subscription to flush hover state at end of repaint.
            // Unity fires the callback for every visible row; we use the fact that
            // instanceID == 0 can appear as an "empty" row after all real items,
            // but the safer approach is a delayCall reset after each repaint.
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUIPost;

            // Phase 5: rebuild component list cache when hierarchy structure changes.
            EditorApplication.hierarchyChanged += OnHierarchyChanged;

            // Strip stale SetIconForObject icons from a previous session.
            EditorApplication.delayCall += ClearBakedIcons;

            // Clear texture caches on domain reload.
            CategoryIconMap.ClearCache();
            ComponentMinimap.ClearIconCache();

            Debug.Log("[HierarchyPlus] Initialized (Phase 6 — Component Hover Tooltip).");
        }

        // ── Phase 6: hover flush ────────────────────────────────────────────

        /// <summary>
        /// Secondary per-item callback used solely to detect end-of-repaint.
        /// Unity calls hierarchyWindowItemOnGUI for every visible row in order.
        /// We schedule a delayCall on the FIRST invocation each frame so that
        /// FlushHoverState runs once after all rows have been processed.
        /// </summary>
        private static bool _flushScheduled;

        private static void OnHierarchyItemGUIPost(int instanceID, Rect selectionRect)
        {
            if (_flushScheduled) return;
            _flushScheduled = true;

            // delayCall fires after the current GUI pass finishes.
            EditorApplication.delayCall += () =>
            {
                ComponentMinimap.FlushHoverState();
                _flushScheduled = false;
            };
        }

        // ── Phase 5: hierarchy change ───────────────────────────────────────

private static void OnHierarchyChanged()
        {
            ComponentMinimap.InvalidateCache();
            MissingScriptDetector.InvalidateCache();
        }

        // ── Icon cleanup ────────────────────────────────────────────────────

        private static void ClearBakedIcons()
        {
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allObjects)
            {
                if (!UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(go.scene) &&
                    go.scene.IsValid())
                {
                    var existing = EditorGUIUtility.GetIconForObject(go);
                    if (existing != null)
                        EditorGUIUtility.SetIconForObject(go, null);
                }
            }
        }
    }
}
