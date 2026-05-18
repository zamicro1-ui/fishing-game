using UnityEngine;
using TMPro;
using HolyMackerel.Core;

namespace HolyMackerel.Shop
{
    /// <summary>
    /// One-shot writer for an upgrade row's cost TMP. Looks up the current
    /// level for its <see cref="UpgradeConfig"/>'s upgrade type from
    /// <see cref="GameManager"/>, then renders the cost of the next level
    /// (or hides the cost panel entirely at max level).
    /// </summary>
    public class ShopRowCostDisplay : MonoBehaviour
    {
        [Tooltip("ScriptableObject defining this row's cost curve and max level.")]
        [SerializeField] private UpgradeConfig config;

        [Tooltip("TMP text component that displays the cost number.")]
        [SerializeField] private TMP_Text costLabel;

        [Tooltip("Parent GameObject of the cost TMP (sprite/panel). Disabled at max level so the whole panel disappears.")]
        [SerializeField] private GameObject costPanelRoot;

        [Tooltip("Format string for the cost. {0} is replaced with the number. Use e.g. \"{0} coins\" for a suffix.")]
        [SerializeField] private string format = "{0}";

        private void Start()
        {
            if (config == null)
            {
                Debug.LogWarning($"[ShopRowCostDisplay] config is not assigned on {name} — disabling.");
                if (costPanelRoot != null) costPanelRoot.SetActive(false);
                return;
            }

            int currentLevel = ReadCurrentLevel();

            if (config.IsMaxLevel(currentLevel))
            {
                if (costPanelRoot != null) costPanelRoot.SetActive(false);
                return;
            }

            int cost = config.GetCostForNextLevel(currentLevel);

            if (costLabel == null)
            {
                Debug.LogWarning($"[ShopRowCostDisplay] costLabel is not assigned on {name} — skipping.");
                return;
            }

            costLabel.text = string.Format(format, cost);
            if (costPanelRoot != null) costPanelRoot.SetActive(true);
        }

        private int ReadCurrentLevel()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning($"[ShopRowCostDisplay] GameManager.Instance is null on upgradeType={config.UpgradeType} — defaulting to level 1.");
                return 1;
            }

            PlayerData data = GameManager.Instance.Data;
            if (data == null)
            {
                Debug.LogWarning($"[ShopRowCostDisplay] GameManager.Instance.Data is null on upgradeType={config.UpgradeType} — defaulting to level 1.");
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
    }
}
