using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Game.Core
{
    /// <summary>Replaces {key:Action} / {key:Move.Up} tokens in authored text with the player's current binding.</summary>
    public static class KeyBindText
    {
        private const string TokenStart = "{key:";
        private const string Unbound = "Unbound";

        private static readonly Regex Token = new(@"\{key:(\w+)(?:\.(\w+))?\}", RegexOptions.Compiled);

        public static string Format(string text)
        {
            if (string.IsNullOrEmpty(text) || !text.Contains(TokenStart))
                return text;

            return Token.Replace(text, match =>
            {
                string key = Resolve(match.Groups[1].Value, match.Groups[2].Value);
                if (key == null)
                {
                    Debug.LogWarning($"[KeyBindText] Unknown key token '{match.Value}' in: {text}");
                    return match.Value; // left visible so authoring typos get noticed
                }
                return $"<b>[{key}]</b>";
            });
        }

        private static string Resolve(string actionName, string directionName)
        {
            var manager = KeyBindManager.Instance;
            if (manager == null || !Enum.TryParse(actionName, out KeyAction action))
                return null;

            string display;
            if (string.IsNullOrEmpty(directionName))
                display = manager.GetDisplayString(action, ControlScheme.KeyboardMouse);
            else if (action == KeyAction.Move && Enum.TryParse(directionName, out MoveDirection direction))
                display = manager.GetMoveBindingDisplayString(direction);
            else
                return null;

            return string.IsNullOrEmpty(display) || display == "---" ? Unbound : display;
        }
    }
}
