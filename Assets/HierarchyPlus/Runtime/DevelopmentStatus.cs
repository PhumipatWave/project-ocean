namespace HierarchyPlus
{
    /// <summary>
    /// DevelopmentStatus — Tracks the workflow state of a GameObject
    /// during development. Shown as a colored badge in the Hierarchy window.
    ///
    /// Color mapping (see StatusColorMap in the Editor assembly):
    ///   New              → Blue
    ///   WorkInProgress   → Yellow
    ///   Complete         → Green
    ///   Problem          → Red
    ///   Review           → Purple
    ///   Deprecated       → Gray
    /// </summary>
    public enum DevelopmentStatus
    {
        None           = 0,
        New            = 1,
        WorkInProgress = 2,
        Complete       = 3,
        Problem        = 4,
        Review         = 5,
        Deprecated     = 6,
    }
}
