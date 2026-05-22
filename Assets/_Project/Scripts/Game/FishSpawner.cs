using System.Collections.Generic;
using UnityEngine;

namespace HolyMackerel.Game
{
    /// <summary>
    /// Spawns fish at scene start. Each FishSpawnEntry defines its own
    /// rectangular spawn area (areaMin / areaMax) — both the X and Y range
    /// are per-entry, so small / medium / large fish can have completely
    /// independent depth bands and horizontal widths. Phase 1 honors every
    /// entry's minimumCount; phase 2 fills remaining slots up to fishCount
    /// via weighted random selection. Shadow fish phase runs independently
    /// using the same per-entry rects. Vertical spacing, wall avoidance,
    /// and bottom buffer are enforced globally via reroll. Spawned fish
    /// are parented to this transform.
    ///
    /// Visualize each entry's rect in the Scene view by selecting the
    /// FishSpawner GameObject — see <see cref="OnDrawGizmosSelected"/>.
    /// Custom Scene-view drag handles for each rect live in
    /// FishSpawnerEditor.cs.
    /// </summary>
    public class FishSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class FishSpawnEntry
        {
            [Tooltip("Fish prefab (must have FishController + Collider2D + tag \"Fish\").")]
            public GameObject prefab;

            [Tooltip("Bottom-left corner of this entry's spawn rect (world coords).")]
            public Vector2 areaMin = new Vector2(-3.5f, -25f);

            [Tooltip("Top-right corner of this entry's spawn rect (world coords). Must be greater than areaMin on both axes.")]
            public Vector2 areaMax = new Vector2(3.5f, -3f);

            [Tooltip("Guaranteed minimum number of this type to spawn.")]
            public int minimumCount = 1;

            [Tooltip("Relative weight for filling remaining slots after minimums are spawned. Higher = more common.")]
            public int weight = 1;
        }

        [Tooltip("List of fish types to spawn. Each entry defines its own spawn rect, minimum count, and weight.")]
        [SerializeField] private List<FishSpawnEntry> fishEntries = new List<FishSpawnEntry>();

        [Tooltip("Target total fish count. Minimums are spawned first, then remaining slots are filled via weighted random.")]
        [SerializeField] private int fishCount = 8;

        [Tooltip("Minimum vertical gap (world units) between any spawn and the \"Bottom\"-tagged collider. Spawns whose point is within this distance above the bottom collider are rejected and rerolled. Set to 0 to disable the bottom check.")]
        [SerializeField] private float bottomBuffer = 0.5f;

        [Tooltip("Minimum vertical distance (world units) between any two spawned fish. The spawner rerolls up to 10 times per fish to satisfy this; if it can't, it accepts the last attempt and logs a warning.")]
        [SerializeField] private float minVerticalSpacing = 1.5f;

        [Tooltip("Maximum number of shadow (silhouette, uncatchable) fish that may spawn this round. Independent of fishCount — shadows do NOT count against the regular fish cap.")]
        [SerializeField] private int maxShadowFish = 6;

        [Range(0f, 1f)]
        [Tooltip("Probability per shadow slot that the slot produces a shadow. With defaults maxShadowFish=6 and shadowSpawnChance=0.25, the round averages 1.5 shadows.")]
        [SerializeField] private float shadowSpawnChance = 0.25f;

        private static readonly Color[] EntryPalette = new Color[]
        {
            new Color(0.3f, 0.85f, 0.9f, 1f), // teal
            new Color(1f, 0.6f, 0.3f, 1f),    // orange
            new Color(1f, 0.9f, 0.3f, 1f),    // yellow
            new Color(1f, 0.5f, 0.8f, 1f),    // pink
            new Color(0.6f, 1f, 0.5f, 1f),    // green
            new Color(0.7f, 0.5f, 1f, 1f),    // purple
        };

        /// <summary>Stable color per entry index for both runtime gizmos and editor handles.</summary>
        public static Color GetEntryColor(int entryIndex)
        {
            return EntryPalette[((entryIndex % EntryPalette.Length) + EntryPalette.Length) % EntryPalette.Length];
        }

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

            int totalWeight = 0;
            for (int i = 0; i < fishEntries.Count; i++)
            {
                FishSpawnEntry e = fishEntries[i];
                if (e != null && e.prefab != null && e.weight > 0) totalWeight += e.weight;
            }

