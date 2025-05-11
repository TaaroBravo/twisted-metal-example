using UnityEngine;

namespace LocalizeProTool.Scripts.DeepL
{
    [CreateAssetMenu(fileName = "DeepLSettings", menuName = "LocalizePro/DeepL Settings", order = 0)]
    public class DeepLSettingsSO : ScriptableObject
    {
        public string apiKey;
    }
}