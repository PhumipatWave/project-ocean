using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// Responsible for finding the MonoScript asset that backs a given
    /// Component and opening it in the configured external editor
    /// (Rider, Visual Studio, VS Code, etc.).
    ///
    /// Usage:
    ///   Alt + Click any MonoBehaviour icon in the minimap.
    ///
    /// Supported source types:
    ///   - MonoBehaviour subclasses  (most common case)
    ///   - ScriptableObject subclasses
    ///
    /// Unity delegates the actual application launch to
    ///   AssetDatabase.OpenAsset()
    /// which respects the "External Script Editor" preference set in
    ///   Edit → Preferences → External Tools.
    ///
    /// SOLID notes:
    ///   Single Responsibility : script resolution + IDE open only.
    ///                           No drawing, no cache, no event handling.
    ///   Open / Closed         : add new resolution strategies by extending
    ///                           the TryResolveMonoScript helper.
    /// </summary>
    public static class ScriptNavigator
    {
        // ── Public API ──────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to locate the C# source file for <paramref name="component"/>
        /// and open it in the external IDE.
        /// </summary>
        /// <param name="component">The component whose script should be opened.</param>
        /// <returns>
        ///   <c>true</c> if a MonoScript was found and the open request was issued;
        ///   <c>false</c> if the component is not script-backed or the asset could
        ///   not be located.
        /// </returns>
        public static bool TryOpenScript(Component component)
        {
            if (component == null) return false;

            var monoScript = TryResolveMonoScript(component);
            if (monoScript == null) return false;

            // AssetDatabase.OpenAsset returns true if the IDE accepted the request.
            return AssetDatabase.OpenAsset(monoScript);
        }

        /// <summary>
        /// Overload that accepts a <see cref="MonoScript"/> directly.
        /// Useful when the caller already has the asset reference.
        /// </summary>
        public static bool TryOpenScript(MonoScript monoScript)
        {
            if (monoScript == null) return false;
            return AssetDatabase.OpenAsset(monoScript);
        }

        /// <summary>
        /// Returns the <see cref="MonoScript"/> asset for <paramref name="component"/>,
        /// or <c>null</c> if none can be resolved.
        /// </summary>
        public static MonoScript TryResolveMonoScript(Component component)
        {
            if (component == null) return null;

            // MonoBehaviour is the only script-backed Component type.
            // Built-in components (Rigidbody, Collider, Animator, etc.)
            // have no backing MonoScript — return null intentionally.
            if (component is MonoBehaviour mb)
                return MonoScript.FromMonoBehaviour(mb);

            return null;
        }

        /// <summary>
        /// Returns <c>true</c> when <paramref name="component"/> has a backing
        /// C# script asset that can be opened.
        /// This is used by the minimap to decide whether to handle Alt+Click.
        /// </summary>
        public static bool IsScriptBacked(Component component)
        {
            if (component == null) return false;
            return component is MonoBehaviour;
        }
    }
}
