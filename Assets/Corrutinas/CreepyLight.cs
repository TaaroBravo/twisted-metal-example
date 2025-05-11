using System.Collections;
using UnityEngine;

namespace Corrutinas
{
    public class CreepyLight : MonoBehaviour
    {
        [SerializeField] private Light targetLight;
        [SerializeField] private float minDelay = 0.3f;        
        [SerializeField] private float maxDelay = 1f;      
        [SerializeField] private float minIntensity = 0.1f;    
        [SerializeField] private float maxIntensity = 1f;    

        private float currentTargetIntensity;

        private void Start()
        {
            StartCoroutine(FlickerRoutine());
        }

        private IEnumerator FlickerRoutine()
        {
            currentTargetIntensity = targetLight.intensity;

            while (true)
            {
                currentTargetIntensity = Random.Range(minIntensity, maxIntensity);

                var waitTime = Random.Range(minDelay, maxDelay);

                var elapsed = 0f;
                var startIntensity = targetLight.intensity;

                while (elapsed < waitTime)
                {
                    elapsed += Time.deltaTime;
                    var t = elapsed / waitTime;
                    targetLight.intensity = Mathf.Lerp(startIntensity, currentTargetIntensity, t);
                    yield return null;
                }
            }
        }
    }
}