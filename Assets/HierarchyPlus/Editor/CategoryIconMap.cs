using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// CategoryIconMap — Loads, caches, and serves Texture2D icons for each
    /// HierarchyCategory using Unity's built-in EditorGUIUtility.IconContent.
    ///
    /// Design:
    ///   - Icons are loaded once and cached in a dictionary (lazy init).
    ///   - Per-category custom texture overrides are supported.
    ///   - Falls back to a default GameObject icon if nothing is found.
    ///   - Cache is cleared on domain reload via [InitializeOnLoad] in Core.
    ///
    /// Built-in icon names come from Unity's internal icon database.
    /// Use the UnityEditor IconBrowser (Window > Icon Browser) to find more.
    /// </summary>
    public static class CategoryIconMap
    {
        // ── Built-in icon name table ─────────────────────────────────────────

        /// <summary>
        /// Maps each category to a Unity built-in icon name string.
        /// These names are passed to EditorGUIUtility.IconContent().
        /// </summary>
        private static readonly Dictionary<HierarchyCategory, string> BuiltInIconNames =
            new Dictionary<HierarchyCategory, string>
        {
            { HierarchyCategory.None,        ""                              },
            { HierarchyCategory.Entity,      "Prefab Icon"                   },
            { HierarchyCategory.Player,      "Avatar Icon"                   },
            { HierarchyCategory.Enemy,       "d_CapsuleCollider Icon"        },
            { HierarchyCategory.UI,          "Canvas Icon"            },
            { HierarchyCategory.Manager,     "ScriptableObject Icon"         },
            { HierarchyCategory.Environment, "Terrain Icon"                  },
            { HierarchyCategory.Audio,       "AudioSource Icon"              },
        };

        // ── Icon cache ───────────────────────────────────────────────────────

        /// <summary>Cached textures resolved from built-in icon names.</summary>
        private static readonly Dictionary<HierarchyCategory, Texture2D> IconCache =
            new Dictionary<HierarchyCategory, Texture2D>();

        /// <summary>
        /// Per-category custom texture overrides set at runtime by the user.
        /// Checked before the built-in cache.
        /// </summary>
        private static readonly Dictionary<HierarchyCategory, Texture2D> CustomOverrides =
            new Dictionary<HierarchyCategory, Texture2D>();

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the icon Texture2D for a category.
        /// Priority: custom override → built-in Unity icon → null.
        /// </summary>
        public static Texture2D GetIcon(HierarchyCategory category)
        {
            if (category == HierarchyCategory.None) return null;

            // 1. Check custom override first.
            if (CustomOverrides.TryGetValue(category, out var custom) && custom != null)
                return custom;

            // 2. Check cache.
            if (IconCache.TryGetValue(category, out var cached))
                return cached;

            // 3. Load from Unity built-in icons.
            Texture2D loaded = null;
            if (BuiltInIconNames.TryGetValue(category, out var iconName) &&
                !string.IsNullOrEmpty(iconName))
            {
                var content = EditorGUIUtility.IconContent(iconName);
                loaded = content?.image as Texture2D;
            }

            // Store in cache even if null, so we don't retry every frame.
            IconCache[category] = loaded;
            return loaded;
        }

        /// <summary>
        /// Sets a custom Texture2D override for a category.
        /// Pass null to clear the override and fall back to built-in.
        /// </summary>
        public static void SetCustomIcon(HierarchyCategory category, Texture2D texture)
        {
            if (texture == null)
                CustomOverrides.Remove(category);
            else
                CustomOverrides[category] = texture;
        }

        /// <summary>
        /// Clears the built-in icon cache.
        /// Called automatically on domain reload via HierarchyPlusCore.
        /// </summary>
        public static void ClearCache()
        {
            IconCache.Clear();
        }
    }
}
