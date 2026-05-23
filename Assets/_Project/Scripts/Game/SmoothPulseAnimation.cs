using UnityEngine;

public class SmoothPulseAnimation : MonoBehaviour
{
    [Header("Scale Settings")]
    [Tooltip("The minimum scale size.")]
    public float minScale = 0.27f;
    
    [Tooltip("The maximum scale size.")]
    public float maxScale = 0.30f;

    [Tooltip("How fast the object scales up and down (cycles per second).")]
    public float speed = 2.0f;

    private void Update()
    {
        // Mathf.PingPong moves smoothly between 0 and 1 back and forth over time
        float t = Mathf.PingPong(Time.time * speed, 1f);

        // Mathf.SmoothStep adds ease-in and ease-out so it doesn't look robotic
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        // Interpolate between our min and max scale values
        float currentScale = Mathf.Lerp(minScale, maxScale, smoothT);

        // Apply the new uniform scale to the 2D object
        transform.localScale = new Vector3(currentScale, currentScale, 1f);
    }
}