using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Gameplay
{
    /// <summary>AnimatedTile whose start frame varies per cell (Perlin noise), so a big water area doesn't animate in lockstep.</summary>
    [CreateAssetMenu(menuName = "2D/Tiles/Random Start Animated Tile")]
    public class RandomStartAnimatedTile : AnimatedTile
    {
        // Low = large patches sharing a phase (wave fronts), high = near-random per tile.
        [SerializeField] private float noiseScale = 0.15f;

        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            if (!base.GetTileAnimationData(position, tilemap, ref tileAnimationData)) return false;

            var tilemapComponent = tilemap.GetComponent<Tilemap>();
            float frameRate = tilemapComponent != null && tilemapComponent.animationFrameRate > 0 ? tilemapComponent.animationFrameRate : 1f;
            // Perlin is near-identical on integer lattice points; the 0.37 offset keeps samples off them.
            // Perlin mostly lands in 0.2-0.8; x2 then wrap spreads it over the full cycle.
            float noise = Mathf.Repeat(2f * Mathf.PerlinNoise(position.x * noiseScale + 0.37f, position.y * noiseScale + 0.37f), 1f);
            tileAnimationData.animationStartTime += noise * m_AnimatedSprites.Length / frameRate;
            return true;
        }
    }
}
