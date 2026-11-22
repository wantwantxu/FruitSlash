using UnityEngine;

namespace FruitSlash
{
    public class BootLoader : MonoBehaviour
    {
        void Start()
        {
            Time.timeScale = 1f;
            SaveService.Load();
            AudioDirector.Ensure();
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    static class AppStartup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Boot()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            SaveService.Load();
        }
    }
}
