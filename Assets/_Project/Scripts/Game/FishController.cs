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

        public bool IsCaught { get; private set; }

        private int direction = 1;
        private SpriteRenderer spriteRenderer;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
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
