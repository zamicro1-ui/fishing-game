using System.Collections.Generic;
using UnityEngine;
using HolyMackerel.Core;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HolyMackerel.StartScreen
{
    /// <summary>
    /// Title-screen input handler. Listens for any tap, click, or key press and
    /// transitions to the game scene — except when the tap lands inside one of
    /// the configured exclusion zones (e.g. a settings button on the title screen).
    /// </summary>
    public class StartScreenController : MonoBehaviour
    {
        [Tooltip("Suppress taps for the first 0.5s after the scene loads to avoid carrying over a tap from the previous screen.")]
        public bool ignoreTapsForFirstHalfSecond = true;

        [Tooltip("How long (seconds) to ignore taps after the scene starts when the flag above is on.")]
        public float ignoreTapWindow = 0.5f;

        [Tooltip("Drag any Collider2D GameObjects here whose area should NOT trigger the game start (e.g. the menu icon).")]
        public List<Collider2D> exclusionZones = new List<Collider2D>();

        private float sceneLoadedTime;
        private bool isTransitioning;

        private void Awake()
        {
            sceneLoadedTime = Time.time;
        }

        private void Update()
        {
            if (isTransitioning) return;
            if (ignoreTapsForFirstHalfSecond && Time.time - sceneLoadedTime < ignoreTapWindow) return;

            if (!TryGetTap(out Vector2 screenPos, out bool hasScreenPos)) return;

            if (hasScreenPos && IsOverExclusionZone(screenPos)) return;

            BeginTransitionToGame();
        }

        /// <summary>
        /// Detects a tap, click, or key press this frame. When the source has a screen
        /// position (mouse/touch), <paramref name="hasScreenPos"/> is true and
        /// <paramref name="screenPos"/> is filled. Keyboard input has no position and
        /// always bypasses the exclusion check.
        /// </summary>
        private bool TryGetTap(out Vector2 screenPos, out bool hasScreenPos)
        {
            screenPos = default;
            hasScreenPos = false;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPos = Mouse.current.position.ReadValue();
                hasScreenPos = true;
                return true;
            }
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
                hasScreenPos = true;
                return true;
            }
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                return true;
            }
            return false;
#else
            if (Input.GetMouseButtonDown(0))
            {
                screenPos = Input.mousePosition;
                hasScreenPos = true;
                return true;
            }
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                screenPos = Input.GetTouch(0).position;
                hasScreenPos = true;
                return true;
            }
            if (Input.anyKeyDown)
            {
                return true;
            }
            return false;
#endif
        }

        /// <summary>
        /// Returns true if the given screen position projects into the world over
        /// any configured exclusion collider.
        /// </summary>
        private bool IsOverExclusionZone(Vector2 screenPos)
        {
            if (exclusionZones == null || exclusionZones.Count == 0) return false;

            Camera cam = Camera.main;
            if (cam == null) return false;

            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

            for (int i = 0; i < exclusionZones.Count; i++)
            {
                Collider2D col = exclusionZones[i];
                if (col != null && col.OverlapPoint(worldPos2D)) return true;
            }
            return false;
        }

        /// <summary>
        /// Locks out further taps and loads the game scene. A fade-out can be hooked
        /// in here before the scene swap.
        /// </summary>
        private void BeginTransitionToGame()
        {
            isTransitioning = true;
            // TODO: trigger a fade-out / transition effect here, then load on completion.
            SceneLoader.LoadScene("HubScene");
        }
    }
}
