using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// HierarchyPlusCore — Entry point for the HierarchyPlus editor extension.
    ///
    /// 1.InitializeOnLoad, subscribe hierarchyWindowItemOnGUI.
    /// 2.hierarchyChanged → ComponentMinimap.InvalidateCache().
    /// 3.Second hierarchyWindowItemOnGUI subscription drives
    ///            ComponentMinimap.FlushHoverState() after each repaint pass.
    /// 4.hierarchyChanged also calls HierarchyDataCache.InvalidateAll()
    ///            so cached GetComponent results are evicted when the scene changes.
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyPlusCore
    {
        static HierarchyPlusCore()
        {
            // Main drawing callback — fires per visible item every repaint.
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyPlusDrawer.OnHierarchyItemGUI;

            // second subscription to flush hover state at end of repaint.
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyItemGUIPost;

            // rebuild caches when hierarchy structure changes.
            EditorApplication.hierarchyChanged += OnHierarchyChanged;

            // Strip stale SetIconForObject icons from a previous session.
            EditorApplication.delayCall += ClearBakedIcons;

            // Clear texture / blacklist caches on domain reload.
            CategoryIconMap.ClearCache();
            ComponentMinimap.ClearIconCache();

            Debug.Log("[HierarchyPlus] Initialized (Phase 11 — Performance Optimization).");
        }

        // ── hover flush ────────────────────────────────────────────

        /// <summary>
        /// Secondary per-item callback used solely to detect end-of-repaint.
        /// A delayCall is scheduled on the FIRST invocation each frame so that
        /// FlushHoverState runs once after all rows have been processed.
        /// </summary>
        private static bool _flushScheduled;

        private static void OnHierarchyItemGUIPost(int instanceID, Rect selectionRect)
        {
            if (_flushScheduled) return;
            _flushScheduled = true;

            EditorApplication.delayCall += () =>
            {
                ComponentMinimap.FlushHoverState();
                _flushScheduled = false;
            };
        }

        // ── hierarchy change ─────────────────────────────────

        /// <summary>
        /// Invalidates all per-GameObject caches when the scene hierarchy changes.
        ///
        /// additions:
        ///   HierarchyDataCache stores GetComponent results keyed by instance ID.
        ///   When a component is added / removed / scene loaded, those results are
        ///   stale and must be evicted. This call is cheap (Dictionary.Clear).
        /// </summary>
        private static void OnHierarchyChanged()
        {
            // component list cache.
            ComponentMinimap.InvalidateCache();

            // missing-script detection cache.
            MissingScriptDetector.InvalidateCache();

            // tag component lookup cache (CategoryTag + StatusTag).
            HierarchyDataCache.InvalidateAll();
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
