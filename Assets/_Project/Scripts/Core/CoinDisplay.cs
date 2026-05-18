using TMPro;
using UnityEngine;

namespace HolyMackerel.Core
{
    /// <summary>
    /// One-shot display of the player's persistent coin balance. Reads
    /// <see cref="GameManager.Instance"/>.Data.coins on Start and writes it into
    /// the assigned TMP text. Attach to any GameObject (typically the coin
    /// counter sprite) and drag a TMP child into <c>coinText</c>.
    /// </summary>
    public class CoinDisplay : MonoBehaviour
    {
        [Tooltip("The TMP text component to update with the current coin balance.")]
        [SerializeField] private TextMeshPro coinText;

        private void Start()
        {
            if (coinText == null)
            {
                Debug.LogWarning("[CoinDisplay] coinText is not assigned in the Inspector.");
                return;
            }

            int coins = GameManager.Instance != null ? GameManager.Instance.Data.coins : 0;
            coinText.text = coins.ToString();
        }
    }
}
