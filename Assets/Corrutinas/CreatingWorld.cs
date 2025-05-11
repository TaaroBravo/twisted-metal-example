using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Corrutinas
{
    public class CreatingWorld : MonoBehaviour
    {
        [SerializeField] private List<GameObject> mapChunks;
    
        private void Start()
        {
            foreach (var chuck in mapChunks)
                chuck.SetActive(false);
            StartCoroutine(LoadingMap());
        }
    
        public IEnumerator LoadingMap()
        {
            foreach (GameObject chunk in mapChunks)
            {
                chunk.SetActive(true);              
                yield return null;                  
            }
        }
    }
}