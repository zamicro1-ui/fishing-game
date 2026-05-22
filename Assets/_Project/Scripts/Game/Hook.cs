using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HolyMackerel.Game
{
    /// <summary>
    /// The hook auto-swings horizontally at a constant speed while descending
    /// or ascending. Swing direction reverses on three events: hitting the
    /// left bound, hitting the right bound, or the player tapping on the
    /// opposite side of the hook (same-side taps are ignored). Vertical motion
    /// is unchanged. Movement runs through <see cref="Rigidbody2D.MovePosition"/>
    /// in FixedUpdate so trigger contacts register cleanly even at high
    /// vertical speed; tap input is read in Update and applied via
    /// <see cref="swingDirection"/>. Reports trigger collisions (fish, bottom,
    /// surface) back to the GameSceneController.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Hook : MonoBehaviour
    {
        public enum HookState { Idle, Descending, Ascending }

        [Header("Speeds")]
        [Tooltip("Downward speed (world units per second) during a cast.")]
        [SerializeField] private float descentSpeed = 3f;

        [Tooltip("Upward speed (world units per second) when reeling in. Slightly faster than descent feels best.")]
        [SerializeField] private float ascentSpeed = 5f;

        [Tooltip("Horizontal swing speed (world units per second). The hook moves constantly left or right at this speed while a cast is in progress.")]
        [SerializeField] private float swingSpeed = 5f;

        [Header("Play Area")]
        [Tooltip("Leftmost X the hook can reach. Hitting this bound reverses swing direction to the right.")]
        [SerializeField] private float leftBound = -4f;

        [Tooltip("Rightmost X the hook can reach. Hitting this bound reverses swing direction to the left.")]
        [SerializeField] private float rightBound = 4f;

        [Header("Anchors")]
        [Tooltip("World-space point where the hook sits before/after a cast.")]
        [SerializeField] private Transform surfacePoint;

        [Tooltip("Reference to the bottom of the play area. Not used to move the hook (collision drives that), purely informational.")]
        [SerializeField] private Transform bottomPoint;

        [Header("Refs")]
        [Tooltip("Scene controller this hook reports events to.")]
        [SerializeField] private GameSceneController gameController;

        public HookState State { get; private set; } = HookState.Idle;
        public int CurrentCatchCount { get; private set; }

        private int swingDirection = 1;
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (State == HookState.Idle) return;
            HandleRedirectTap();
        }

        private void FixedUpdate()
        {
            if (State == HookState.Idle) return;

            Vector2 pos = rb.position;

            pos.x += swingDirection * swingSpeed * Time.fixedDeltaTime;
            if (pos.x >= rightBound)
            {
                pos.x = rightBound;
                swingDirection = -1;
            }
            else if (pos.x <= leftBound)
            {
                pos.x = leftBound;
                swingDirection = 1;
            }

            if (State == HookState.Descending)
            {
                pos.y -= descentSpeed * Time.fixedDeltaTime;
            }
            else if (State == HookState.Ascending)
            {
                pos.y += ascentSpeed * Time.fixedDeltaTime;
            }

            rb.MovePosition(pos);
        }

        private void HandleRedirectTap()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector2 screenPos;
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPos = Mouse.current.position.ReadValue();
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else
            {
                return;
            }
#else
            if (Input.GetMouseButtonDown(0))
            {
                screenPos = Input.mousePosition;
            }
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                screenPos = Input.GetTouch(0).position;
            }
            else
            {
                return;
            }
#endif

            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            if (world.x < transform.position.x) swingDirection = -1;
            else if (world.x > transform.position.x) swingDirection = 1;
        }

        /// <summary>Begin descending. Resets the per-round catch count and starts the swing moving right.</summary>
        public void StartDescent()
        {
            State = HookState.Descending;
            CurrentCatchCount = 0;
            swingDirection = 1;
        }

        /// <summary>Begin ascending. Called when the bottom is hit or a fish is caught mid-descent.</summary>
        public void StartAscent()
        {
            State = HookState.Ascending;
        }

        /// <summary>
        /// Reset to the idle state at the surface. Destroys any fish still parented
        /// to the hook from the previous round.
        /// </summary>
        public void GoIdle()
        {
            State = HookState.Idle;
            if (surfacePoint != null && rb != null) rb.position = surfacePoint.position;
            else if (surfacePoint != null) transform.position = surfacePoint.position;
            DetachAllFish();
            CurrentCatchCount = 0;
        }

        private void DetachAllFish()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponent<FishController>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Hazard"))
            {
                if (gameController != null) gameController.OnHookHitHazard();
                return;
            }
            if (other.CompareTag("Fish"))
            {
                FishController fish = other.GetComponent<FishController>();
                if (fish != null && !fish.isShadow && !fish.IsCaught)
                {
                    fish.AttachToHook(this);
                    CurrentCatchCount++;
                    if (gameController != null) gameController.OnFishCaught(fish.pointValue);
                }
                return;
            }
            if (other.CompareTag("Bottom"))
            {
                if (State == HookState.Descending && gameController != null)
                {
                    gameController.OnHookReachedBottom();
                }
                return;
            }
            if (other.CompareTag("Surface"))
            {
                if (State == HookState.Ascending && gameController != null)
                {
                    gameController.OnHookReachedSurface();
                }
            }
        }
    }
}
