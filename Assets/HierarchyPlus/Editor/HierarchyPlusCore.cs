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
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyPlusCore
    {
        // ── Static constructor ──────────────────────────────────────────────
        static HierarchyPlusCore()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HierarchyPlusDrawer.OnHierarchyItemGUI;

            // Clear any icons that were permanently baked by SetIconForObject
            // in a previous session — reset all scene GameObjects to null icon.
            EditorApplication.delayCall += ClearBakedIcons;

            // Clear icon cache on every domain reload so stale textures are never used.
            CategoryIconMap.ClearCache();

            
            Debug.Log("[HierarchyPlus] Initialized.");
        }

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
