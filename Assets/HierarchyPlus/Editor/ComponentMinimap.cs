using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// ComponentMinimap — Phase 5 / 7
    ///
    /// Draws a row of small component icons on the RIGHT side of each
    /// Hierarchy item.
    ///
    /// Interaction model:
    ///   Hover icon       → opens ComponentInspectorPopup (real Unity Inspector).
    ///   Alt + Click icon → opens the script in the external IDE (Phase 8).
    ///
    /// Filtering strategy: BLACKLIST
    ///   - Always skip Transform / RectTransform.
    ///   - Skip noisy built-in UI / event / tilemap infrastructure by full type name.
    ///   - Everything else (Colliders, Rigidbody, Animator, AudioSource,
    ///     NavMeshAgent, custom MonoBehaviours, etc.) is shown.
    ///
    /// SOLID notes:
    ///   Single Responsibility : filtering + drawing + hover detection + caching.
    ///   Open / Closed         : extend BlacklistedTypeNames to suppress more types.
    /// </summary>
    public static class ComponentMinimap
    {
        // ── Layout constants ────────────────────────────────────────────────

        /// <summary>Size (px) of each component icon square.</summary>
        public const float IconSize = 14f;

        /// <summary>Gap (px) between consecutive icons.</summary>
        public const float IconSpacing = 2f;

        // ── Hard-type blacklist ─────────────────────────────────────────────

        private static readonly HashSet<System.Type> BlacklistedTypes = new HashSet<System.Type>
        {
            typeof(Transform),
            typeof(RectTransform),
        };

        // ── String-name blacklist (optional packages) ───────────────────────

        private static readonly HashSet<string> BlacklistedTypeNames = new HashSet<string>
        {
            // Canvas infrastructure
            "UnityEngine.CanvasRenderer",
            "UnityEngine.UI.CanvasScaler",
            "UnityEngine.UI.GraphicRaycaster",

            // UI layout helpers
            "UnityEngine.UI.LayoutElement",
            "UnityEngine.UI.ContentSizeFitter",
            "UnityEngine.UI.AspectRatioFitter",
            "UnityEngine.UI.HorizontalLayoutGroup",
            "UnityEngine.UI.VerticalLayoutGroup",
            "UnityEngine.UI.GridLayoutGroup",

            // UI visuals (leaf-level rendering detail)
            "UnityEngine.UI.Image",
            "UnityEngine.UI.RawImage",
            "UnityEngine.UI.Text",
            "TMPro.TextMeshProUGUI",
            "TMPro.TMP_Text",
            "TMPro.TextMeshPro",

            // UI decoration
            "UnityEngine.UI.Shadow",
            "UnityEngine.UI.Outline",
            "UnityEngine.UI.PositionAsUV1",

            // Event system infrastructure
            "UnityEngine.EventSystems.EventSystem",
            "UnityEngine.EventSystems.EventTrigger",
            "UnityEngine.EventSystems.StandaloneInputModule",
            "UnityEngine.EventSystems.BaseInputModule",
            "UnityEngine.EventSystems.PhysicsRaycaster",
            "UnityEngine.EventSystems.Physics2DRaycaster",

            // Tilemap plumbing (Tilemap data component itself is NOT blacklisted)
            "UnityEngine.Tilemaps.TilemapRenderer",

            // Mask / clipping
            "UnityEngine.UI.Mask",
            "UnityEngine.UI.RectMask2D",

            // Canvas group
            "UnityEngine.CanvasGroup",
        };

        // ── Caches ──────────────────────────────────────────────────────────

        private static readonly Dictionary<System.Type, bool>       BlacklistCache = new Dictionary<System.Type, bool>();
        private static readonly Dictionary<int, Component[]>         ComponentCache = new Dictionary<int, Component[]>();
        private static readonly Dictionary<System.Type, Texture2D>  IconCache      = new Dictionary<System.Type, Texture2D>();

        // ── Hover state ─────────────────────────────────────────────────────

        /// <summary>Component under the mouse this repaint pass, or null.</summary>
        private static Component _hoveredComponent;

        /// <summary>Icon rect (GUI space) of the hovered component.</summary>
        private static Rect _hoveredIconRect;

        // ── Public API ──────────────────────────────────────────────────────

        /// <summary>
        /// Called from HierarchyPlusDrawer.OnHierarchyItemGUI.
        /// Draws minimap icons and tracks which one the mouse is over.
        /// </summary>
        public static float Draw(GameObject go, Rect selectionRect, float rightOffset = 0f)
        {
            var components = GetCachedComponents(go);
            if (components == null || components.Length == 0) return 0f;

            int   count      = components.Length;
            float totalWidth = count * IconSize + (count - 1) * IconSpacing;
            float startX     = selectionRect.xMax - rightOffset - totalWidth;

            var ev       = Event.current;
            var mousePos = ev?.mousePosition ?? Vector2.zero;

            for (int i = 0; i < count; i++)
            {
                var   comp     = components[i];
                float x        = startX + i * (IconSize + IconSpacing);
                float y        = selectionRect.y + (selectionRect.height - IconSize) * 0.5f;
                var   iconRect = new Rect(x, y, IconSize, IconSize);

                // ── Missing script slot ─────────────────────────────────────
                if (comp == null)
                {
                    DrawMissingScriptIcon(iconRect);
                    continue;
                }

                // ── Icon ────────────────────────────────────────────────────
                var icon = GetCachedIcon(comp);
                if (icon != null)
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
                else
                    DrawFallbackIcon(iconRect);

                // ── Hover detection → inspector popup ───────────────────────
                if (ev != null && iconRect.Contains(mousePos))
                {
                    _hoveredComponent = comp;
                    _hoveredIconRect  = iconRect;
                    EditorApplication.RepaintHierarchyWindow();
                }

                // ── Alt+Click → open script in IDE (Phase 8) ────────────────
                HandleAltClick(iconRect, comp, ev);
            }

            return totalWidth;
        }

        /// <summary>
        /// Called once per repaint after ALL rows have been drawn.
        /// Opens the inspector popup for whichever icon is currently hovered,
        /// or closes it when the mouse leaves all icons.
        /// </summary>
        public static void FlushHoverState()
        {
            if (_hoveredComponent != null)
            {
                // Show popup anchored just below the hovered icon rect.
                var activatorRect = new Rect(
                    _hoveredIconRect.x,
                    _hoveredIconRect.yMax,
                    _hoveredIconRect.width,
                    0f);

                ComponentInspectorPopup.ShowForComponent(_hoveredComponent, activatorRect);
            }
            else
            {
                // Mouse is not over any icon — close popup if open.
                ComponentInspectorPopup.HideIfNotPinned();
            }

            // Reset for next repaint.
            _hoveredComponent = null;
            _hoveredIconRect  = default;
        }

        /// <summary>Invalidates component-list cache. Wire to hierarchyChanged.</summary>
        public static void InvalidateCache()
        {
            ComponentCache.Clear();
        }

        /// <summary>Clears icon + blacklist caches. Call on domain reload.</summary>
        public static void ClearIconCache()
        {
            IconCache.Clear();
            BlacklistCache.Clear();
        }

        // ── Blacklist ───────────────────────────────────────────────────────

        private static bool IsBlacklisted(Component c)
        {
            var type = c.GetType();
            if (BlacklistCache.TryGetValue(type, out var cached)) return cached;

            bool result = BlacklistedTypes.Contains(type) ||
                          BlacklistedTypeNames.Contains(type.FullName);

            BlacklistCache[type] = result;
            return result;
        }

        // ── Component list ──────────────────────────────────────────────────

        private static Component[] GetCachedComponents(GameObject go)
        {
            int id = go.GetInstanceID();
            if (ComponentCache.TryGetValue(id, out var cached)) return cached;

            var all      = go.GetComponents<Component>();
            var filtered = new List<Component>(all.Length);
            foreach (var c in all)
            {
                if (c == null) { filtered.Add(null); continue; }
                if (!IsBlacklisted(c)) filtered.Add(c);
            }

            var result = filtered.ToArray();
            ComponentCache[id] = result;
            return result;
        }

        // ── Icon helpers ────────────────────────────────────────────────────

        private static Texture2D GetCachedIcon(Component comp)
        {
            var type = comp.GetType();
            if (IconCache.TryGetValue(type, out var cached)) return cached;
            var tex = AssetPreview.GetMiniThumbnail(comp);
            IconCache[type] = tex;
            return tex;
        }

        private static void DrawFallbackIcon(Rect r)
        {
            var prev  = GUI.color;
            GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            GUI.DrawTexture(r, EditorGUIUtility.whiteTexture);
            GUI.color = prev;
        }

        private static void DrawMissingScriptIcon(Rect r)
        {
            var warnIcon = EditorGUIUtility.IconContent("console.warnicon.sml");
            if (warnIcon?.image != null)
                GUI.DrawTexture(r, (Texture2D)warnIcon.image, ScaleMode.ScaleToFit, true);
        }

        // ── Alt+Click → open script ─────────────────────────────────────────

        private static void HandleAltClick(Rect iconRect, Component comp, Event ev)
        {
            if (ev == null || ev.type != EventType.MouseDown) return;
            if (!ev.alt) return;
            if (!iconRect.Contains(ev.mousePosition)) return;

            TryOpenScript(comp);
            ev.Use();
        }

        private static void TryOpenScript(Component comp)
        {
            if (!(comp is MonoBehaviour mb)) return;
            var monoScript = MonoScript.FromMonoBehaviour(mb);
            if (monoScript != null) AssetDatabase.OpenAsset(monoScript);
        }
    }
}
