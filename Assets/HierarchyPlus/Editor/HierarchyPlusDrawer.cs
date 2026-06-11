using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// HierarchyPlusDrawer - Responsible for all per-item custom GUI drawing
    /// inside the Unity Hierarchy window.
    ///
    /// Phase 2 additions:
    ///   - Reads HierarchyCategoryTag from the GameObject.
    ///   - Draws a colored background tint across the full row.
    ///   - Draws a category emoji icon to the left of the name.
    ///
    /// Architecture note (SOLID):
    ///   Single Responsibility : only draws; does not own data or state.
    ///   Open / Closed         : new visual layers added as Draw* methods.
    /// </summary>
    public static class HierarchyPlusDrawer
    {
        // ---- Constants -----------------------------------------------------

        /// <summary>Right-side margin (px) before the far edge of the row rect.</summary>
        private const float RightPadding = 4f;

        /// <summary>Width reserved for the category icon label.</summary>
        private const float IconWidth = 20f;

        // ---- Cached GUIStyle -----------------------------------------------

        private static GUIStyle _iconStyle;

        private static GUIStyle IconStyle
        {
            get
            {
                if (_iconStyle == null)
                {
                    _iconStyle = new GUIStyle(EditorStyles.label)
                    {
                        fontSize  = 11,
                        alignment = TextAnchor.MiddleCenter,
                        padding   = new RectOffset(0, 0, 0, 0)
                    };
                }
                return _iconStyle;
            }
        }

        // ---- Main callback -------------------------------------------------

        /// <summary>
        /// Called by Unity for EVERY visible Hierarchy row on each repaint.
        /// instanceID resolves to the row's GameObject (or null for headers).
        /// </summary>
        public static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;

            DrawCategory(go, selectionRect);   // Phase 2
            DrawStatus(go, selectionRect);     // Phase 3
            // DrawMinimap(go, selectionRect); // Phase 5  (hooked up later)
        }

        // ---- Phase 2: Category drawing ------------------------------------

        /// <summary>
        /// Draws the category background tint and emoji icon for a row.
        /// Silently skips objects with no HierarchyCategoryTag or category == None.
        /// </summary>
        private static void DrawCategory(GameObject go, Rect selectionRect)
        {
            // NOTE: Phase 11 will replace GetComponent with a cache.
            var tag = go.GetComponent<HierarchyCategoryTag>();
            if (tag == null || tag.category == HierarchyCategory.None) return;

            var category = tag.category;

            // ── 1. Full-row background color tint ──────────────────────────
            var bgColor = CategoryColorMap.GetColor(category);
            if (bgColor.a > 0f)
            {
                var prevColor = GUI.color;
                GUI.color = bgColor;
                GUI.DrawTexture(
                    new Rect(0f, selectionRect.y, selectionRect.xMax, selectionRect.height),
                    EditorGUIUtility.whiteTexture);
                GUI.color = prevColor;
            }

            // ── 2. Replace Unity's GameObject icon slot ─────────────────────
            // Layout: [indent] [arrow ~14px] [icon 16px] [2px gap] [name...]
            // selectionRect.x = start of name text.
            const float unityIconSize = 16f;
            const float iconGap       = 2f;

            float iconX = selectionRect.x - unityIconSize - iconGap;
            float iconY = selectionRect.y + (selectionRect.height - unityIconSize) * 0.5f;
            var   iconRect = new Rect(iconX, iconY, unityIconSize, unityIconSize);

            // Try real Texture2D icon first (Phase 4).
            var iconTex = CategoryIconMap.GetIcon(category);
            if (iconTex != null)
            {
                // Paint solid color background behind the icon so the
                // Unity default icon is fully covered.
                var fillColor = CategoryColorMap.GetSolidColor(category);
                var prev      = GUI.color;
                GUI.color     = fillColor;
                GUI.DrawTexture(iconRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color     = prev;

                // Draw the real icon on top.
                GUI.DrawTexture(iconRect, iconTex, ScaleMode.ScaleToFit, true);
            }
            else
            {
                // Fallback: solid color fill + emoji label.
                var fillColor = CategoryColorMap.GetSolidColor(category);
                var prev      = GUI.color;
                GUI.color     = fillColor;
                GUI.DrawTexture(iconRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color     = prev;

                var emoji = CategoryColorMap.GetIcon(category);
                if (!string.IsNullOrEmpty(emoji))
                    GUI.Label(iconRect, emoji, IconStyle);
            }
        }

        // ---- Phase 3: Status badge drawing ------------------------------------

        /// <summary>Cached GUIStyle for the status badge text.</summary>
        private static GUIStyle _badgeStyle;
        private static GUIStyle BadgeStyle
        {
            get
            {
                if (_badgeStyle == null)
                {
                    _badgeStyle = new GUIStyle(EditorStyles.label)
                    {
                        fontSize  = 8,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        padding   = new RectOffset(2, 2, 0, 0),
                    };
                    _badgeStyle.normal.textColor = Color.white;
                }
                return _badgeStyle;
            }
        }

        /// <summary>
        /// Draws a colored status badge on the RIGHT side of the row.
        /// Shows a tooltip with status, developer, and notes on hover.
        /// </summary>
        private static void DrawStatus(GameObject go, Rect selectionRect)
        {
            // NOTE: Phase 11 will replace GetComponent with a cache.
            var tag = go.GetComponent<DevelopmentStatusTag>();
            if (tag == null || tag.status == DevelopmentStatus.None) return;

            var status = tag.status;
            var color  = StatusColorMap.GetColor(status);
            if (color.a <= 0f) return;

            // Badge rect — right-aligned, 36px wide, vertically centered with 2px inset.
            const float badgeWidth  = 36f;
            const float badgeHeight = 13f;
            var badgeRect = new Rect(
                selectionRect.xMax - badgeWidth - RightPadding,
                selectionRect.y + (selectionRect.height - badgeHeight) * 0.5f,
                badgeWidth,
                badgeHeight);

            // Draw colored background.
            var prev  = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(badgeRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = prev;

            // Draw label text (e.g. "WIP", "BUG", "DONE").
            var label = StatusColorMap.GetLabel(status);
            GUI.Label(badgeRect, label, BadgeStyle);

            // Tooltip — shows on hover over the badge rect.
            var tooltip = StatusColorMap.BuildTooltip(tag);
            GUI.Label(badgeRect, new GUIContent("", tooltip));
        }

        
// ---- Shared utility -----------------------------------------------

        /// <summary>
        /// Returns a rect aligned to the right side of a row rect.
        /// Used by Phase 3+ to stack badges/icons from the right edge inward.
        /// </summary>
        /// <param name="rowRect">The full selectionRect from the callback.</param>
        /// <param name="width">Desired width of the overlay rect.</param>
        /// <param name="offset">Additional right-to-left offset for stacking.</param>
        public static Rect GetRightAlignedRect(Rect rowRect, float width, float offset = 0f)
        {
            return new Rect(
                rowRect.xMax - width - RightPadding - offset,
                rowRect.y,
                width,
                rowRect.height);
        }
    }
}
