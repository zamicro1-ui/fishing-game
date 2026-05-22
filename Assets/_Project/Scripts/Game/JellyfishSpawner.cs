using UnityEngine;

namespace HolyMackerel.Game
{
    /// <summary>
    /// Spawns decorative jellyfish at randomized positions inside a single
    /// bounding rect at scene start. Always spawns minCount jellyfish;
    /// chanceOfMaxCount probability bumps that to maxCount. Each jellyfish
    /// gets a uniform random (X, Y) within the spawn rect. Unlike crabs, no
    /// sprite flipping happens — jellyfish are symmetric.
    ///
    /// The spawn rect is meant to cover the open water in the middle of the
    /// play area (away from the cliffs), so position the rect roughly within
    /// the fish-spawn horizontal band and between surface and bottom.
    /// Visualize the rect in the Scene view by selecting the JellyfishSpawner
    /// GameObject — see <see cref="OnDrawGizmosSelected"/>.
    /// </summary>
    public class JellyfishSpawner : MonoBehaviour
    {
        [Tooltip("Jellyfish prefab. A simple GameObject with a SpriteRenderer is enough — no behavior script required.")]
        [SerializeField] private GameObject jellyfishPrefab;

        [Header("Spawn Area")]
        [Tooltip("Bottom-left corner of the spawn rect (world coords).")]
        [SerializeField] private Vector2 spawnAreaMin = new Vector2(-3f, -18f);

        [Tooltip("Top-right corner of the spawn rect (world coords). Must be greater than spawnAreaMin on both axes.")]
        [SerializeField] private Vector2 spawnAreaMax = new Vector2(3f, -5f);

        [Header("Counts")]
        [Tooltip("Always spawn at least this many jellyfish.")]
        [SerializeField] private int minCount = 1;

        [Tooltip("Maximum jellyfish to spawn when chanceOfMaxCount triggers.")]
        [SerializeField] private int maxCount = 2;

        [Range(0f, 1f)]
        [Tooltip("Probability of spawning maxCount jellyfish instead of minCount. Default 0.10 = 10% chance for 2 jellyfish, otherwise minCount.")]
        [SerializeField] private float chanceOfMaxCount = 0.1f;

        private void Start()
        {
            if (jellyfishPrefab == null)
            {
                Debug.LogWarning("[JellyfishSpawner] No jellyfish prefab assigned — nothing will spawn.", this);
                return;
            }

            int countToSpawn = Random.value < chanceOfMaxCount ? maxCount : minCount;
            if (countToSpawn <= 0) return;

            for (int i = 0; i < countToSpawn; i++)
            {
                float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
                float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);

                Instantiate(jellyfishPrefab, new Vector3(x, y, 0f), Quaternion.identity, transform);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.8f, 0.4f, 1f, 1f);
            Vector3 center = new Vector3((spawnAreaMin.x + spawnAreaMax.x) * 0.5f, (spawnAreaMin.y + spawnAreaMax.y) * 0.5f, 0f);
            Vector3 size = new Vector3(Mathf.Abs(spawnAreaMax.x - spawnAreaMin.x), Mathf.Abs(spawnAreaMax.y - spawnAreaMin.y), 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
