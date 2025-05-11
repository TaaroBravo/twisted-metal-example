using System.Collections.Generic;

namespace LocalizeProTool.Scripts.DeepL
{
    public static class DeepLValidationHelper
    {
        /// <summary>
        /// Languages that support the "formality" parameter according to DeepL docs.
        /// </summary>
        private static readonly HashSet<DeepLTargetLanguage> languagesWithFormality = new HashSet<DeepLTargetLanguage>
        {
            DeepLTargetLanguage.DE,     // German
            DeepLTargetLanguage.FR,     // French
            DeepLTargetLanguage.IT,     // Italian
            DeepLTargetLanguage.ES,     // Spanish
            DeepLTargetLanguage.NL,     // Dutch
            DeepLTargetLanguage.PL,     // Polish
            DeepLTargetLanguage.PT_BR,  // Portuguese (Brazilian)
            DeepLTargetLanguage.PT_PT   // Portuguese (Portugal)
        };

        /// <summary>
        /// Checks if the given target language supports the "formality" option.
        /// </summary>
        public static bool SupportsFormality(DeepLTargetLanguage lang)
        {
            return languagesWithFormality.Contains(lang);
        }
    }
}