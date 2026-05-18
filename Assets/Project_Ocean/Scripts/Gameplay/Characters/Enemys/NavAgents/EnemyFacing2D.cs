using UnityEngine;

/// <summary>
/// Flips enemy sprites on a visual child so world-space UI (Canvas) on the root does not mirror.
/// </summary>
public static class EnemyFacing2D
{
    public static Transform ResolveVisualRoot(Transform body, Transform assigned)
    {
        if (assigned != null)
            return assigned;

        Transform visual = body.Find("Visual");
        if (visual != null)
            return visual;

        SpriteRenderer sprite = body.GetComponentInChildren<SpriteRenderer>();
        return sprite != null ? sprite.transform : null;
    }

    public static bool ReadFacingRight(Transform body, Transform visualRoot)
    {
        Transform scaleSource = visualRoot != null ? visualRoot : body;
        return scaleSource.localScale.x >= 0f;
    }

    public static void ApplyHorizontalFlip(Transform body, Transform visualRoot, bool facingRight)
    {
        if (visualRoot != null && visualRoot != body)
        {
            Vector3 visualScale = visualRoot.localScale;
            float absX = Mathf.Abs(visualScale.x);
            float absY = Mathf.Abs(visualScale.y);
            visualRoot.localScale = new Vector3(
                facingRight ? absX : -absX,
                absY,
                visualScale.z
            );

            Vector3 bodyScale = body.localScale;
            body.localScale = new Vector3(
                Mathf.Abs(bodyScale.x),
                Mathf.Abs(bodyScale.y),
                bodyScale.z
            );
            return;
        }

        Vector3 scale = body.localScale;
        body.localScale = new Vector3(
            facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x),
            Mathf.Abs(scale.y),
            scale.z
        );
    }
}
