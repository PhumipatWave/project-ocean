using UnityEngine;

namespace Game.Testing
{
    /// <summary>
    /// Flips character sprites on a visual child.
    /// </summary>
    public static class CharacterFacing2D
    {
        public static Transform RecheckVisualRoot(Transform bodyTranform, Transform visualTranform)
        {
            if (visualTranform != null)
                return visualTranform;

            Transform visual = bodyTranform.Find("Visual");
            if (visual != null)
                return visual;

            SpriteRenderer sprite = bodyTranform.GetComponentInChildren<SpriteRenderer>();
            return sprite != null ? sprite.transform : null;
        }

        public static bool IsFacingRight(Transform body, Transform visualRoot)
        {
            Transform scaleSource = visualRoot != null ? visualRoot : body;
            return scaleSource.localScale.x >= 0f;
        }

        public static void ApplyFlip(Transform body, Transform visualRoot, bool facingRight)
        {
            if (visualRoot != null && visualRoot != body)
            {
                Vector3 visualScale = visualRoot.localScale;
                float absX = Mathf.Abs(visualScale.x);
                float absY = Mathf.Abs(visualScale.y);
                visualRoot.localScale = new Vector3(facingRight ? absX : -absX, absY, visualScale.z);

                Vector3 bodyScale = body.localScale;
                body.localScale = new Vector3(Mathf.Abs(bodyScale.x), Mathf.Abs(bodyScale.y), bodyScale.z);
                return;
            }

            Vector3 scale = body.localScale;
            body.localScale = new Vector3(facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x), Mathf.Abs(scale.y), scale.z);
        }
    }
}