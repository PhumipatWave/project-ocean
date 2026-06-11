using UnityEngine;
using System.Collections.Generic;

namespace HierarchyPlus.Editor
{
    /// <summary>
    /// StatusColorMap — Provides colors and badge labels for each DevelopmentStatus.
    ///
    /// Badge colors:
    ///   New            → Blue
    ///   WorkInProgress → Yellow
    ///   Complete       → Green
    ///   Problem        → Red
    ///   Review         → Purple
    ///   Deprecated     → Gray
    /// </summary>
    public static class StatusColorMap
    {
        // ── Badge colors ─────────────────────────────────────────────────────

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

        /// <summary>Returns the badge color for a status.</summary>
        public static Color GetColor(DevelopmentStatus status)
        {
            return Colors.TryGetValue(status, out var c) ? c : Color.clear;
        }

        /// <summary>Returns the short badge label for a status.</summary>
        public static string GetLabel(DevelopmentStatus status)
        {
            return Labels.TryGetValue(status, out var l) ? l : string.Empty;
        }

        /// <summary>
        /// Builds the full tooltip string shown on hover.
        /// Includes status, assigned developer, and notes.
        /// </summary>
        public static string BuildTooltip(DevelopmentStatusTag tag)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Status: {tag.status}");

            if (!string.IsNullOrEmpty(tag.notes))
                sb.AppendLine($"Notes: {tag.notes}");

            return sb.ToString().TrimEnd();
        }
    }
}
