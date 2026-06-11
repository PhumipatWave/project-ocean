using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// MissingScriptDetector — Phase 9
    ///
    /// Detects GameObjects that have one or more missing (null) script slots
    /// and draws a ⚠ warning icon on their Hierarchy row.
    ///
    /// A "missing script" occurs when a MonoBehaviour's backing .cs file has
    /// been deleted or moved without updating the scene reference. Unity stores
    /// the slot but the Component reference is null.
    ///
    /// Detection strategy:
    ///   GetComponents&lt;Component&gt;() returns null entries for missing scripts.
    ///   We scan once per GameObject and cache the result keyed by instance ID.
    ///   The cache is invalidated on hierarchyChanged (wired in HierarchyPlusCore).
    ///
    /// Drawing:
    ///   A small ⚠ icon is drawn on the RIGHT side of the row, integrated into
    ///   the stacking cursor used by HierarchyPlusDrawer so it never overlaps
    ///   the status badge or component minimap.
    ///
    /// SOLID notes:
    ///   Single Responsibility : missing-script detection + warning icon only.
    ///   Open / Closed         : detection logic isolated; caller controls layout.
    /// </summary>
    public static class MissingScriptDetector
    {
        // ── Layout ──────────────────────────────────────────────────────────

        /// <summary>Width of the warning icon slot.</summary>
        public const float IconWidth = 14f;

        // ── Cache ────────────────────────────────────────────────────────────

        /// <summary>
        /// Keyed by GameObject instance ID.
        /// true  = this object has at least one missing script.
        /// false = all components are intact.
        /// </summary>
        private static readonly Dictionary<int, bool> _cache = new Dictionary<int, bool>();

        // ── Cached GUIContent ────────────────────────────────────────────────

        private static GUIContent _warningContent;

        private static GUIContent WarningContent
        {
            get
            {
                if (_warningContent == null)
                {
                    var icon = EditorGUIUtility.IconContent("console.warnicon.sml");
                    _warningContent = new GUIContent(
                        icon?.image,
                        "Missing Script Detected\nOne or more component slots on this GameObject " +
                        "reference a script that no longer exists.");
                }
                return _warningContent;
            }
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Draws a ⚠ warning icon at the right side of <paramref name="selectionRect"/>
        /// if the GameObject has any missing scripts.
        /// </summary>
        /// <param name="go">The GameObject for this hierarchy row.</param>
        /// <param name="selectionRect">Row rect provided by Unity's callback.</param>
        /// <param name="rightOffset">
        ///   Pixels already consumed from the right edge by other elements
        ///   (status badge, minimap). The icon is placed left of this offset.
        /// </param>
        /// <returns>
        ///   Pixel width consumed (IconWidth + padding) if a warning was drawn,
        ///   otherwise 0.
        /// </returns>
        public static float Draw(GameObject go, Rect selectionRect, float rightOffset)
        {
            if (!HasMissingScript(go)) return 0f;

            const float padding = 2f;
            float x = selectionRect.xMax - rightOffset - IconWidth - padding;
            float y = selectionRect.y + (selectionRect.height - IconWidth) * 0.5f;
            var   iconRect = new Rect(x, y, IconWidth, IconWidth);

            // Draw icon + tooltip via GUIContent.
            GUI.Label(iconRect, WarningContent, GUIStyle.none);

            return IconWidth + padding;
        }

        /// <summary>
        /// Returns true if <paramref name="go"/> has at least one missing script slot.
        /// Result is cached per instance ID until <see cref="InvalidateCache"/> is called.
        /// </summary>
        public static bool HasMissingScript(GameObject go)
        {
            if (go == null) return false;

            int id = go.GetInstanceID();
            if (_cache.TryGetValue(id, out bool cached)) return cached;

            bool result = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go) > 0;
            _cache[id] = result;
            return result;
        }

        /// <summary>
        /// Clears the detection cache.
        /// Call this from HierarchyPlusCore.OnHierarchyChanged().
        /// </summary>
        public static void InvalidateCache()
        {
            _cache.Clear();
        }
    }
}
