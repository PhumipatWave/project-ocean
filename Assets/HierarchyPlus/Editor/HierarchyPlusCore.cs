using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// HierarchyPlusCore — Entry point for the HierarchyPlus editor extension.
    ///
    /// Uses [InitializeOnLoad] so Unity automatically calls the static constructor
    /// when the Editor loads or recompiles scripts. We subscribe to
    /// EditorApplication.hierarchyWindowItemOnGUI which fires once per visible
    /// Hierarchy item every repaint, giving us a rect + instance ID to work with.
    ///
    /// Phase 5 additions:
    ///   - Subscribe hierarchyChanged → ComponentMinimap.InvalidateCache()
    ///     so component lists are rebuilt after scene edits, not every repaint.
    ///   - Clear ComponentMinimap icon cache on domain reload.
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyPlusCore
    {
        // ── Static constructor ──────────────────────────────────────────────
        static HierarchyPlusCore()
        {
            // Main drawing callback — fires per-item on every repaint.
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyPlusDrawer.OnHierarchyItemGUI;

            // Phase 5: invalidate the component list cache whenever the hierarchy
            // structure changes (objects added / removed / renamed).
            EditorApplication.hierarchyChanged += OnHierarchyChanged;

            // Clear any icons that were permanently baked by SetIconForObject
            // in a previous session — reset all scene GameObjects to null icon.
            EditorApplication.delayCall += ClearBakedIcons;

            // Clear icon caches on domain reload so stale textures are never used.
            CategoryIconMap.ClearCache();
            ComponentMinimap.ClearIconCache(); // Phase 5

            Debug.Log("[HierarchyPlus] Initialized (Phase 5 — Component Minimap).");
        }

        // ── Hierarchy change handler ────────────────────────────────────────

        /// <summary>
        /// Fired by Unity when the hierarchy changes (object added, removed,
        /// renamed, reparented, etc.).  Wiping the component cache here means
        /// GetComponents is only called once per change, not every repaint.
        /// </summary>
        private static void OnHierarchyChanged()
        {
            ComponentMinimap.InvalidateCache();
        }

        // ── Icon cleanup ────────────────────────────────────────────────────

        private static void ClearBakedIcons()
        {
            // Find every GameObject in all loaded scenes and strip any
            // icon that was set via SetIconForObject previously.
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allObjects)
            {
                // Only touch scene objects, not assets.
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
