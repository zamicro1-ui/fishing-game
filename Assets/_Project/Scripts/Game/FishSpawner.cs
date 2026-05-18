using System.Collections.Generic;
using UnityEngine;

namespace HolyMackerel.Game
{
    /// <summary>
    /// Spawns fish at scene start in two phases: first every FishSpawnEntry's
    /// minimumCount is honored, then any remaining slots up to fishCount are
    /// filled by weighted random selection over the entries. minVerticalSpacing
    /// is enforced globally across both phases via reroll. Spawned fish are
    /// parented to this transform.
    /// </summary>
    public class FishSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class FishSpawnEntry
        {
            [Tooltip("Fish prefab (must have FishController + Collider2D + tag \"Fish\").")]
            public GameObject prefab;

            [Tooltip("Shallowest Y this fish type can spawn at (typically near the surface, negative).")]
            public float minDepth;

            [Tooltip("Deepest Y this fish type can spawn at (more negative than minDepth).")]
            public float maxDepth;

            [Tooltip("Guaranteed minimum number of this type to spawn.")]
            public int minimumCount = 1;

            [Tooltip("Relative weight for filling remaining slots after minimums are spawned. Higher = more common.")]
            public int weight = 1;
        }

        [Tooltip("List of fish types to spawn. Each entry defines its own depth band, minimum count, and weight.")]
        [SerializeField] private List<FishSpawnEntry> fishEntries = new List<FishSpawnEntry>();

        [Tooltip("Target total fish count. Minimums are spawned first, then remaining slots are filled via weighted random.")]
        [SerializeField] private int fishCount = 8;

        [Tooltip("Half-width of the horizontal spawn range, centered on X=0.")]
        [SerializeField] private float horizontalSpread = 3.5f;

        [Tooltip("Minimum vertical distance (world units) between any two spawned fish. The spawner rerolls Y up to 10 times per fish to satisfy this; if it can't, it accepts the last attempt and logs a warning.")]
        [SerializeField] private float minVerticalSpacing = 1.5f;

        private void Start()
        {
            if (fishEntries == null || fishEntries.Count == 0)
            {
                Debug.LogWarning("[FishSpawner] No fish entries configured — nothing will spawn.", this);
                return;
            }

            List<float> spawnedYs = new List<float>();

            int totalMinimums = 0;
            for (int i = 0; i < fishEntries.Count; i++)
            {
                FishSpawnEntry e = fishEntries[i];
                if (e == null || e.prefab == null || e.minimumCount <= 0) continue;
                totalMinimums += e.minimumCount;
            }

            if (totalMinimums > fishCount)
            {
                Debug.LogWarning($"[FishSpawner] Sum of minimumCount ({totalMinimums}) exceeds fishCount ({fishCount}) — spawning all minimums anyway; weighted fill phase will be skipped.", this);
            }

            for (int entryIndex = 0; entryIndex < fishEntries.Count; entryIndex++)
            {
                FishSpawnEntry entry = fishEntries[entryIndex];
                if (entry == null || entry.prefab == null || entry.minimumCount <= 0) continue;
                for (int n = 0; n < entry.minimumCount; n++)
                {
                    SpawnOne(entry, entryIndex, spawnedYs);
                }
            }

            int remaining = fishCount - totalMinimums;
            if (remaining <= 0) return;

            int totalWeight = 0;
            for (int i = 0; i < fishEntries.Count; i++)
            {
                FishSpawnEntry e = fishEntries[i];
                if (e != null && e.prefab != null && e.weight > 0) totalWeight += e.weight;
            }

            if (totalWeight <= 0)
            {
                Debug.LogWarning($"[FishSpawner] {remaining} slot(s) remaining but no entries have a valid prefab and positive weight — fill phase skipped.", this);
                return;
            }

            for (int i = 0; i < remaining; i++)
            {
                FishSpawnEntry entry = PickWeighted(totalWeight);
                if (entry == null) continue;
                SpawnOne(entry, -1, spawnedYs);
            }
        }

        private void SpawnOne(FishSpawnEntry entry, int entryIndex, List<float> spawnedYs)
        {
            float top = Mathf.Max(entry.minDepth, entry.maxDepth);
            float bottom = Mathf.Min(entry.minDepth, entry.maxDepth);
            float x = Random.Range(-horizontalSpread, horizontalSpread);

            float y = Random.Range(bottom, top);
            const int maxAttempts = 10;
            int attempt = 1;
            while (attempt < maxAttempts && IsTooClose(y, spawnedYs))
            {
                y = Random.Range(bottom, top);
                attempt++;
            }
            if (IsTooClose(y, spawnedYs))
            {
                Debug.LogWarning($"[FishSpawner] Could not find a Y for fish {spawnedYs.Count} (entry {entryIndex}) satisfying minVerticalSpacing={minVerticalSpacing} after {maxAttempts} attempts — accepting last attempt. Spawner is overpacked.", this);
            }

            spawnedYs.Add(y);
            Instantiate(entry.prefab, new Vector3(x, y, 0f), Quaternion.identity, transform);
        }

        private FishSpawnEntry PickWeighted(int totalWeight)
        {
            int roll = Random.Range(0, totalWeight);
            int cumulative = 0;
            for (int i = 0; i < fishEntries.Count; i++)
            {
                FishSpawnEntry e = fishEntries[i];
                if (e == null || e.prefab == null || e.weight <= 0) continue;
                cumulative += e.weight;
                if (roll < cumulative) return e;
            }
            return null;
        }

        private bool IsTooClose(float y, List<float> existingYs)
        {
            for (int i = 0; i < existingYs.Count; i++)
            {
                if (Mathf.Abs(existingYs[i] - y) < minVerticalSpacing) return true;
            }
            return false;
        }
    }
}
