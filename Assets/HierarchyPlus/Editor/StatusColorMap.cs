using UnityEngine;
using System.Collections.Generic;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// StatusColorMap — Provides badge colors, short labels, and tooltip text
    /// </summary>
    public static class StatusColorMap
    {
        // ── Badge colors ─────────────────────────────────────────────────────

        /// <summary>Maps each status to its badge background color.</summary>
        private static readonly Dictionary<DevelopmentStatus, Color> Colors =
            new Dictionary<DevelopmentStatus, Color>
        {
            { DevelopmentStatus.None,           Color.clear                    },
            { DevelopmentStatus.New,            new Color(0.20f, 0.50f, 1.00f) }, // Blue
            { DevelopmentStatus.WorkInProgress, new Color(1.00f, 0.85f, 0.10f) }, // Yellow
            { DevelopmentStatus.Complete,       new Color(0.20f, 0.80f, 0.30f) }, // Green
            { DevelopmentStatus.Problem,        new Color(0.95f, 0.20f, 0.20f) }, // Red
            { DevelopmentStatus.Review,         new Color(0.65f, 0.25f, 0.90f) }, // Purple
            { DevelopmentStatus.Deprecated,     new Color(0.50f, 0.50f, 0.50f) }, // Gray
        };

        // ── Short badge labels ────────────────────────────────────────────────

        /// <summary>Short all-caps label drawn inside the badge pill.</summary>
        private static readonly Dictionary<DevelopmentStatus, string> Labels =
            new Dictionary<DevelopmentStatus, string>
        {
            { DevelopmentStatus.None,           ""     },
            { DevelopmentStatus.New,            "NEW"  },
            { DevelopmentStatus.WorkInProgress, "WIP"  },
            { DevelopmentStatus.Complete,       "DONE" },
            { DevelopmentStatus.Problem,        "BUG"  },
            { DevelopmentStatus.Review,         "REV"  },
            { DevelopmentStatus.Deprecated,     "DEP"  },
        };

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Returns the badge background color for a given status.</summary>
        /// <param name="status">The <see cref="DevelopmentStatus"/> to look up.</param>
        /// <returns>The badge color, or <see cref="Color.clear"/> for None.</returns>
        public static Color GetColor(DevelopmentStatus status)
        {
            return Colors.TryGetValue(status, out var c) ? c : Color.clear;
        }

        /// <summary>Returns the short all-caps badge label for a given status.</summary>
        /// <param name="status">The <see cref="DevelopmentStatus"/> to look up.</param>
        /// <returns>A short string such as "WIP" or "DONE", or empty for None.</returns>
        public static string GetLabel(DevelopmentStatus status)
        {
            return Labels.TryGetValue(status, out var l) ? l : string.Empty;
        }

        /// <summary>
        /// Builds the full multi-line tooltip string shown when hovering a badge.
        /// Includes status, assigned developer (if set), and notes (if set).
        /// </summary>
        /// <param name="tag">The <see cref="DevelopmentStatusTag"/> to read from.</param>
        /// <returns>A trimmed multi-line string suitable for a Unity GUIContent tooltip.</returns>
        public static string BuildTooltip(DevelopmentStatusTag tag)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"Status: {tag.status}");

            if (!string.IsNullOrEmpty(tag.assignedDeveloper))
                sb.AppendLine($"Developer: {tag.assignedDeveloper}");

            if (!string.IsNullOrEmpty(tag.notes))
                sb.AppendLine($"Notes: {tag.notes}");

            return sb.ToString().TrimEnd();
        }
    }
}
