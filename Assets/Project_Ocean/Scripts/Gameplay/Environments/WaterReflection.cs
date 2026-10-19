using UnityEngine;
using UnityEngine.Tilemaps;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Spawns a flipped copy of this sprite or tilemap that only shows where ground tiles leave water uncovered.</summary>
    public class WaterReflection : MonoBehaviour
    {
        public enum ReflectionSource { Sprite, Tilemap }
        public enum MirrorDirection { Down, Left }

        [SerializeField] private ReflectionSource source;
        [SerializeField] private Material reflectionMaterial;
        [SerializeField] private Color tint = new Color(0.6f, 0.8f, 1f, 0.4f);
        // Must sort above the Water tilemap and below the ground tilemaps ("Way"), so ground hides it on land.
        [SerializeField] private string sortingLayer = "StoneMain";
        [SerializeField] private int sortingOrder = 2;
        [SerializeField] private Vector2 offset;

        [Header("Sprite")]
        // For persistent objects like the player, which also live in Ocean_Map where there is no water surface.
        [SerializeField] private bool landOnly;

        [Header("Tilemap")]
        [SerializeField] private MirrorDirection mirrorDirection;
        [SerializeField, Min(1)] private int depth = 1;

        private static readonly Matrix4x4 FlipX = Matrix4x4.Scale(new Vector3(-1f, 1f, 1f));
        private static readonly Matrix4x4 FlipY = Matrix4x4.Scale(new Vector3(1f, -1f, 1f));

        private SpriteRenderer sourceSprite;
        private SpriteRenderer reflectionSprite;
        private Transform reflectionTilemap;

        private void Reset()
        {
            bool isTilemap = GetComponent<Tilemap>() != null;
            source = isTilemap ? ReflectionSource.Tilemap : ReflectionSource.Sprite;
            sortingOrder = isTilemap ? 1 : 2;
        }

        private void Awake()
        {
            if (source == ReflectionSource.Tilemap) BuildTilemap();
            else BuildSprite();
        }

        private void BuildSprite()
        {
            sourceSprite = GetComponent<SpriteRenderer>();
            if (sourceSprite == null)
            {
                Debug.LogWarning($"{name}: WaterReflection set to Sprite but has no SpriteRenderer.", this);
                enabled = false;
                return;
            }

            reflectionSprite = CreateChild().AddComponent<SpriteRenderer>();
            ApplyLook(reflectionSprite);
            reflectionSprite.color = tint;
            reflectionSprite.flipY = true;
        }

        private void BuildTilemap()
        {
            var sourceMap = GetComponent<Tilemap>();
            if (sourceMap == null)
            {
                Debug.LogWarning($"{name}: WaterReflection set to Tilemap but has no Tilemap.", this);
                enabled = false;
                return;
            }

            var go = CreateChild();
            reflectionTilemap = go.transform;
            var target = go.AddComponent<Tilemap>();
            target.color = tint;
            ApplyLook(go.AddComponent<TilemapRenderer>());

            bool left = mirrorDirection == MirrorDirection.Left;
            var outward = left ? Vector3Int.left : Vector3Int.down;
            var flip = left ? FlipX : FlipY;

            sourceMap.CompressBounds();
            foreach (var cell in sourceMap.cellBounds.allPositionsWithin)
            {
                if (!sourceMap.HasTile(cell) || sourceMap.HasTile(cell + outward)) continue;

                // cell is an edge facing the water: mirror the cells behind it to the cells past it.
                for (int k = 0; k < depth; k++)
                {
                    var from = cell - outward * k;
                    if (!sourceMap.HasTile(from)) break;
                    var to = cell + outward * (k + 1);
                    target.SetTile(to, sourceMap.GetTile(from));
                    // Tile assets lock their transform by default; SetTransformMatrix is ignored otherwise.
                    target.SetTileFlags(to, TileFlags.None);
                    target.SetTransformMatrix(to, sourceMap.GetTransformMatrix(from) * flip);
                }
            }
        }

        private GameObject CreateChild()
        {
            var go = new GameObject("Reflection");
            go.transform.SetParent(transform, false);
            return go;
        }

        private void ApplyLook(Renderer target)
        {
            if (reflectionMaterial != null) target.sharedMaterial = reflectionMaterial;
            target.sortingLayerName = sortingLayer;
            target.sortingOrder = sortingOrder;
        }

        private void LateUpdate()
        {
            if (reflectionTilemap != null)
            {
                reflectionTilemap.localPosition = offset;
                return;
            }

            reflectionSprite.enabled = sourceSprite.enabled && sourceSprite.sprite != null
                && (!landOnly || GameSession.State == GameState.Land);
            if (!reflectionSprite.enabled) return;

            reflectionSprite.sprite = sourceSprite.sprite;
            reflectionSprite.flipX = sourceSprite.flipX;
            // flipY mirrors around the pivot; shift by 2x bottom edge so it mirrors around the feet instead.
            reflectionSprite.transform.localPosition =
                new Vector3(offset.x, 2f * sourceSprite.sprite.bounds.min.y + offset.y, 0f);
        }
    }
}
