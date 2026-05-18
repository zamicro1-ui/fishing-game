using UnityEngine;
using HolyMackerel.Core;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace HolyMackerel.Game
{
    /// <summary>
    /// Top-level orchestrator for the fishing scene. Owns the high-level state
    /// machine (Idle → Descending → Ascending → Results) and is the single
    /// authority that the Hook and GameUI both talk to.
    /// </summary>
    public class GameSceneController : MonoBehaviour
    {
        public enum GameState { Idle, Descending, Ascending, Results }

        [Header("Scene References")]
        [Tooltip("The player-controlled hook.")]
        [SerializeField] private Hook hook;

        [Tooltip("World-space UI manager.")]
        [SerializeField] private GameUI gameUI;

        [Tooltip("Spawns the fish populating the play area at scene start.")]
        [SerializeField] private FishSpawner fishSpawner;

        [Tooltip("Anchor point at the water surface — where the hook waits before a cast.")]
        [SerializeField] private Transform surfacePoint;

        public GameState State { get; private set; } = GameState.Idle;
        public int CatchCount { get; private set; }
        public int CoinsEarned { get; private set; }

        private void Start()
        {
            State = GameState.Idle;
            CatchCount = 0;
            CoinsEarned = 0;

            if (hook != null)
            {
                if (surfacePoint != null) hook.transform.position = surfacePoint.position;
                hook.GoIdle();
            }
            if (gameUI != null) gameUI.ShowIdle();
        }

        private void Update()
        {
            if (State == GameState.Idle && DetectCastInput())
            {
                StartCast();
            }
        }

        /// <summary>
        /// Transitions Idle → Descending and tells the hook to drop.
        /// Called by the cast-input detection in Update.
        /// </summary>
        public void StartCast()
        {
            if (State != GameState.Idle) return;
            State = GameState.Descending;
            CatchCount = 0;
            CoinsEarned = 0;
            if (hook != null) hook.StartDescent();
            if (gameUI != null) gameUI.ShowInGame();
        }

        /// <summary>
        /// Called by Hook when it reaches the bottom trigger.
        /// Transitions Descending → Ascending.
        /// </summary>
        public void OnHookReachedBottom()
        {
            if (State != GameState.Descending) return;
            State = GameState.Ascending;
            if (hook != null) hook.StartAscent();
        }

        /// <summary>
        /// Called by Hook when it collides with a fish. Increments the catch count;
        /// if currently descending, also flips to Ascending.
        /// </summary>
        public void OnFishCaught(int coinValue)
        {
            CatchCount++;
            CoinsEarned += coinValue;
            if (State == GameState.Descending)
            {
                State = GameState.Ascending;
                if (hook != null) hook.StartAscent();
            }
        }

        /// <summary>
        /// Called by Hook when it reaches the surface trigger while ascending.
        /// Ends the round and shows the results panel.
        /// </summary>
        public void OnHookReachedSurface()
        {
            if (State != GameState.Ascending) return;
            State = GameState.Results;
            if (hook != null) hook.GoIdle();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCoins(CoinsEarned);
            }
            else
            {
                Debug.LogWarning("[GameSceneController] GameManager.Instance is null — coins not persisted. Did you place a GameManager in StartScreen?");
            }
            if (gameUI != null) gameUI.ShowResults(CatchCount, CoinsEarned);
        }

        /// <summary>
        /// Hooked up to the results screen's Return-to-Menu button.
        /// </summary>
        public void ReturnToMenu()
        {
            SceneLoader.LoadStartScreen();
        }

        private bool DetectCastInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) return true;
            return false;
#else
            if (Input.GetMouseButtonDown(0)) return true;
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
            if (Input.GetKeyDown(KeyCode.Space)) return true;
            return false;
#endif
        }
    }
}
