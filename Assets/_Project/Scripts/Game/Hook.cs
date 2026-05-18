using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HolyMackerel.Game
{
    /// <summary>
    /// The player-controlled hook. Moves vertically at constant speed (descent or
    /// ascent) while accepting horizontal steering input. Reports trigger collisions
    /// (fish, bottom, surface) back to the GameSceneController.
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

        [Tooltip("Horizontal steering speed (world units per second).")]
        [SerializeField] private float horizontalSpeed = 4f;

        [Header("Play Area")]
        [Tooltip("Leftmost X the hook can reach.")]
        [SerializeField] private float leftBound = -4f;

        [Tooltip("Rightmost X the hook can reach.")]
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

        private void Update()
        {
            if (State == HookState.Idle) return;

            Vector3 pos = transform.position;

            float horizontal = ReadHorizontalInput();
            pos.x += horizontal * horizontalSpeed * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, leftBound, rightBound);

            if (State == HookState.Descending)
            {
                pos.y -= descentSpeed * Time.deltaTime;
            }
            else if (State == HookState.Ascending)
            {
                pos.y += ascentSpeed * Time.deltaTime;
            }

            transform.position = pos;
        }

        private float ReadHorizontalInput()
        {
#if ENABLE_INPUT_SYSTEM
            float h = 0f;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
            }
            return h;
#else
            return Input.GetAxisRaw("Horizontal");
#endif
        }

        /// <summary>Begin descending. Resets the per-round catch count.</summary>
        public void StartDescent()
        {
            State = HookState.Descending;
            CurrentCatchCount = 0;
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
            if (surfacePoint != null) transform.position = surfacePoint.position;
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
            if (other.CompareTag("Fish"))
            {
                FishController fish = other.GetComponent<FishController>();
                if (fish != null && !fish.IsCaught)
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
