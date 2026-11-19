using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// CategoryColorMap — Provides the color and icon emoji for each
    /// HierarchyCategory value.
    ///
    /// Design notes:
    ///   - Colors use EditorGUIUtility.isProSkin to stay readable in both
    ///     Dark and Light Unity themes.
    ///   - Icons are Unicode emoji strings rendered via Unity's label GUI.
    ///     Phase 4 will replace/augment these with real Texture2D icons.
    ///   - All lookups are dictionary-based — O(1), no switch overhead.
    /// </summary>
    public static class CategoryColorMap
    {
        // ── Color tables ─────────────────────────────────────────────────────

        /// <summary>Background tint color per category (dark theme).</summary>
        private static readonly Dictionary<HierarchyCategory, Color> DarkColors =
            new Dictionary<HierarchyCategory, Color>
        {
            { HierarchyCategory.None,        new Color(0f,    0f,    0f,    0f)    },
            { HierarchyCategory.Entity,      new Color(1.0f,  0.85f, 0.2f,  0.25f) }, // Yellow
            { HierarchyCategory.Player,      new Color(0.2f,  0.5f,  1.0f,  0.25f) }, // Blue
            { HierarchyCategory.Enemy,       new Color(1.0f,  0.25f, 0.25f, 0.25f) }, // Red
            { HierarchyCategory.UI,          new Color(0.9f,  0.9f,  0.9f,  0.20f) }, // White
            { HierarchyCategory.Manager,     new Color(0.6f,  0.6f,  0.6f,  0.25f) }, // Gray
            { HierarchyCategory.Environment, new Color(0.25f, 0.8f,  0.3f,  0.25f) }, // Green
            { HierarchyCategory.Audio,       new Color(1.0f,  0.55f, 0.1f,  0.25f) }, // Orange
        };

        /// <summary>Background tint color per category (light theme).</summary>
        private static readonly Dictionary<HierarchyCategory, Color> LightColors =
            new Dictionary<HierarchyCategory, Color>
        {
            { HierarchyCategory.None,        new Color(0f,    0f,    0f,    0f)    },
            { HierarchyCategory.Entity,      new Color(1.0f,  0.85f, 0.2f,  0.15f) }, // Yellow
            { HierarchyCategory.Player,      new Color(0.2f,  0.5f,  1.0f,  0.15f) }, // Blue
            { HierarchyCategory.Enemy,       new Color(1.0f,  0.25f, 0.25f, 0.15f) }, // Red
            { HierarchyCategory.UI,          new Color(0.5f,  0.5f,  0.5f,  0.10f) }, // White
            { HierarchyCategory.Manager,     new Color(0.4f,  0.4f,  0.4f,  0.15f) }, // Gray
            { HierarchyCategory.Environment, new Color(0.25f, 0.8f,  0.3f,  0.15f) }, // Green
            { HierarchyCategory.Audio,       new Color(1.0f,  0.55f, 0.1f,  0.15f) }, // Orange
        };

        // ── Icon table (emoji / text symbols) ────────────────────────────────

        /// <summary>
        /// Emoji icon shown to the left of the GameObject name.
        /// Phase 4 will extend this with real Texture2D icons.
        /// </summary>
        private static readonly Dictionary<HierarchyCategory, string> Icons =
            new Dictionary<HierarchyCategory, string>
        {
            { HierarchyCategory.None,        ""   },
            { HierarchyCategory.Entity,      "🧩" },
            { HierarchyCategory.Player,      "🎮" },
            { HierarchyCategory.Enemy,       "👾" },
            { HierarchyCategory.UI,          "🖼" },
            { HierarchyCategory.Manager,     "⚙"  },
            { HierarchyCategory.Environment, "🌿" },
            { HierarchyCategory.Audio,       "🔊" },
        };

        // ── Icon Texture cache (for SetIconForObject) ────────────────────────────

        private static readonly Dictionary<HierarchyCategory, Texture2D> IconTextureCache =
            new Dictionary<HierarchyCategory, Texture2D>();

        /// <summary>
        /// Emoji characters to render onto the generated icon texture per category.
        /// Keyed the same as Icons but used for texture generation.
        /// </summary>
        private static readonly Dictionary<HierarchyCategory, string> EmojiForTexture =
            new Dictionary<HierarchyCategory, string>
        {
            { HierarchyCategory.Entity,      "🧩" },
            { HierarchyCategory.Player,      "🎮" },
            { HierarchyCategory.Enemy,       "👾" },
            { HierarchyCategory.UI,          "🖼" },
            { HierarchyCategory.Manager,     "⚙"  },
            { HierarchyCategory.Environment, "🌿" },
            { HierarchyCategory.Audio,       "🔊" },
        };

// ── Solid color table (full opacity, for icon background fill) ───────

        private static readonly Dictionary<HierarchyCategory, Color> SolidColors =
            new Dictionary<HierarchyCategory, Color>
        {
            { HierarchyCategory.None,        Color.clear                    },
            { HierarchyCategory.Entity,      new Color(0.95f, 0.80f, 0.10f) }, // Yellow
            { HierarchyCategory.Player,      new Color(0.20f, 0.50f, 1.00f) }, // Blue
            { HierarchyCategory.Enemy,       new Color(0.90f, 0.20f, 0.20f) }, // Red
            { HierarchyCategory.UI,          new Color(0.50f, 0.50f, 0.50f) }, // Gray
            { HierarchyCategory.Manager,     new Color(0.50f, 0.50f, 0.50f) }, // Gray
            { HierarchyCategory.Environment, new Color(0.20f, 0.75f, 0.25f) }, // Green
            { HierarchyCategory.Audio,       new Color(1.00f, 0.50f, 0.05f) }, // Orange
        };

        
// ── Public API ────────────────────────────────────────────────────────

        /// <summary>Returns the background tint color for a category.</summary>
        public static Color GetColor(HierarchyCategory category)
        {
            var table = EditorGUIUtility.isProSkin ? DarkColors : LightColors;
            return table.TryGetValue(category, out var c) ? c : Color.clear;
        }

        /// <summary>Returns the full-opacity color for the icon background fill.</summary>
        public static Color GetSolidColor(HierarchyCategory category)
        {
            return SolidColors.TryGetValue(category, out var c) ? c : Color.clear;
        }

        
/// <summary>Returns the emoji icon string for a category.</summary>
        /// <summary>
        /// Returns a 16x16 Texture2D for use with EditorGUIUtility.SetIconForObject.
        /// Generates a colored circle texture with the category color on first call,
        /// then caches it. Returns null for HierarchyCategory.None.
        /// </summary>
        public static Texture2D GetIconTexture(HierarchyCategory category)
        {
            if (category == HierarchyCategory.None) return null;

            if (IconTextureCache.TryGetValue(category, out var cached))
                return cached;

            const int size = 16;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.hideFlags = HideFlags.HideAndDontSave;

            // Get the base category color (use full alpha for the icon).
            var table = EditorGUIUtility.isProSkin ? DarkColors : LightColors;
            table.TryGetValue(category, out var tint);
            var iconColor = new Color(tint.r, tint.g, tint.b, 1f);

            // Draw a filled circle on a transparent background.
            float cx = size * 0.5f, cy = size * 0.5f, r = size * 0.5f - 1f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - cx, dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    // Smooth edge with 1px antialiasing.
                    float alpha = Mathf.Clamp01(r - dist + 0.5f);
                    tex.SetPixel(x, y, new Color(iconColor.r, iconColor.g, iconColor.b, alpha));
                }
            }
            tex.Apply();

            IconTextureCache[category] = tex;
            return tex;
        }

        public static string GetIcon(HierarchyCategory category)
        {
            return Icons.TryGetValue(category, out var icon) ? icon : string.Empty;
        }
    }
}