            int remaining = fishCount - totalMinimums;
            if (remaining > 0)
            {
                if (totalWeight <= 0)
                {
                    Debug.LogWarning($"[FishSpawner] {remaining} slot(s) remaining but no entries have a valid prefab and positive weight — fill phase skipped.", this);
                }
                else
                {
                    for (int i = 0; i < remaining; i++)
                    {
                        FishSpawnEntry entry = PickWeighted(totalWeight);
                        if (entry == null) continue;
                        SpawnOne(entry, -1, spawnedYs);
                    }
                }
            }

            if (maxShadowFish > 0 && shadowSpawnChance > 0f && totalWeight > 0)
            {
                for (int i = 0; i < maxShadowFish; i++)
                {
                    if (Random.value >= shadowSpawnChance) continue;
                    FishSpawnEntry entry = PickWeighted(totalWeight);
                    if (entry == null) continue;
                    SpawnOneShadow(entry, -1, spawnedYs);
                }
            }
        }

        private void SpawnOne(FishSpawnEntry entry, int entryIndex, List<float> spawnedYs)
        {
            Vector2 pos = PickSpawnPoint(entry, entryIndex, spawnedYs, isShadow: false);
            spawnedYs.Add(pos.y);
            Instantiate(entry.prefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity, transform);
        }

        private void SpawnOneShadow(FishSpawnEntry entry, int entryIndex, List<float> spawnedYs)
        {
            Vector2 pos = PickSpawnPoint(entry, entryIndex, spawnedYs, isShadow: true);
            spawnedYs.Add(pos.y);

            bool wasActive = entry.prefab.activeSelf;
            if (wasActive) entry.prefab.SetActive(false);
            GameObject go = Instantiate(entry.prefab, new Vector3(pos.x, pos.y, 0f), Quaternion.identity, transform);
            if (wasActive) entry.prefab.SetActive(true);

            FishController fc = go.GetComponent<FishController>();
            if (fc != null) fc.isShadow = true;

            go.SetActive(true);
        }

        private Vector2 PickSpawnPoint(FishSpawnEntry entry, int entryIndex, List<float> spawnedYs, bool isShadow)
        {
            float minX = Mathf.Min(entry.areaMin.x, entry.areaMax.x);
            float maxX = Mathf.Max(entry.areaMin.x, entry.areaMax.x);
            float minY = Mathf.Min(entry.areaMin.y, entry.areaMax.y);
            float maxY = Mathf.Max(entry.areaMin.y, entry.areaMax.y);

            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            const int maxAttempts = 10;
            int attempt = 1;
            while (attempt < maxAttempts && (IsTooClose(y, spawnedYs) || IsInsideWall(new Vector2(x, y)) || IsTooCloseToBottom(new Vector2(x, y))))
            {
                x = Random.Range(minX, maxX);
                y = Random.Range(minY, maxY);
                attempt++;
            }
            if (IsTooClose(y, spawnedYs) || IsInsideWall(new Vector2(x, y)) || IsTooCloseToBottom(new Vector2(x, y)))
            {
                string label = isShadow ? "shadow" : "fish";
                Debug.LogWarning($"[FishSpawner] Could not find a valid {label} spawn (entry {entryIndex}) after {maxAttempts} attempts — accepting last attempt (may overlap a Wall collider, sit within bottomBuffer={bottomBuffer} of the Bottom collider, or violate minVerticalSpacing={minVerticalSpacing}).", this);
            }

            return new Vector2(x, y);
        }

        private bool IsInsideWall(Vector2 point)
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(point);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] != null && hits[i].CompareTag("Wall")) return true;
            }
            return false;
        }

        private bool IsTooCloseToBottom(Vector2 point)
        {
            if (bottomBuffer <= 0f) return false;
            Vector2 probe = new Vector2(point.x, point.y - bottomBuffer);
            Collider2D[] hits = Physics2D.OverlapPointAll(probe);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] != null && hits[i].CompareTag("Bottom")) return true;
            }
            return false;
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

        private void OnDrawGizmosSelected()
        {
            if (fishEntries == null) return;
            for (int i = 0; i < fishEntries.Count; i++)
            {
                FishSpawnEntry e = fishEntries[i];
                if (e == null) continue;
                Gizmos.color = GetEntryColor(i);
                Vector3 center = new Vector3((e.areaMin.x + e.areaMax.x) * 0.5f, (e.areaMin.y + e.areaMax.y) * 0.5f, 0f);
                Vector3 size = new Vector3(Mathf.Abs(e.areaMax.x - e.areaMin.x), Mathf.Abs(e.areaMax.y - e.areaMin.y), 0.1f);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}
