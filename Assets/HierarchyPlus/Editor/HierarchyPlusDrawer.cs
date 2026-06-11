using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// HierarchyPlusDrawer — Responsible for all per-item custom GUI drawing
    /// inside the Unity Hierarchy window.
    ///
    /// Phase 2 : category background tint + emoji/icon in the icon slot.
    /// Phase 3 : colored status badge on the right edge.
    /// Phase 5 : component minimap icons to the left of the status badge.
    ///
    /// Right-side layout uses a stacking cursor so nothing overlaps:
    ///
    ///   xMax → [RightPadding] [StatusBadge] [SectionGap] [MinimapIcons...]
    ///
    ///   1. DrawStatus()         draws rightmost, returns width consumed.
    ///   2. ComponentMinimap.Draw() receives that offset and starts left of it.
    ///
    /// Architecture note (SOLID):
    ///   Single Responsibility : only draws; does not own data or state.
    ///   Open / Closed         : new visual layers added as Draw* methods.
    /// </summary>
    public static class HierarchyPlusDrawer
    {
        // ── Constants ────────────────────────────────────────────────────────

        /// <summary>Right-side margin (px) before the far edge of the row rect.</summary>
        private const float RightPadding = 4f;

        /// <summary>Width of the status badge.</summary>
        private const float BadgeWidth = 36f;

        /// <summary>Height of the status badge.</summary>
        private const float BadgeHeight = 13f;

        /// <summary>Gap (px) between the status badge and the minimap block.</summary>
        private const float SectionGap = 4f;

        // ── Cached GUIStyles ─────────────────────────────────────────────────

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

        // ── Main callback ────────────────────────────────────────────────────

        /// <summary>
        /// Called by Unity for EVERY visible Hierarchy row on each repaint.
        /// Right-side elements are drawn right-to-left using a shared cursor
        /// so they never overlap each other.
        /// </summary>
        public static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null) return;

            // Phase 2 — background tint + category icon (left side, no cursor needed).
            DrawCategory(go, selectionRect);

            // ── Right-side stacking cursor ───────────────────────────────────
            // Each Draw* call returns the pixel width it consumed from the right.
            // The cursor grows left so the next element starts to the left of the previous.

            float rightCursor = 0f;

            // Phase 3 — status badge (rightmost element).
            rightCursor += DrawStatus(go, selectionRect, rightCursor);

            // Gap between badge and minimap (only add if badge was drawn).
            if (rightCursor > 0f) rightCursor += SectionGap;

            // Phase 5 — minimap icons (to the left of the badge).
            ComponentMinimap.Draw(go, selectionRect, rightCursor);
        }

        // ── Phase 2: Category drawing ────────────────────────────────────────

        private static void DrawCategory(GameObject go, Rect selectionRect)
        {
            var tag = go.GetComponent<HierarchyCategoryTag>();
            if (tag == null || tag.category == HierarchyCategory.None) return;

            var category = tag.category;

            // Full-row background tint.
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

            // Replace Unity's GameObject icon slot with a category icon/emoji.
            const float unityIconSize = 16f;
            const float iconGap       = 2f;

            float iconX    = selectionRect.x - unityIconSize - iconGap;
            float iconY    = selectionRect.y + (selectionRect.height - unityIconSize) * 0.5f;
            var   iconRect = new Rect(iconX, iconY, unityIconSize, unityIconSize);

            var iconTex = CategoryIconMap.GetIcon(category);
            if (iconTex != null)
            {
                var fillColor = CategoryColorMap.GetSolidColor(category);
                var prev      = GUI.color;
                GUI.color     = fillColor;
                GUI.DrawTexture(iconRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
                GUI.color     = prev;
                GUI.DrawTexture(iconRect, iconTex, ScaleMode.ScaleToFit, true);
            }
            else
            {
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

        // ── Phase 3: Status badge drawing ────────────────────────────────────

        /// <summary>
        /// Draws the status badge anchored at (xMax - rightOffset) and returns
        /// the total pixel width consumed so the caller can advance the cursor.
        /// Returns 0 if no badge was drawn.
        /// </summary>
        private static float DrawStatus(GameObject go, Rect selectionRect, float rightOffset)
        {
            var tag = go.GetComponent<DevelopmentStatusTag>();
            if (tag == null || tag.status == DevelopmentStatus.None) return 0f;

            var status = tag.status;
            var color  = StatusColorMap.GetColor(status);
            if (color.a <= 0f) return 0f;

            // Badge sits at the right edge, shifted left by rightOffset.
            var badgeRect = new Rect(
                selectionRect.xMax - BadgeWidth - RightPadding - rightOffset,
                selectionRect.y + (selectionRect.height - BadgeHeight) * 0.5f,
                BadgeWidth,
                BadgeHeight);

            // Colored background.
            var prev  = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(badgeRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = prev;

            // Short label.
            GUI.Label(badgeRect, StatusColorMap.GetLabel(status), BadgeStyle);

            // Tooltip.
            GUI.Label(badgeRect, new GUIContent(string.Empty, StatusColorMap.BuildTooltip(tag)));

            // Return the width this element consumed from the right edge.
            return BadgeWidth + RightPadding;
        }

        // ── Shared utility ───────────────────────────────────────────────────

        /// <summary>
        /// Returns a rect aligned to the right side of a row rect.
        /// Utility for future phases that need to stack more right-side elements.
        /// </summary>
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
