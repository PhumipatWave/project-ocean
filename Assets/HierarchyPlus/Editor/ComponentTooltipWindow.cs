using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// ComponentTooltipWindow
    ///
    /// A lightweight floating EditorWindow that shows component details
    /// (name, namespace, assembly) when the mouse hovers over a minimap icon.
    ///
    /// Design:
    ///   - Single shared instance (singleton pattern) — never more than one open.
    ///   - Shown via ShowPopup() so it has no titlebar and no OS chrome.
    ///   - Positioned to follow the mouse cursor with a small offset.
    ///   - Auto-hides when the mouse leaves all known icon rects, or when the
    ///     Hierarchy window loses focus.
    ///   - Deliberately NOT an EditorWindow that steals focus — uses
    ///     ShowPopup + wantsMouseMove to receive move events without focus grab.
    ///
    /// Called from ComponentMinimap.Draw() every repaint.
    /// </summary>
    public class ComponentTooltipWindow : EditorWindow
    {
        // ── Singleton ────────────────────────────────────────────────────────

        private static ComponentTooltipWindow _instance;

        // ── Content ──────────────────────────────────────────────────────────

        private string _componentName;
        private string _namespace;
        private string _assembly;

        // ── Layout ───────────────────────────────────────────────────────────

        private const float WindowWidth   = 220f;
        private const float WindowHeight  = 52f;
        private const float CursorOffsetX = 14f;
        private const float CursorOffsetY = 6f;
        private const float Padding       = 8f;

        // ── Styles (created once inside OnGUI) ───────────────────────────────

        private GUIStyle _nameStyle;
        private GUIStyle _detailStyle;

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Show (or update) the tooltip window near the given screen position.
        /// Call every repaint while the mouse is hovering an icon.
        /// </summary>
        public static void Show(Component comp, Vector2 screenPosition)
        {
            if (comp == null) return;

            var type = comp.GetType();

            // Create the window only once; update content every call.
            if (_instance == null)
            {
                _instance = CreateInstance<ComponentTooltipWindow>();
                _instance.wantsMouseMove = true;
            }

            _instance._componentName = type.Name;
            _instance._namespace     = string.IsNullOrEmpty(type.Namespace)
                                           ? "(global namespace)"
                                           : type.Namespace;
            _instance._assembly      = type.Assembly.GetName().Name;

            // Position the window just below-right of the cursor.
            float x = screenPosition.x + CursorOffsetX;
            float y = screenPosition.y + CursorOffsetY;

            // Keep it on-screen horizontally.
            float screenW = Screen.currentResolution.width;
            if (x + WindowWidth > screenW) x = screenPosition.x - WindowWidth - 4f;

            _instance.minSize = new Vector2(WindowWidth, WindowHeight);
            _instance.maxSize = new Vector2(WindowWidth, WindowHeight);
            _instance.ShowPopup();
            _instance.position = new Rect(x, y, WindowWidth, WindowHeight);
        }

        /// <summary>
        /// Hide the tooltip window if it is currently open.
        /// Call when the mouse leaves an icon rect.
        /// </summary>
        public static void Hide()
        {
            if (_instance != null)
            {
                _instance.Close();
                _instance = null;
            }
        }

        /// <summary>True if the tooltip window is currently visible.</summary>
        public static bool IsVisible => _instance != null;

        // ── EditorWindow lifecycle ────────────────────────────────────────────

        private void OnGUI()
        {
            EnsureStyles();

            // Dark rounded background.
            var bgColor = EditorGUIUtility.isProSkin
                ? new Color(0.15f, 0.15f, 0.15f, 0.97f)
                : new Color(0.88f, 0.88f, 0.88f, 0.97f);

            var prevColor = GUI.color;
            GUI.color = bgColor;
            GUI.DrawTexture(new Rect(0, 0, WindowWidth, WindowHeight),
                            EditorGUIUtility.whiteTexture);
            GUI.color = prevColor;

            // Left accent bar (teal).
            var accentColor = new Color(0.25f, 0.75f, 0.75f, 1f);
            GUI.color = accentColor;
            GUI.DrawTexture(new Rect(0, 0, 3f, WindowHeight), EditorGUIUtility.whiteTexture);
            GUI.color = prevColor;

            // Content area.
            float contentX = 3f + Padding;
            float contentW = WindowWidth - contentX - Padding;

            // Component name (bold, larger).
            GUI.Label(new Rect(contentX, 6f, contentW, 18f), _componentName, _nameStyle);

            // Namespace.
            GUI.Label(new Rect(contentX, 24f, contentW, 13f), _namespace, _detailStyle);

            // Assembly.
            GUI.Label(new Rect(contentX + 0f, 37f, contentW, 13f), _assembly, _detailStyle);
        }

        private void OnLostFocus()
        {
            // Close if the user clicks elsewhere.
            Hide();
        }

        // ── Style setup ──────────────────────────────────────────────────────

        private void EnsureStyles()
        {
            if (_nameStyle != null) return;

            var textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            var dimColor  = EditorGUIUtility.isProSkin
                                ? new Color(0.75f, 0.75f, 0.75f)
                                : new Color(0.35f, 0.35f, 0.35f);

            _nameStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize  = 11,
                fontStyle = FontStyle.Bold,
            };
            _nameStyle.normal.textColor = textColor;

            _detailStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize  = 9,
                fontStyle = FontStyle.Normal,
            };
            _detailStyle.normal.textColor = dimColor;
        }
    }
}
