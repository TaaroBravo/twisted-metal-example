using System;
using LocalizeProTool.Scripts.DeepL;

namespace LocalizeProTool.Scripts.Editor
{
    [Serializable]
    public class DeepLTranslationConfig
    {
        public DeepLTargetLanguage targetLanguage;
        public bool showCustomOptions = false;
        public string formality = "default";
        public bool preserveFormatting = false;
        public string splitSentences = "1";

        public bool SupportsFormality =>
            DeepLValidationHelper.SupportsFormality(targetLanguage);
    }
}