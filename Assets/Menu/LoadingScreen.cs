using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menu
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private Slider loadingBar;
        [SerializeField] private float smoothSpeed = 0.5f;
        [SerializeField] private float delayBeforeActivate = 1f;

        private float targetProgress = 0f;
        private float currentProgress = 0f;
        private AsyncOperation loadingOperation;

        void Start()
        {
            string targetScene = SceneLoader.TargetSceneName;
            StartCoroutine(LoadSceneAsync(targetScene));
        }

        IEnumerator LoadSceneAsync(string sceneToLoad)
        {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            loadOp.allowSceneActivation = false;

            while (!loadOp.isDone)
            {
                targetProgress = Mathf.Clamp01(loadOp.progress / 0.9f);
                currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime / smoothSpeed);
                loadingBar.value = currentProgress;

                if (currentProgress >= 0.99f && loadOp.progress >= 0.9f)
                {
                    loadingBar.value = 1f;
                    yield return new WaitForSeconds(delayBeforeActivate);

                    loadOp.allowSceneActivation = true;

                    while (!loadOp.isDone)
                        yield return null;

                    Scene newScene = SceneManager.GetSceneByName(sceneToLoad);
                    if (newScene.IsValid())
                        SceneManager.SetActiveScene(newScene);

                    string previousScene = SceneLoader.PreviousSceneName;
                    if (!string.IsNullOrEmpty(previousScene))
                    {
                        Scene sceneToUnload = SceneManager.GetSceneByName(previousScene);
                        if (sceneToUnload.IsValid())
                            SceneManager.UnloadSceneAsync(sceneToUnload);
                    }

                    SceneManager.UnloadSceneAsync("LoadingScene");

                }

                yield return null;
            }
        }

    }
}