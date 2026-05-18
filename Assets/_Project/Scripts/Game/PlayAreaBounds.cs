using UnityEngine;

namespace HolyMackerel.Game
{
    /// <summary>
    /// Editor-only helper: visualizes the playable depth range with Gizmos so the
    /// surface line, bottom line, and left/right walls are visible while designing
    /// the scene. Has no runtime behavior.
    /// </summary>
    public class PlayAreaBounds : MonoBehaviour
    {
        [Tooltip("Y of the water surface (top of the play area).")]
        [SerializeField] private float surfaceY = 0f;

        [Tooltip("Y of the deepest point of the play area.")]
        [SerializeField] private float bottomY = -25f;

        [Tooltip("Half-width to draw the surface/bottom lines across.")]
        [SerializeField] private float halfWidth = 5f;

        private void OnDrawGizmos()
        {
            Vector3 surfL = new Vector3(-halfWidth, surfaceY, 0f);
            Vector3 surfR = new Vector3(halfWidth, surfaceY, 0f);
            Vector3 botL = new Vector3(-halfWidth, bottomY, 0f);
            Vector3 botR = new Vector3(halfWidth, bottomY, 0f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(surfL, surfR);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(botL, botR);

            Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);
            Gizmos.DrawLine(surfL, botL);
            Gizmos.DrawLine(surfR, botR);
        }
    }
}
