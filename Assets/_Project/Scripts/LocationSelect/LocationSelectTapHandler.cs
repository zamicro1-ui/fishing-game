using UnityEngine;
using HolyMackerel.Core;

namespace HolyMackerel.LocationSelect
{
    /// <summary>
    /// Attach to a button GameObject in the LocationSelectScene. A tap or click on
    /// the attached Collider2D loads the scene selected by <see cref="targetScene"/>.
    /// Repeated taps are swallowed by an isTransitioning guard so a fast double-tap
    /// can't stack scene loads. Mirrors the BoatTapHandler pattern.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LocationSelectTapHandler : MonoBehaviour
    {
        public enum TargetScene
        {
            StartScreen,
            HubScene,
            LocationSelectScene,
            GameScene,
            ShopScene
        }

        [Tooltip("Which scene to load when this button is tapped.")]
        [SerializeField] private TargetScene targetScene = TargetScene.GameScene;

        private bool isTransitioning;

        private void OnMouseDown()
        {
            if (isTransitioning) return;
            isTransitioning = true;
            // TODO: trigger a fade-out / transition effect here, then load on completion.
            LoadTargetScene();
        }

        private void LoadTargetScene()
        {
            switch (targetScene)
            {
                case TargetScene.StartScreen:
                    SceneLoader.LoadStartScreen();
                    break;
                case TargetScene.HubScene:
                    SceneLoader.LoadHubScene();
                    break;
                case TargetScene.LocationSelectScene:
                    SceneLoader.LoadLocationSelectScene();
                    break;
                case TargetScene.GameScene:
                    SceneLoader.LoadGameScene();
                    break;
                case TargetScene.ShopScene:
                    SceneLoader.LoadShopScene();
                    break;
            }
        }
    }
}
