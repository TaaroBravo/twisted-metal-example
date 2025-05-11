using System;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class MenuExample : MonoBehaviour
    {
        public Button start;

        private void Start()
        {
            start.onClick.AddListener(() => SceneLoader.LoadSceneWithLoading("GameScene"));
        }
    }
}
