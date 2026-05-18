using TMPro;
using UnityEngine;

namespace HolyMackerel.StartScreen
{
    public class PressToStartBlink : MonoBehaviour
    {
        [Tooltip("Target TMP text whose alpha will be animated.")]
        public TMP_Text targetText;

        [Tooltip("How fast the alpha ping-pongs. Higher = faster blink.")]
        public float blinkSpeed = 1.5f;

        [Range(0f, 1f)]
        public float minAlpha = 0.2f;

        [Range(0f, 1f)]
        public float maxAlpha = 1f;

        private void Reset()
        {
            targetText = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            if (targetText == null) return;

            float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

            Color c = targetText.color;
            c.a = alpha;
            targetText.color = c;
        }
    }
}
