using UnityEngine;
using HolyMackerel.Core;

namespace HolyMackerel.Shop
{
    /// <summary>
    /// Controls one upgrade row's progress bar in ShopScene. Reads the current
    /// level for its <see cref="upgradeType"/> from <see cref="GameManager"/> on
    /// Start, then sizes two stacked sprites: a dark-green fill (owned levels)
    /// and a bright-green fill (next level preview). Cost display and button
    /// affordability are handled elsewhere — this component is bar-only.
    /// </summary>
    public class ShopRow : MonoBehaviour
    {
        public enum UpgradeType { Boat, Depth, Bait }

        [Tooltip("Which upgrade this row represents. Determines which PlayerData field is read.")]
        [SerializeField] private UpgradeType upgradeType = UpgradeType.Boat;

        [Tooltip("Maximum level cap for this upgrade. Boat=3, Depth=4, Bait=5.")]
        [SerializeField] private int maxLevel = 3;

        [Tooltip("Anchor Transform for the dark-green fill (owned levels). Anchor sits at the bar's left edge; its child sprite is offset so its left edge aligns with the anchor's origin. Scaling this anchor on X grows the fill rightward.")]
        [SerializeField] private Transform darkGreenFill;

        [Tooltip("Anchor Transform for the bright-green fill (next level preview). Same left-edge anchor pattern as darkGreenFill.")]
        [SerializeField] private Transform brightGreenFill;

        [Tooltip("The Scale X value the bar uses when fully filled. Should match the grey track sprite's Scale X.")]
        [SerializeField] private float fullBarScaleX = 1f;

        private void Start()
        {
            int currentLevel = ReadCurrentLevel();

            if (currentLevel >= maxLevel)
            {
                ApplyScaleX(darkGreenFill, fullBarScaleX, "darkGreenFill");
                ApplyScaleX(brightGreenFill, 0f, "brightGreenFill");
                return;
            }

            float segmentWidth = fullBarScaleX / maxLevel;
            float darkGreenWidth = currentLevel * segmentWidth;
            float brightGreenWidth = Mathf.Min((currentLevel + 1) * segmentWidth, fullBarScaleX);

            ApplyScaleX(darkGreenFill, darkGreenWidth, "darkGreenFill");
            ApplyScaleX(brightGreenFill, brightGreenWidth, "brightGreenFill");
        }

        private int ReadCurrentLevel()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("[ShopRow] GameManager.Instance is null — using level 1 fallback.");
                return 1;
            }

            PlayerData data = GameManager.Instance.Data;
            if (data == null)
            {
                Debug.LogWarning($"[ShopRow] GameManager.Instance.Data is null on upgradeType={upgradeType} — defaulting to level 1.");
                return 1;
            }

            switch (upgradeType)
            {
                case UpgradeType.Boat: return data.boatLevel;
                case UpgradeType.Depth: return data.depthLevel;
                case UpgradeType.Bait: return data.baitLevel;
                default: return 1;
            }
        }

        private void ApplyScaleX(Transform target, float x, string fieldName)
        {
            if (target == null)
            {
                Debug.LogWarning($"[ShopRow] {fieldName} is not assigned on upgradeType={upgradeType} — skipping.");
                return;
            }
            Vector3 s = target.localScale;
            s.x = x;
            target.localScale = s;
        }
    }
}
