using UnityEngine;

namespace HolyMackerel.Shop
{
    /// <summary>
    /// ScriptableObject defining the cost curve and cap for a single upgrade
    /// type. Authored as an asset (one per upgrade); consumed at runtime by
    /// <see cref="ShopRowCostDisplay"/> and any future purchase logic.
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradeConfig", menuName = "HolyMackerel/Upgrade Config", order = 0)]
    public class UpgradeConfig : ScriptableObject
    {
        [Tooltip("Which upgrade this config drives. Matched against ShopRow.UpgradeType when a row reads its level.")]
        [SerializeField] private ShopRow.UpgradeType upgradeType = ShopRow.UpgradeType.Boat;

        [Tooltip("Cost of reaching level 2 — the first purchase from the level-1 starting state.")]
        [SerializeField] private int baseCost = 100;

        [Tooltip("Multiplier applied per level beyond the base. Cost(currentLevel) = baseCost * costMultiplier^(currentLevel-1), rounded.")]
        [SerializeField] private float costMultiplier = 1.5f;

        [Tooltip("Maximum attainable level. At currentLevel >= maxLevel there is no next purchase.")]
        [SerializeField] private int maxLevel = 4;

        public ShopRow.UpgradeType UpgradeType => upgradeType;
        public int MaxLevel => maxLevel;

        /// <summary>
        /// Coin cost to upgrade FROM <paramref name="currentLevel"/> TO currentLevel + 1.
        /// Returns -1 if there is no next level (defensive — callers should check
        /// <see cref="IsMaxLevel"/> first).
        /// </summary>
        public int GetCostForNextLevel(int currentLevel)
        {
            if (currentLevel >= maxLevel) return -1;
            if (currentLevel < 1) currentLevel = 1;
            return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel - 1));
        }

        public bool IsMaxLevel(int currentLevel)
        {
            return currentLevel >= maxLevel;
        }
    }
}
