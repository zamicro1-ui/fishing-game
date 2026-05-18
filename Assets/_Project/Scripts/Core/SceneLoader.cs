using UnityEngine.SceneManagement;

namespace HolyMackerel.Core
{
    public static class SceneLoader
    {
        public const string StartScreenSceneName = "StartScreen";
        public const string GameSceneName = "GameScene";
        public const string HubSceneName = "HubScene";
        public const string LocationSelectSceneName = "LocationSelectScene";
        public const string ShopSceneName = "ShopScene";

        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void LoadGameScene()
        {
            LoadScene(GameSceneName);
        }

        public static void LoadStartScreen()
        {
            LoadScene(StartScreenSceneName);
        }

        public static void LoadHubScene()
        {
            LoadScene(HubSceneName);
        }

        public static void LoadLocationSelectScene()
        {
            LoadScene(LocationSelectSceneName);
        }

        public static void LoadShopScene()
        {
            LoadScene(ShopSceneName);
        }
    }
}
