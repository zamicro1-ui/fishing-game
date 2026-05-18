using UnityEngine;
using TMPro;
using HolyMackerel.Core;

namespace HolyMackerel.Shop
{
    /// <summary>
    /// One-shot writer for an upgrade row's "LVL X" TMP label. Reads the current
    /// level for its <see cref="upgradeType"/> from <see cref="GameManager"/> on
    /// Start and formats it into <see cref="label"/>. Pairs with <see cref="ShopRow"/>,
    /// which drives the progress bar on the same row. Duplication of the level
    /// switch with ShopRow.ReadCurrentLevel is intentional — each component
    /// stays self-contained.
    /// </summary>
    public class ShopRowLevelLabel : MonoBehaviour
    {
        [Tooltip("Which upgrade's level to display. Determines which PlayerData field is read.")]
        [SerializeField] private ShopRow.UpgradeType upgradeType = ShopRow.UpgradeType.Boat;

        [Tooltip("The TextMeshPro text component to write the formatted level into.")]
        [SerializeField] private TMP_Text label;

        [Tooltip("Format string. {0} is replaced with the current level number.")]
        [SerializeField] private string format = "LVL {0}";

        private void Start()
        {
            int currentLevel = ReadCurrentLevel();

            if (label == null)
            {
                Debug.LogWarning($"[ShopRowLevelLabel] label is not assigned on upgradeType={upgradeType} — skipping.");
                return;
            }

            label.text = string.Format(format, currentLevel);
        }

        private int ReadCurrentLevel()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogWarning($"[ShopRowLevelLabel] GameManager.Instance is null on upgradeType={upgradeType} — defaulting to level 1.");
                return 1;
            }

            PlayerData data = GameManager.Instance.Data;
            if (data == null)
            {
                Debug.LogWarning($"[ShopRowLevelLabel] GameManager.Instance.Data is null on upgradeType={upgradeType} — defaulting to level 1.");
                return 1;
            }

            switch (upgradeType)
            {
                case ShopRow.UpgradeType.Boat: return data.boatLevel;
                case ShopRow.UpgradeType.Depth: return data.depthLevel;
                case ShopRow.UpgradeType.Bait: return data.baitLevel;
                default: return 1;
            }
        }
    }
}
