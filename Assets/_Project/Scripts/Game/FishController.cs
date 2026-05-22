using UnityEngine;

namespace HolyMackerel.Game
{
    /// <summary>
    /// A single fish in the play area. Swims horizontally in one direction until
    /// it hits a "Wall"-tagged collider, then reverses. When caught, parents
    /// itself to the hook and stops moving.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FishController : MonoBehaviour
    {
        [Tooltip("Horizontal swim speed (world units per second). Uniform across all fish types.")]
        [SerializeField] private float swimSpeed = 1.5f;

        [Tooltip("Score value when caught. Hardcoded to 1 in Phase 1; consumed by upgrade/economy systems later.")]
        public int pointValue = 1;

        [Tooltip("Optional sprite pool. If non-empty, one sprite is chosen at random on spawn, replacing the prefab's default. Leave empty to use the prefab's assigned sprite.")]
        [SerializeField] private Sprite[] spriteVariants;

        [Tooltip("Marks this fish as a silhouette ('shadow') — tinted dark blue, behind regular fish in sort order, uncatchable by the hook. Set at spawn time by FishSpawner; leave false on the prefab.")]
        public bool isShadow = false;

        public bool IsCaught { get; private set; }

        private int direction = 1;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (isShadow && spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.15f, 0.25f, 0.4f, 0.75f);
                spriteRenderer.sortingOrder -= 1;
            }
        }

        private void Start()
        {
            if (spriteRenderer != null && spriteVariants != null && spriteVariants.Length > 0)
            {
                Sprite pick = spriteVariants[Random.Range(0, spriteVariants.Length)];
                if (pick != null) spriteRenderer.sprite = pick;
            }
            direction = Random.value < 0.5f ? -1 : 1;
            if (spriteRenderer != null) spriteRenderer.flipX = direction < 0;
        }

        private void Update()
        {
            if (IsCaught) return;

            Vector3 pos = transform.position;
            pos.x += direction * swimSpeed * Time.deltaTime;
            transform.position = pos;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsCaught) return;
            if (other.CompareTag("Wall"))
            {
                direction = -direction;
                if (spriteRenderer != null) spriteRenderer.flipX = direction < 0;
            }
        }

        /// <summary>
        /// Latches this fish onto the hook: stops swim movement and reparents the
        /// transform so it visually follows the hook on its way back up.
        /// </summary>
        public void AttachToHook(Hook hook)
        {
            if (IsCaught || hook == null) return;
            IsCaught = true;
            transform.SetParent(hook.transform, worldPositionStays: true);
        }
    }
}
