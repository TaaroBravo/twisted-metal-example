using System;
using System.Collections.Generic;

namespace LocalizeProTool.Scripts.DeepL
{
    [Serializable]
    public class DeepLTranslationResponse
    {
        [Serializable]
        public class TranslationResult
        {
            public string text;
            public string detected_source_language;
        }

        public List<TranslationResult> translations;
    }
}