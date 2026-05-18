using UnityEngine;
using UnityEngine.SceneManagement;
using HolyMackerel.Core;

namespace HolyMackerel.Shop
{
    /// <summary>
    /// Click handler for an upgrade row's green (affordable) buy button.
    /// Deducts coins, increments the matching level field on
    /// <see cref="PlayerData"/>, persists via <see cref="GameManager.Save"/>,
    /// and reloads the active scene so all ShopRow* components refresh from
    /// the updated data.
    ///
    /// Requires:
    ///   - A Collider2D (typically BoxCollider2D) on the same GameObject so
    ///     OnMouseDown fires on click.
    ///   - Attach ONLY to the green button GameObject. The grey button is
    ///     purely decorative and should have no collider and no script.
    /// </summary>
    public class ShopRowBuyButton : MonoBehaviour
    {
        [Tooltip("ScriptableObject defining this row's cost curve, upgrade type, and max level.")]
        [SerializeField] private UpgradeConfig config;

        private void OnMouseDown()
        {
            if (config == null)
            {
                Debug.LogWarning($"[ShopRowBuyButton] config is not assigned on {name}.");
                return;
            }

            if (GameManager.Instance == null || GameManager.Instance.Data == null)
            {
                Debug.LogWarning("[ShopRowBuyButton] GameManager unavailable — purchase aborted.");
                return;
            }

            PlayerData data = GameManager.Instance.Data;
            int currentLevel = ReadCurrentLevel(data);

            if (config.IsMaxLevel(currentLevel))
            {
                Debug.LogWarning($"[ShopRowBuyButton] Already at max level for {config.UpgradeType} — purchase ignored.");
                return;
            }

            int cost = config.GetCostForNextLevel(currentLevel);
            if (data.coins < cost)
            {
                Debug.LogWarning($"[ShopRowBuyButton] Insufficient coins for {config.UpgradeType} (have {data.coins}, need {cost}) — purchase ignored.");
                return;
            }

            data.coins -= cost;
            switch (config.UpgradeType)
            {
                case ShopRow.UpgradeType.Boat: data.boatLevel += 1; break;
                case ShopRow.UpgradeType.Depth: data.depthLevel += 1; break;
                case ShopRow.UpgradeType.Bait: data.baitLevel += 1; break;
            }

            GameManager.Instance.Save();

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private int ReadCurrentLevel(PlayerData data)
        {
            switch (config.UpgradeType)
            {
                case ShopRow.UpgradeType.Boat: return data.boatLevel;
                case ShopRow.UpgradeType.Depth: return data.depthLevel;
                case ShopRow.UpgradeType.Bait: return data.baitLevel;
                default: return 1;
            }
        }
    }
}
