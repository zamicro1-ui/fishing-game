using UnityEngine;

namespace HolyMackerel.Game
{
    /// <summary>
    /// Draws a two-point LineRenderer between the rod tip and the hook every frame.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class FishingLine : MonoBehaviour
    {
        [Header("Endpoints")]
        [Tooltip("Starting point of the line — typically an empty child of the boat at the rod tip.")]
        [SerializeField] private Transform rodTip;

        [Tooltip("Ending point of the line — the Hook GameObject.")]
        [SerializeField] private Transform hook;

        private LineRenderer line;

        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = 2;
        }

        private void Update()
        {
            if (rodTip == null || hook == null) return;
            line.SetPosition(0, rodTip.position);
            line.SetPosition(1, hook.position);
        }
    }
}
