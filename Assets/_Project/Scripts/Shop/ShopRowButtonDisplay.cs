using UnityEngine;
using HolyMackerel.Core;

namespace HolyMackerel.Shop
{
    /// <summary>
    /// One-shot affordability switch for an upgrade row's buy buttons. Reads
    /// the current level and coin balance from <see cref="GameManager"/> on
    /// Start, then activates the green (affordable) button or the grey
    /// (unaffordable) button — or hides both at max level.
    /// </summary>
    public class ShopRowButtonDisplay : MonoBehaviour
    {
        [Tooltip("ScriptableObject defining this row's cost curve and max level.")]
        [SerializeField] private UpgradeConfig config;

        [Tooltip("The affordable/clickable buy button GameObject. Active when coins >= next-level cost.")]
        [SerializeField] private GameObject greenButton;

        [Tooltip("The unaffordable/disabled buy button GameObject. Active when coins < next-level cost.")]
        [SerializeField] private GameObject greyButton;

        private void Start()
        {
            if (config == null)
            {
                Debug.LogWarning($"[ShopRowButtonDisplay] config is not assigned on {name} — disabling both buttons.");
                if (greenButton != null) greenButton.SetActive(false);
                if (greyButton != null) greyButton.SetActive(false);
                return;
            }

            int currentLevel = ReadCurrentLevel();

            if (config.IsMaxLevel(currentLevel))
            {
                if (greenButton != null) greenButton.SetActive(false);
                if (greyButton != null) greyButton.SetActive(false);
                return;
            }

            int currentCoins = ReadCurrentCoins();
            int cost = config.GetCostForNextLevel(currentLevel);
            bool canAfford = currentCoins >= cost;

            if (greenButton != null) greenButton.SetActive(canAfford);
            if (greyButton != null) greyButton.SetActive(!canAfford);
        }

        private int ReadCurrentLevel()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning($"[ShopRowButtonDisplay] GameManager.Instance is null on upgradeType={config.UpgradeType} — defaulting to level 1.");
                return 1;
            }

            PlayerData data = GameManager.Instance.Data;
            if (data == null)
            {
                Debug.LogWarning($"[ShopRowButtonDisplay] GameManager.Instance.Data is null on upgradeType={config.UpgradeType} — defaulting to level 1.");
                return 1;
            }

            switch (config.UpgradeType)
            {
                case ShopRow.UpgradeType.Boat: return data.boatLevel;
                case ShopRow.UpgradeType.Depth: return data.depthLevel;
                case ShopRow.UpgradeType.Bait: return data.baitLevel;
                default: return 1;
            }
        }

        private int ReadCurrentCoins()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning($"[ShopRowButtonDisplay] GameManager.Instance is null on upgradeType={config.UpgradeType} — defaulting coins to 0.");
                return 0;
            }

            PlayerData data = GameManager.Instance.Data;
            if (data == null)
            {
                Debug.LogWarning($"[ShopRowButtonDisplay] GameManager.Instance.Data is null on upgradeType={config.UpgradeType} — defaulting coins to 0.");
                return 0;
            }

            return data.coins;
        }
    }
}
