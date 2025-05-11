using System;
using UnityEngine;

namespace LocalizeProTool.Scripts
{
    public class LocalizationData : ScriptableObject
    {
        public string language;
        public LocalizationContent[] content;
    }
    
    [Serializable]
    public struct LocalizationContent
    {
        public string tid;
        public string text;
    }
}