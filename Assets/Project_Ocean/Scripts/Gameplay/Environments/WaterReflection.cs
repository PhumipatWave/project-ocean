using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Game.Core;

namespace Game.Gameplay
{
    /// <summary>Spawns a reflection copy of this sprite or tilemap that only shows where ground tiles leave water uncovered.</summary>
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
            // Copy the drawn sprite, not the tile asset: a RuleTile would re-pick its sprite from the mirror's neighbours.
            var tiles = new Dictionary<Sprite, Tile>();

            sourceMap.CompressBounds();
            foreach (var cell in sourceMap.cellBounds.allPositionsWithin)
            {
                if (!sourceMap.HasTile(cell) || sourceMap.HasTile(cell + outward)) continue;

                // cell is an edge facing the water: copy the run behind it past it, keeping row order.
                int run = 0;
                while (run < depth && sourceMap.GetSprite(cell - outward * run) != null) run++;

                for (int k = 0; k < run; k++)
                {
                    var from = cell - outward * k;
                    var sprite = sourceMap.GetSprite(from);
                    if (!tiles.TryGetValue(sprite, out var tile))
                    {
                        tile = ScriptableObject.CreateInstance<Tile>();
                        tile.sprite = sprite;
                        // Default flags lock the transform, which would ignore the copied rotation below.
                        tile.flags = TileFlags.None;
                        tiles[sprite] = tile;
                    }
                    var to = from + outward * run;
                    target.SetTile(to, tile);
                    target.SetTransformMatrix(to, sourceMap.GetTransformMatrix(from));
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
