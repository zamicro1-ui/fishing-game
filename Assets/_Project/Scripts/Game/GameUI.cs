using TMPro;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HolyMackerel.Game
{
    /// <summary>
    /// World-space UI for the fishing scene. Manages the "Tap to Cast" prompt,
    /// the live depth/catch readouts, and the end-of-round results panel.
    /// World-space TMP is used in preference to a Canvas to sidestep an earlier
    /// URP UI rendering issue.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("Refs")]
        [Tooltip("Top-level scene controller — read state and catch count from here.")]
        [SerializeField] private GameSceneController gameController;

        [Tooltip("The hook — depth is derived from its Y position.")]
        [SerializeField] private Hook hook;

        [Header("World-space TMP")]
        [Tooltip("Shown only while the game is Idle (\"Tap to Cast\" prompt).")]
        [SerializeField] private TextMeshPro tapToCastText;

        [Tooltip("Live depth readout in meters (shown while a cast is in progress).")]
        [SerializeField] private TextMeshPro depthText;

        [Tooltip("Live catch counter (shown while a cast is in progress).")]
        [SerializeField] private TextMeshPro catchText;

        [Header("Results Panel (world-space)")]
        [Tooltip("Root of the results panel — toggled active when the round ends.")]
        [SerializeField] private GameObject resultsRoot;

        [Tooltip("\"You caught X fish!\" headline text on the results panel.")]
        [SerializeField] private TextMeshPro resultsText;

        [Tooltip("Coin total text on the results panel.")]
        [SerializeField] private TextMeshPro coinTotalText;

        [Tooltip("Collider2D on the Return-to-Menu button. A click whose world position overlaps this triggers ReturnToMenu().")]
        [SerializeField] private Collider2D returnToMenuButton;

        [Header("Depth Calibration")]
        [Tooltip("World Y that represents the water surface (depth = 0 here).")]
        [SerializeField] private float surfaceY = 0f;

        private void Start()
        {
            ShowIdle();
        }

        private void Update()
        {
            if (gameController == null) return;

            switch (gameController.State)
            {
                case GameSceneController.GameState.Idle:
                    break;
                case GameSceneController.GameState.Descending:
                case GameSceneController.GameState.Ascending:
                    UpdateLiveReadouts();
                    break;
                case GameSceneController.GameState.Results:
                    DetectResultsClick();
                    break;
            }
        }

        private void UpdateLiveReadouts()
        {
            if (depthText != null && hook != null)
            {
                int meters = Mathf.Max(0, Mathf.RoundToInt(surfaceY - hook.transform.position.y));
                depthText.text = meters + "m";
            }
            if (catchText != null)
            {
                catchText.text = "Catches: " + gameController.CatchCount;
            }
        }

        /// <summary>Show only the "Tap to Cast" prompt; hide gameplay and results UI.</summary>
        public void ShowIdle()
        {
            SetActive(tapToCastText, true);
            SetActive(depthText, false);
            SetActive(catchText, false);
            if (resultsRoot != null) resultsRoot.SetActive(false);
        }

        /// <summary>Show the in-game depth/catch readouts; hide the prompt and results.</summary>
        public void ShowInGame()
        {
            SetActive(tapToCastText, false);
            SetActive(depthText, true);
            SetActive(catchText, true);
            if (resultsRoot != null) resultsRoot.SetActive(false);
        }

        /// <summary>Show the end-of-round results panel with the final catch count.</summary>
        public void ShowResults(int catchCount, int coinTotal)
        {
            SetActive(tapToCastText, false);
            SetActive(depthText, false);
            SetActive(catchText, false);
            if (resultsRoot != null) resultsRoot.SetActive(true);
            if (resultsText != null) resultsText.text = "You caught " + catchCount + " fish!";
            if (coinTotalText != null) coinTotalText.text = coinTotal.ToString();
        }

        private static void SetActive(Component c, bool active)
        {
            if (c != null) c.gameObject.SetActive(active);
        }

        private void DetectResultsClick()
        {
            if (returnToMenuButton == null) return;
            if (!TryGetTapWorld(out Vector2 worldPos)) return;
            if (returnToMenuButton.OverlapPoint(worldPos))
            {
                gameController.ReturnToMenu();
            }
        }

        private bool TryGetTapWorld(out Vector2 worldPos)
        {
            worldPos = default;
            Camera cam = Camera.main;
            if (cam == null) return false;

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
                return false;
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
                return false;
            }
#endif

            Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            worldPos = new Vector2(world.x, world.y);
            return true;
        }
    }
}
