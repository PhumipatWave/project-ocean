using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// ComponentMinimap — Phase 5
    ///
    /// Draws a row of small component icons on the RIGHT side of each
    /// Hierarchy item, similar to vHierarchy's component minimap.
    ///
    /// Filtering strategy: BLACKLIST
    ///   - Always skip Transform / RectTransform (structural noise).
    ///   - Skip known "invisible infrastructure" components by type name:
    ///       Canvas plumbing  : CanvasRenderer, CanvasScaler, GraphicRaycaster
    ///       Tilemap plumbing : TilemapRenderer, TilemapCollider2D (visual-only variant)
    ///       Layout helpers   : LayoutElement, ContentSizeFitter, AspectRatioFitter,
    ///                          HorizontalLayoutGroup, VerticalLayoutGroup, GridLayoutGroup
    ///       UI visuals       : Image, RawImage, Text, TextMeshProUGUI, TMP_Text
    ///       UI decoration    : Shadow, Outline, PositionAsUV1
    ///       Event infrastr.  : EventSystem, EventTrigger, StandaloneInputModule,
    ///                          BaseInputModule, PhysicsRaycaster, Physics2DRaycaster
    ///       Misc noise       : CanvasGroup (show only when standalone, not on every UI obj)
    ///   - Everything NOT on the blacklist is shown — including all Colliders,
    ///     Rigidbody/Rigidbody2D, Animator, AudioSource, NavMeshAgent, ParticleSystem,
    ///     Light, Camera, and every custom MonoBehaviour.
    ///
    /// Rationale:
    ///   Type-name string matching is used for optional modules (UnityEngine.UI,
    ///   TMPro, UnityEngine.Tilemaps) so the minimap compiles without hard
    ///   assembly references to those packages.
    ///
    /// SOLID notes:
    ///   Single Responsibility : filtering + drawing + caching only.
    ///   Open / Closed         : extend BlacklistedTypeNames to suppress more types.
    /// </summary>
    public static class ComponentMinimap
    {
        // ── Layout constants ────────────────────────────────────────────────

        /// <summary>Size (px) of each component icon square.</summary>
        public const float IconSize = 14f;

        /// <summary>Gap (px) between consecutive icons.</summary>
        public const float IconSpacing = 2f;

        /// <summary>Right-side margin before the row's right edge.</summary>
        private const float RightPadding = 4f;

        // ── Hard-type blacklist (no string lookup needed) ───────────────────

        /// <summary>
        /// Always-skipped types resolved at compile time.
        /// Add types here only when the assembly is guaranteed to be present
        /// (UnityEngine.CoreModule, UnityEngine.PhysicsModule, etc.).
        /// </summary>
        private static readonly HashSet<System.Type> BlacklistedTypes = new HashSet<System.Type>
        {
            // ── Transform (structural, always present, never informative) ──
            typeof(Transform),
            typeof(RectTransform),
        };

        // ── String-name blacklist (for optional packages) ───────────────────

        /// <summary>
        /// Full type names (namespace + class) of components to hide.
        /// Using strings avoids compile errors when optional packages
        /// (UnityEngine.UI, TMPro, Tilemap) are not installed.
        /// </summary>
        private static readonly HashSet<string> BlacklistedTypeNames = new HashSet<string>
        {
            // ── Canvas infrastructure ──────────────────────────────────────
            "UnityEngine.CanvasRenderer",       // auto-added to every UI element
            "UnityEngine.UI.CanvasScaler",      // root canvas helper
            "UnityEngine.UI.GraphicRaycaster",  // root canvas helper

            // ── UI layout helpers (pure structure, not gameplay-relevant) ──
            "UnityEngine.UI.LayoutElement",
            "UnityEngine.UI.ContentSizeFitter",
            "UnityEngine.UI.AspectRatioFitter",
            "UnityEngine.UI.HorizontalLayoutGroup",
            "UnityEngine.UI.VerticalLayoutGroup",
            "UnityEngine.UI.GridLayoutGroup",

            // ── UI visuals (text / image primitives) ──────────────────────
            // Keep these hidden — they are leaf-level rendering details;
            // the meaningful context is the MonoBehaviour controlling them.
            "UnityEngine.UI.Image",
            "UnityEngine.UI.RawImage",
            "UnityEngine.UI.Text",
            "TMPro.TextMeshProUGUI",
            "TMPro.TMP_Text",
            "TMPro.TextMeshPro",

            // ── UI decoration ─────────────────────────────────────────────
            "UnityEngine.UI.Shadow",
            "UnityEngine.UI.Outline",
            "UnityEngine.UI.PositionAsUV1",

            // ── Event system infrastructure ───────────────────────────────
            "UnityEngine.EventSystems.EventSystem",
            "UnityEngine.EventSystems.EventTrigger",
            "UnityEngine.EventSystems.StandaloneInputModule",
            "UnityEngine.EventSystems.BaseInputModule",
            "UnityEngine.EventSystems.PhysicsRaycaster",
            "UnityEngine.EventSystems.Physics2DRaycaster",

            // ── Tilemap plumbing ──────────────────────────────────────────
            "UnityEngine.Tilemaps.TilemapRenderer",  // visual companion to Tilemap
            // Note: Tilemap itself IS informative and is NOT blacklisted.

            // ── Mask / clipping helpers ───────────────────────────────────
            "UnityEngine.UI.Mask",
            "UnityEngine.UI.RectMask2D",

            // ── Canvas group (almost always auto-added noise) ─────────────
            // Remove this line if you want CanvasGroup to remain visible.
            "UnityEngine.CanvasGroup",
        };

        // ── Blacklist lookup cache ──────────────────────────────────────────

        /// <summary>
        /// Cache of IsBlacklisted results per type so the HashSet lookups
        /// only happen once per type across the entire session.
        /// </summary>
        private static readonly Dictionary<System.Type, bool> BlacklistCache =
            new Dictionary<System.Type, bool>();

        // ── Component-list cache ────────────────────────────────────────────

        /// <summary>
        /// Maps instanceID → filtered component array.
        /// Rebuilt lazily; wiped when hierarchyChanged fires.
        /// </summary>
        private static readonly Dictionary<int, Component[]> ComponentCache =
            new Dictionary<int, Component[]>();

        // ── Icon texture cache ──────────────────────────────────────────────

        /// <summary>
        /// Maps System.Type → mini thumbnail texture.
        /// AssetPreview.GetMiniThumbnail is cheap but still worth caching.
        /// </summary>
        private static readonly Dictionary<System.Type, Texture2D> IconCache =
            new Dictionary<System.Type, Texture2D>();

        // ── Public API ──────────────────────────────────────────────────────

        /// <summary>
        /// Call from HierarchyPlusDrawer.OnHierarchyItemGUI.
        /// Draws icons and returns the total pixel width consumed.
        /// </summary>
public static float Draw(GameObject go, Rect selectionRect, float rightOffset = 0f)
        {
            var components = GetCachedComponents(go);
            if (components == null || components.Length == 0) return 0f;

            int count = components.Length;

            float totalWidth = count * IconSize + (count - 1) * IconSpacing;

            // Start left of rightOffset so we never overlap the status badge.
            float startX = selectionRect.xMax - rightOffset - totalWidth;

            for (int i = 0; i < count; i++)
            {
                var comp = components[i];

                float x      = startX + i * (IconSize + IconSpacing);
                float y      = selectionRect.y + (selectionRect.height - IconSize) * 0.5f;
                var iconRect = new Rect(x, y, IconSize, IconSize);

                if (comp == null)
                {
                    DrawMissingScriptIcon(iconRect);
                    continue;
                }

                var icon = GetCachedIcon(comp);
                if (icon != null)
                    GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
                else
                    DrawFallbackIcon(iconRect);

                GUI.Label(iconRect, new GUIContent(string.Empty, BuildTooltip(comp)));
                HandleIconClick(iconRect, comp);
            }

            return totalWidth;
        }

        /// <summary>
        /// Invalidates the component-list cache for all GameObjects.
        /// Wired to EditorApplication.hierarchyChanged in HierarchyPlusCore.
        /// </summary>
        public static void InvalidateCache()
        {
            ComponentCache.Clear();
        }

        /// <summary>
        /// Clears icon texture + blacklist caches.
        /// Called on domain reload in HierarchyPlusCore.
        /// </summary>
        public static void ClearIconCache()
        {
            IconCache.Clear();
            BlacklistCache.Clear();
        }

        // ── Blacklist logic ─────────────────────────────────────────────────

        /// <summary>
        /// Returns true if the component's type should be hidden from the minimap.
        /// Result is cached per-type so HashSet lookups only run once per type.
        /// </summary>
        private static bool IsBlacklisted(Component c)
        {
            var type = c.GetType();

            if (BlacklistCache.TryGetValue(type, out var cached))
                return cached;

            // 1. Hard-type check (compile-time types).
            if (BlacklistedTypes.Contains(type))
            {
                BlacklistCache[type] = true;
                return true;
            }

            // 2. Full type-name check (optional packages, avoids compile deps).
            if (BlacklistedTypeNames.Contains(type.FullName))
            {
                BlacklistCache[type] = true;
                return true;
            }

            BlacklistCache[type] = false;
            return false;
        }

        // ── Component list building ─────────────────────────────────────────

        private static Component[] GetCachedComponents(GameObject go)
        {
            int id = go.GetInstanceID();
            if (ComponentCache.TryGetValue(id, out var cached))
                return cached;

            var all      = go.GetComponents<Component>();
            var filtered = new List<Component>(all.Length);

            foreach (var c in all)
            {
                // Keep null slots for Phase 9 missing-script detection.
                if (c == null) { filtered.Add(null); continue; }

                if (!IsBlacklisted(c))
                    filtered.Add(c);
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

        // ── Tooltip ─────────────────────────────────────────────────────────

        private static string BuildTooltip(Component comp)
        {
            var type         = comp.GetType();
            var assemblyName = type.Assembly.GetName().Name;
            var ns           = string.IsNullOrEmpty(type.Namespace) ? "(global)" : type.Namespace;
            return $"{type.Name}\n{ns}\n{assemblyName}";
        }

        // ── Click handling ──────────────────────────────────────────────────

        private static void HandleIconClick(Rect iconRect, Component comp)
        {
            var ev = Event.current;
            if (ev == null || ev.type != EventType.MouseDown) return;
            if (!iconRect.Contains(ev.mousePosition)) return;

            if (ev.alt)
            {
                TryOpenScript(comp); // Phase 8
                ev.Use();
            }
            else
            {
                ev.Use(); // Phase 7 popup placeholder
            }
        }

        private static void TryOpenScript(Component comp)
        {
            if (!(comp is MonoBehaviour mb)) return;
            var monoScript = MonoScript.FromMonoBehaviour(mb);
            if (monoScript != null) AssetDatabase.OpenAsset(monoScript);
        }
    }
}
