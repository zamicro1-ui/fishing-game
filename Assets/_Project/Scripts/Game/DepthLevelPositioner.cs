using UnityEngine;
using HolyMackerel.Core;

namespace HolyMackerel.Game
{
    /// <summary>
    /// Positions the GameObject's Y on scene start based on the player's
    /// current depth-upgrade level read from <see cref="GameManager"/>. The
    /// final Y is <c>baseY + (depthLevel - 1) * deltaPerLevel</c>. Defaults
    /// give the bottom of the play area: level 1 → -15, level 2 → -20,
    /// level 3 → -25, level 4 → -30, level 5 → -35.
    ///
    /// Attach to the "Bottom"-tagged trigger GameObject (and to any visual
    /// element that should move with it — e.g. a "MAX DEPTH" sign sprite —
    /// each with its own baseY).
    /// </summary>
    public class DepthLevelPositioner : MonoBehaviour
    {
        [Tooltip("Y position at depth level 1 (the baseline). Set to whatever this object's level-1 height should be.")]
        [SerializeField] private float baseY = -15f;

        [Tooltip("How much Y shifts per level above 1. Negative moves the object deeper. Default -5.")]
        [SerializeField] private float deltaPerLevel = -5f;

        private void Start()
        {
            int depthLevel = ReadDepthLevel();
            float targetY = baseY + (depthLevel - 1) * deltaPerLevel;
            Vector3 p = transform.position;
            p.y = targetY;
            transform.position = p;
        }

        private int ReadDepthLevel()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning($"[DepthLevelPositioner] GameManager.Instance is null on {name} — defaulting to level 1.");
                return 1;
            }
            PlayerData data = GameManager.Instance.Data;
            if (data == null)
            {
                Debug.LogWarning($"[DepthLevelPositioner] GameManager.Instance.Data is null on {name} — defaulting to level 1.");
                return 1;
            }
            return Mathf.Max(1, data.depthLevel);
        }
    }
}
