using UnityEngine;
using HolyMackerel.Core;

namespace HolyMackerel.Hub
{
    /// <summary>
    /// Attach to the boat GameObject in the HubScene. A tap or click anywhere on
    /// the attached Collider2D loads the LocationSelectScene. Repeated taps are
    /// swallowed by an isTransitioning guard so a fast double-tap can't stack
    /// scene loads.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BoatTapHandler : MonoBehaviour
    {
        private bool isTransitioning;

        private void OnMouseDown()
        {
            if (isTransitioning) return;
            isTransitioning = true;
            // TODO: trigger a fade-out / transition effect here, then load on completion.
            SceneLoader.LoadLocationSelectScene();
        }
    }
}
