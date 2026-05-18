using UnityEngine;

namespace HolyMackerel.Game
{
    /// <summary>
    /// Smoothly follows a target's Y position with optional clamping. Locks X to 0
    /// so the portrait game has no horizontal scroll. Attach to the Main Camera.
    /// </summary>
    public class CameraFollower : MonoBehaviour
    {
        [Tooltip("Transform to follow vertically — typically the hook.")]
        [SerializeField] private Transform target;

        [Tooltip("How fast the camera catches up to the target. Higher = snappier follow.")]
        [SerializeField] private float smoothSpeed = 5f;

        [Tooltip("Lowest (most negative) Y the camera is allowed to reach.")]
        [SerializeField] private float minY = -30f;

        [Tooltip("Highest Y the camera is allowed to reach (usually the surface).")]
        [SerializeField] private float maxY = 0f;

        private void LateUpdate()
        {
            if (target == null) return;

            float desiredY = Mathf.Clamp(target.position.y, minY, maxY);
            Vector3 pos = transform.position;
            pos.y = Mathf.Lerp(pos.y, desiredY, smoothSpeed * Time.deltaTime);
            pos.x = 0f;
            transform.position = pos;
        }
    }
}
