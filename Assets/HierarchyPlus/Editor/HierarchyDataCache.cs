using System.Collections.Generic;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// HierarchyDataCache
    ///
    /// Caches per-GameObject tag component lookups so that
    /// HierarchyPlusDrawer never calls GetComponent during a repaint.
    ///
    /// Problem solved:
    ///   Unity's Hierarchy window repaints every frame while the window is
    ///   visible. Each repaint calls OnHierarchyItemGUI for every visible row.
    ///   Without a cache, two GetComponent calls (CategoryTag + StatusTag) fire
    ///   per row per frame — causing continuous GC pressure and CPU overhead.
    ///
    /// Strategy — sentinel dictionary:
    ///   We store the component reference directly. A missing key means
    ///   "not yet looked up". A stored null means "looked up, component absent".
    ///   This lets us distinguish the two states without a separate bool flag.
    ///
    ///   Dictionary&lt;int, T&gt; where key = GameObject.GetInstanceID().
    ///
    /// Invalidation:
    ///   InvalidateAll() is called from HierarchyPlusCore.OnHierarchyChanged().
    ///   Individual entries are not invalidated because the instance ID is
    ///   stable for the lifetime of a GameObject in the scene.
    ///
    /// Thread safety:
    ///   Editor-only. All calls happen on the main thread during GUI events.
    ///   No locking required.
    ///
    /// SOLID notes:
    ///   Single Responsibility : tag-component caching only.
    ///   Open / Closed         : add new tag types by adding a new inner Cache&lt;T&gt;.
    /// </summary>
    public static class HierarchyDataCache
    {
        // ── Generic inner cache ──────────────────────────────────────────────

        /// <summary>
        /// Type-safe cache for a single component type T.
        /// Stores component references (or null) keyed by instance ID.
        /// A missing key = "not yet queried". A null value = "component absent".
        /// </summary>
        private sealed class Cache<T> where T : Component
        {
            // Pre-allocate with a reasonable initial capacity to avoid rehashing.
            private readonly Dictionary<int, T> _dict = new Dictionary<int, T>(256);

            /// <summary>
            /// Returns the cached T for this GameObject, querying if needed.
            /// Returns null if the component does not exist (result is cached).
            /// </summary>
            public T Get(GameObject go)
            {
                int id = go.GetInstanceID();

                // TryGetValue returns false for missing key AND for stored-null;
                // we need to distinguish them. Use ContainsKey as the gate.
                if (_dict.ContainsKey(id))
                    return _dict[id];         // may be null — that is intentional

                var comp = go.GetComponent<T>();
                _dict[id] = comp;             // null is a valid cached result
                return comp;
            }

            /// <summary>Clears all cached entries.</summary>
            public void Clear() => _dict.Clear();
        }

        // ── Per-type caches ──────────────────────────────────────────────────

        private static readonly Cache<HierarchyCategoryTag>   _categoryCache = new Cache<HierarchyCategoryTag>();
        private static readonly Cache<DevelopmentStatusTag>   _statusCache   = new Cache<DevelopmentStatusTag>();

        // ── Public accessors ─────────────────────────────────────────────────

        /// <summary>
        /// Returns the cached <see cref="HierarchyCategoryTag"/> for <paramref name="go"/>,
        /// or null if the GameObject has none.
        /// </summary>
        public static HierarchyCategoryTag GetCategoryTag(GameObject go)
            => _categoryCache.Get(go);

        /// <summary>
        /// Returns the cached <see cref="DevelopmentStatusTag"/> for <paramref name="go"/>,
        /// or null if the GameObject has none.
        /// </summary>
        public static DevelopmentStatusTag GetStatusTag(GameObject go)
            => _statusCache.Get(go);

        // ── Cache management ─────────────────────────────────────────────────

        /// <summary>
        /// Clears all caches. Call this from HierarchyPlusCore.OnHierarchyChanged()
        /// so stale entries are evicted whenever the scene hierarchy is modified.
        /// </summary>
        public static void InvalidateAll()
        {
            _categoryCache.Clear();
            _statusCache.Clear();
        }
    }
}
