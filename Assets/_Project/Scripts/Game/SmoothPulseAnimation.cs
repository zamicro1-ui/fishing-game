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

    // Stores our random time offset
    private float timeOffset;

    private void Start()
    {
        // Calculate a random offset so objects don't pulse perfectly in sync
        // A range between 0 and 20 provides plenty of variance
        timeOffset = Random.Range(0f, 20f);
    }

    private void Update()
    {
        // Adding the timeOffset shifts the starting position of the animation loop
        float t = Mathf.PingPong((Time.time + timeOffset) * speed, 1f);

        // Mathf.SmoothStep adds ease-in and ease-out so it doesn't look robotic
        float smoothT = Mathf.SmoothStep(0f, 1f, t);

        // Interpolate between our min and max scale values
        float currentScale = Mathf.Lerp(minScale, maxScale, smoothT);

        // Apply the new uniform scale to the 2D object
        transform.localScale = new Vector3(currentScale, currentScale, 1f);
    }
}