using UnityEngine;

namespace HolyMackerel.Game
{
    /// <summary>
    /// Spawns decorative crabs at randomized positions inside two cliff
    /// bounding rects (left and right) at scene start. Always spawns
    /// minCount crabs; chanceOfMaxCount probability bumps that to maxCount.
    /// Each crab independently rolls a 50/50 side, then a uniform random
    /// (X, Y) within that side's rect. Right-side crabs get a local scale X of 1,
    /// while left-side crabs get a local scale X of -1 to flip them horizontally.
    ///
    /// Visualize the two rects in the Scene view by selecting the
    /// CrabSpawner GameObject — see <see cref="OnDrawGizmosSelected"/>.
    /// </summary>
    public class CrabSpawner : MonoBehaviour
    {
        [Tooltip("Crab prefab. A simple GameObject with a SpriteRenderer is enough — no behavior script required.")]
        [SerializeField] private GameObject crabPrefab;

        [Header("Right Cliff Spawn Area")]
        [Tooltip("Bottom-left corner of the right cliff's spawn rect (world coords).")]
        [SerializeField] private Vector2 rightAreaMin = new Vector2(3.5f, -22f);

        [Tooltip("Top-right corner of the right cliff's spawn rect (world coords). Must be greater than rightAreaMin on both axes.")]
        [SerializeField] private Vector2 rightAreaMax = new Vector2(4.5f, -2f);

        [Header("Left Cliff Spawn Area")]
        [Tooltip("Bottom-left corner of the left cliff's spawn rect (world coords).")]
        [SerializeField] private Vector2 leftAreaMin = new Vector2(-4.5f, -22f);

        [Tooltip("Top-right corner of the left cliff's spawn rect (world coords). Must be greater than leftAreaMin on both axes.")]
        [SerializeField] private Vector2 leftAreaMax = new Vector2(-3.5f, -2f);

        [Header("Counts")]
        [Tooltip("Always spawn at least this many crabs.")]
        [SerializeField] private int minCount = 1;

        [Tooltip("Maximum crabs to spawn when chanceOfMaxCount triggers.")]
        [SerializeField] private int maxCount = 2;

        [Range(0f, 1f)]
        [Tooltip("Probability of spawning maxCount crabs instead of minCount. Default 0.10 = 10% chance for 2 crabs, otherwise minCount.")]
        [SerializeField] private float chanceOfMaxCount = 0.1f;

        private void Start()
        {
            if (crabPrefab == null)
            {
                Debug.LogWarning("[CrabSpawner] No crab prefab assigned — nothing will spawn.", this);
                return;
            }

            int countToSpawn = Random.value < chanceOfMaxCount ? maxCount : minCount;
            if (countToSpawn <= 0) return;

            for (int i = 0; i < countToSpawn; i++)
            {
                bool isRightSide = Random.value < 0.5f;
                Vector2 areaMin = isRightSide ? rightAreaMin : leftAreaMin;
                Vector2 areaMax = isRightSide ? rightAreaMax : leftAreaMax;

                float x = Random.Range(areaMin.x, areaMax.x);
                float y = Random.Range(areaMin.y, areaMax.y);

                GameObject crab = Instantiate(crabPrefab, new Vector3(x, y, 0f), Quaternion.identity, transform);
                
                // Get the current local scale to preserve whatever Y and Z values the prefab has
                Vector3 currentScale = crab.transform.localScale;
                
                // Set scale X to 1 if right side, -1 if left side
                currentScale.x = isRightSide ? 1f : -1f;
                
                crab.transform.localScale = currentScale;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.4f, 1f);
            DrawAreaGizmo(rightAreaMin, rightAreaMax);
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 1f);
            DrawAreaGizmo(leftAreaMin, leftAreaMax);
        }

        private static void DrawAreaGizmo(Vector2 min, Vector2 max)
        {
            Vector3 center = new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, 0f);
            Vector3 size = new Vector3(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y), 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}