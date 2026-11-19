namespace HierarchyPlus
{
    /// <summary>
    /// HierarchyCategory — Defines all supported scene-object categories.
    ///
    /// Used by HierarchyCategoryTag to let designers tag GameObjects directly
    /// in the Inspector. Each value maps to a color and an icon in the Editor
    /// overlay (see CategoryColorMap in the Editor assembly).
    /// </summary>
    public enum HierarchyCategory
    {
        None        = 0,
        Entity      = 1,
        Player      = 2,
        Enemy       = 3,
        UI          = 4,
        Manager     = 5,
        Environment = 6,
        Audio       = 7,
    }
}
