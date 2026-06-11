using UnityEditor;
using UnityEngine;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// ComponentInspectorPopup — Phase 7
    ///
    /// A PopupWindowContent that renders the real Unity Inspector for a
    /// single component. Opens automatically when the mouse hovers a minimap icon.
    ///
    /// Hover behaviour:
    ///   - Popup appears while the mouse is over an icon.
    ///   - Switches instantly to a different component when hovering another icon.
    ///   - Closes when the mouse leaves all minimap icons.
    ///
    /// Design:
    ///   - Static singleton so only one popup is ever open at a time.
    ///   - Uses Editor.CreateEditor() for the real Inspector drawer.
    ///   - Fixed width 350px, auto-height up to 400px then scrollable.
    ///   - Header: component icon + name + Script/Built-in badge.
    ///   - Cleans up the created Editor on close to avoid leaks.
    /// </summary>
    public class ComponentInspectorPopup : PopupWindowContent
    {
        // ── Singleton / static show-hide ─────────────────────────────────────

        private static ComponentInspectorPopup _current;
        private static Component               _currentComponent;

        /// <summary>
        /// Show the popup for <paramref name="comp"/> anchored at
        /// <paramref name="activatorRect"/>. If the same component is already
        /// showing, does nothing. If a different one is showing, replaces it.
        /// </summary>
        public static void ShowForComponent(Component comp, Rect activatorRect)
        {
            if (comp == null) return;

            // Already showing this component — nothing to do.
            if (_currentComponent == comp && _current != null) return;

            // Close previous if open.
            _current?.editorWindow?.Close();

            _currentComponent = comp;
            _current          = new ComponentInspectorPopup(comp);
            PopupWindow.Show(activatorRect, _current);
        }

        /// <summary>
        /// Close the popup if the mouse is no longer hovering any icon.
        /// Does not close if the user has interacted with the popup (future pin feature).
        /// </summary>
        public static void HideIfNotPinned()
        {
            if (_current == null) return;
            _current.editorWindow?.Close();
            _current          = null;
            _currentComponent = null;
        }

        // ── Layout ───────────────────────────────────────────────────────────

        private const float PopupWidth    = 350f;
        private const float HeaderHeight  = 28f;
        private const float MaxBodyHeight = 400f;
        private const float MinBodyHeight = 60f;
        private const float Padding       = 6f;

        // ── State ────────────────────────────────────────────────────────────

        private readonly Component      _component;
        private UnityEditor.Editor      _editor;
        private Vector2                 _scroll;
        private float                   _measuredHeight = MinBodyHeight;
        private bool                    _heightMeasured;

        // ── Styles ───────────────────────────────────────────────────────────

        private GUIStyle _headerStyle;
        private GUIStyle _typeStyle;

        // ── Constructor ──────────────────────────────────────────────────────

        private ComponentInspectorPopup(Component component)
        {
            _component = component;
        }

        // ── PopupWindowContent overrides ─────────────────────────────────────

        public override Vector2 GetWindowSize()
        {
            float bodyH = Mathf.Clamp(_measuredHeight, MinBodyHeight, MaxBodyHeight);
            return new Vector2(PopupWidth, HeaderHeight + bodyH + Padding);
        }

        public override void OnOpen()
        {
            if (_component != null)
                _editor = UnityEditor.Editor.CreateEditor(_component);
        }

        public override void OnClose()
        {
            if (_editor != null)
            {
                Object.DestroyImmediate(_editor);
                _editor = null;
            }

            // Clear singleton ref if this is the current popup.
            if (_current == this)
            {
                _current          = null;
                _currentComponent = null;
            }
        }

        public override void OnGUI(Rect rect)
        {
            EnsureStyles();

            if (_component == null)
            {
                EditorGUILayout.HelpBox("Component has been destroyed.", MessageType.Warning);
                return;
            }

            DrawHeader();

            // Separator line.
            EditorGUI.DrawRect(
                new Rect(0, HeaderHeight, PopupWidth, 1f),
                EditorGUIUtility.isProSkin
                    ? new Color(0.1f, 0.1f, 0.1f, 1f)
                    : new Color(0.6f, 0.6f, 0.6f, 1f));

            // Inspector body.
            float bodyY = HeaderHeight + 1f;
            float bodyH = rect.height - bodyY - Padding;
            DrawInspectorBody(new Rect(0, bodyY, PopupWidth, bodyH));
        }

        // ── Drawing helpers ──────────────────────────────────────────────────

        private void DrawHeader()
        {
            // Background.
            EditorGUI.DrawRect(
                new Rect(0, 0, PopupWidth, HeaderHeight),
                EditorGUIUtility.isProSkin
                    ? new Color(0.22f, 0.22f, 0.22f, 1f)
                    : new Color(0.82f, 0.82f, 0.82f, 1f));

            // Component icon.
            var icon = AssetPreview.GetMiniThumbnail(_component);
            if (icon != null)
            {
                float iconSize = HeaderHeight - 8f;
                float iconY    = (HeaderHeight - iconSize) * 0.5f;
                GUI.DrawTexture(new Rect(Padding, iconY, iconSize, iconSize),
                                icon, ScaleMode.ScaleToFit, true);
            }

            // Component name.
            float textX = Padding + HeaderHeight;
            GUI.Label(
                new Rect(textX, 0, PopupWidth - textX - 60f, HeaderHeight),
                _component.GetType().Name,
                _headerStyle);

            // Script / Built-in badge.
            bool   isScript  = _component is MonoBehaviour;
            string badge     = isScript ? "Script" : "Built-in";
            var    badgeColor = isScript
                ? new Color(0.25f, 0.65f, 0.35f, 1f)
                : new Color(0.30f, 0.55f, 0.90f, 1f);

            var badgeRect = new Rect(PopupWidth - 56f, (HeaderHeight - 14f) * 0.5f, 52f, 14f);
            EditorGUI.DrawRect(badgeRect, badgeColor);
            GUI.Label(badgeRect, badge, _typeStyle);
        }

        private void DrawInspectorBody(Rect bodyRect)
        {
            if (_editor == null) return;

            GUILayout.BeginArea(bodyRect);

            if (!_heightMeasured)
            {
                // First pass: measure natural height.
                var measured = EditorGUILayout.BeginVertical();
                DrawEditorGUI();
                EditorGUILayout.EndVertical();

                if (Event.current.type == EventType.Repaint && measured.height > 0)
                {
                    _measuredHeight = measured.height + Padding * 2f;
                    _heightMeasured = true;
                    editorWindow?.Repaint();
                }
            }
            else
            {
                bool needsScroll = _measuredHeight > MaxBodyHeight;
                if (needsScroll)
                    _scroll = GUILayout.BeginScrollView(_scroll,
                                  GUILayout.Width(PopupWidth),
                                  GUILayout.Height(bodyRect.height));

                DrawEditorGUI();

                if (needsScroll)
                    GUILayout.EndScrollView();
            }

            GUILayout.EndArea();
        }

        private void DrawEditorGUI()
        {
            if (_editor == null) return;
            try
            {
                EditorGUI.indentLevel = 0;
                _editor.OnInspectorGUI();
            }
            catch (System.Exception ex)
            {
                EditorGUILayout.HelpBox($"Could not draw inspector:\n{ex.Message}", MessageType.Error);
            }
        }

        // ── Style init ───────────────────────────────────────────────────────

        private void EnsureStyles()
        {
            if (_headerStyle != null) return;

            _headerStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize  = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
            };
            _headerStyle.normal.textColor =
                EditorGUIUtility.isProSkin ? Color.white : Color.black;

            _typeStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize  = 8,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
            };
            _typeStyle.normal.textColor = Color.white;
        }
    }
}
