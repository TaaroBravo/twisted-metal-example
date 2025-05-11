using System.Collections.Generic;

namespace LocalizeProTool.Scripts.DeepL
{
    public static class DeepLTargetLanguageHelper
    {
        private static readonly Dictionary<DeepLTargetLanguage, string> languageNames = new Dictionary<DeepLTargetLanguage, string>
        {
            { DeepLTargetLanguage.AR, "Arabic" },
            { DeepLTargetLanguage.BG, "Bulgarian" },
            { DeepLTargetLanguage.CS, "Czech" },
            { DeepLTargetLanguage.DA, "Danish" },
            { DeepLTargetLanguage.DE, "German" },
            { DeepLTargetLanguage.EL, "Greek" },
            { DeepLTargetLanguage.EN, "English (unspecified)" },
            { DeepLTargetLanguage.EN_GB, "English (British)" },
            { DeepLTargetLanguage.EN_US, "English (American)" },
            { DeepLTargetLanguage.ES, "Spanish" },
            { DeepLTargetLanguage.ET, "Estonian" },
            { DeepLTargetLanguage.FI, "Finnish" },
            { DeepLTargetLanguage.FR, "French" },
            { DeepLTargetLanguage.HU, "Hungarian" },
            { DeepLTargetLanguage.ID, "Indonesian" },
            { DeepLTargetLanguage.IT, "Italian" },
            { DeepLTargetLanguage.JA, "Japanese" },
            { DeepLTargetLanguage.KO, "Korean" },
            { DeepLTargetLanguage.LT, "Lithuanian" },
            { DeepLTargetLanguage.LV, "Latvian" },
            { DeepLTargetLanguage.NB, "Norwegian Bokmål" },
            { DeepLTargetLanguage.NL, "Dutch" },
            { DeepLTargetLanguage.PL, "Polish" },
            { DeepLTargetLanguage.PT, "Portuguese (unspecified)" },
            { DeepLTargetLanguage.PT_BR, "Portuguese (Brazilian)" },
            { DeepLTargetLanguage.PT_PT, "Portuguese (excluding Brazilian)" },
            { DeepLTargetLanguage.RO, "Romanian" },
            { DeepLTargetLanguage.RU, "Russian" },
            { DeepLTargetLanguage.SK, "Slovak" },
            { DeepLTargetLanguage.SL, "Slovenian" },
            { DeepLTargetLanguage.SV, "Swedish" },
            { DeepLTargetLanguage.TR, "Turkish" },
            { DeepLTargetLanguage.UK, "Ukrainian" },
            { DeepLTargetLanguage.ZH, "Chinese (unspecified)" },
            { DeepLTargetLanguage.ZH_HANS, "Chinese (simplified)" },
            { DeepLTargetLanguage.ZH_HANT, "Chinese (traditional)" }
        };

        public static string GetLanguageName(DeepLTargetLanguage language)
        {
            return languageNames.TryGetValue(language, out var name) ? name : "Unknown";
        }
    }
}
