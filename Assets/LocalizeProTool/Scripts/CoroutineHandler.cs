using System.Collections;
using UnityEngine;

namespace LocalizeProTool.Scripts
{
    public class CoroutineHandler : MonoBehaviour
    {
        private static CoroutineHandler _instance;

        public static CoroutineHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CoroutineHandler>();

                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("CoroutineHandler");
                        _instance = obj.AddComponent<CoroutineHandler>();
                    }
                }

                return _instance;
            }
        }

        public void StartDownloadCoroutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }

        public void Clear()
        {
            DestroyImmediate(gameObject);
        }
    }
}