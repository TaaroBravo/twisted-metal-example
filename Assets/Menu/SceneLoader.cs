using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }
        public static string TargetSceneName { get; private set; }
        public static string PreviousSceneName { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); 
            }
            else
                Destroy(gameObject);
        }


        public static void LoadSceneWithLoading(string targetScene)
        {
            PreviousSceneName = SceneManager.GetActiveScene().name;
            TargetSceneName = targetScene;
            SceneManager.LoadScene("LoadingScene", LoadSceneMode.Additive);
        }

    }
}