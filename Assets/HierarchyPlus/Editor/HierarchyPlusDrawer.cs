using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// HierarchyPlusDrawer — Responsible for all per-item custom GUI drawing
    /// inside the Unity Hierarchy window.
    ///
    /// 1.category background tint + emoji/icon in the icon slot.
    /// 2.colored status badge on the right edge.
    /// 3.component minimap icons to the left of the status badge.
    /// 4.missing-script warning icon.
    /// 5.tag lookups routed through HierarchyDataCache — zero GetComponent calls during repaint for already-seen objects.
    /// 6.XML documentation pass; obsolete API pragma added for Unity 6 compatibility.
    ///
    /// Right-side layout uses a stacking cursor so nothing overlaps:
    ///
    ///   xMax → [RightPadding] [StatusBadge] [SectionGap] [WarnIcon] [MinimapIcons...]
    ///
    ///   1. DrawStatus()                 draws rightmost, returns width consumed.
    ///   2. MissingScriptDetector.Draw() draws left of that.
    ///   3. ComponentMinimap.Draw()      draws left of both.
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

        /// <summary>Width of the status badge pill.</summary>
        private const float BadgeWidth = 36f;

        /// <summary>Height of the status badge pill.</summary>
        private const float BadgeHeight = 13f;

        /// <summary>Gap (px) between the status badge and the element to its left.</summary>
        private const float SectionGap = 4f;

        // ── Cached GUIStyles ─────────────────────────────────────────────────

        private static GUIStyle _iconStyle;

        /// <summary>
        /// GUIStyle used for the category emoji label in the icon slot.
        /// Created once and reused; never allocates during repaint.
        /// </summary>
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

        /// <summary>
        /// GUIStyle for the short status badge label (e.g. "WIP", "DONE").
        /// Bold white text, small font, centered.
        /// </summary>
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
        /// Orchestrates all drawing passes and manages the right-side cursor.
        ///
        /// Performance :
        ///   All per-GameObject lookups are served from caches populated on first
        ///   access and invalidated on hierarchyChanged. No GetComponent calls occur
        ///   during repaint for objects that have been seen at least once.
        /// </summary>
        /// <param name="instanceID">Unity instance ID for the row's GameObject.</param>
        /// <param name="selectionRect">Row rect in Hierarchy window GUI space.</param>
        public static void OnHierarchyItemGUI(int instanceID, Rect selectionRect)
        {
            // CS0618: InstanceIDToObject is deprecated in Unity 6 in favour of
            // EditorUtility.EntityIdToObject. We keep the old API so the code
            // compiles on Unity 2022 and 2023 as well. Suppress the warning on
            // Unity 6 builds where the attribute is present.
#pragma warning disable CS0618
            var go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
#pragma warning restore CS0618
            if (go == null) return;

            // background tint + category icon (left side).
            DrawCategory(go, selectionRect);

            // ── Right-side stacking cursor ───────────────────────────────────
            // Each Draw* call consumes pixels from the right edge and returns
            // how many. The next element starts left of the previous result.

            float rightCursor = 0f;

            // status badge (rightmost right-side element).
            rightCursor += DrawStatus(go, selectionRect, rightCursor);

            // Gap between badge and next element (skipped when no badge was drawn).
            if (rightCursor > 0f) rightCursor += SectionGap;

            // missing-script warning icon.
            rightCursor += MissingScriptDetector.Draw(go, selectionRect, rightCursor);

            // component minimap icons.
            ComponentMinimap.Draw(go, selectionRect, rightCursor);
        }

        // ── Category drawing ────────────────────────────────────────

        /// <summary>
        /// Draws the category background tint and category icon/emoji for a row.
        /// Tag lookup is served from <see cref="HierarchyDataCache"/> — no
        /// <c>GetComponent</c> call fires during repaint for cached objects.
        /// </summary>
        /// <param name="go">The row's GameObject.</param>
        /// <param name="selectionRect">Row rect in Hierarchy GUI space.</param>
        private static void DrawCategory(GameObject go, Rect selectionRect)
        {
            var tag = HierarchyDataCache.GetCategoryTag(go);
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

            // Replace Unity's default GameObject icon with the category icon/emoji.
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

        // ── Status badge ────────────────────────────────────────────

        /// <summary>
        /// Draws the development-status badge at the right edge of the row.
        /// Returns the pixel width consumed so the caller can advance the cursor.
        /// Returns 0 when no badge is drawn (status is None or component absent).
        /// </summary>
        /// <param name="go">The row's GameObject.</param>
        /// <param name="selectionRect">Row rect in Hierarchy GUI space.</param>
        /// <param name="rightOffset">Pixels already consumed from the right edge.</param>
        /// <returns>Pixels consumed (BadgeWidth + RightPadding), or 0.</returns>
        private static float DrawStatus(GameObject go, Rect selectionRect, float rightOffset)
        {
            var tag = HierarchyDataCache.GetStatusTag(go);
            if (tag == null || tag.status == DevelopmentStatus.None) return 0f;

            var status = tag.status;
            var color  = StatusColorMap.GetColor(status);
            if (color.a <= 0f) return 0f;

            var badgeRect = new Rect(
                selectionRect.xMax - BadgeWidth - RightPadding - rightOffset,
                selectionRect.y + (selectionRect.height - BadgeHeight) * 0.5f,
                BadgeWidth,
                BadgeHeight);

            // Colored background pill.
            var prev  = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(badgeRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);
            GUI.color = prev;

            // Short label ("WIP", "DONE", etc.).
            GUI.Label(badgeRect, StatusColorMap.GetLabel(status), BadgeStyle);

            // Invisible overlay carrying the full tooltip (status + developer + notes).
            GUI.Label(badgeRect, new GUIContent(string.Empty, StatusColorMap.BuildTooltip(tag)));

            return BadgeWidth + RightPadding;
        }

        // ── Shared utility ───────────────────────────────────────────────────

        /// <summary>
        /// Returns a <see cref="Rect"/> aligned to the right side of
        /// <paramref name="rowRect"/>, offset inward by <paramref name="offset"/>
        /// additional pixels.
        /// Utility for future phases that add more right-side elements.
        /// </summary>
        /// <param name="rowRect">The full row rect from the hierarchy callback.</param>
        /// <param name="width">Desired width of the returned rect.</param>
        /// <param name="offset">Additional offset from the right edge (default 0).</param>
        /// <returns>Right-aligned rect inside <paramref name="rowRect"/>.</returns>
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
