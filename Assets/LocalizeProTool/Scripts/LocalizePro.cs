using System.IO;
using System.Linq;
using UnityEngine;

namespace LocalizeProTool.Scripts
{
    /// <summary>
    /// The LocalizePro allows you to get translated/localized text from a TID.
    /// </summary>
    public class LocalizePro : MonoBehaviour
    {        
        private static LocalizePro _instance;
        /// <summary>
        /// Get LocalizePro or create it if it does not exist.
        /// </summary>
        public static LocalizePro Instance  
        {
            get
            {
                if (_instance == null)
                {
                    var gameObject = new GameObject("LocalizePro");
                    _instance = gameObject.AddComponent<LocalizePro>();
                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        private void Awake()
        {
            _instance = this;
        }
        
        /// <summary>
        /// Get Localized Text for an TID
        /// </summary>
        /// <param name="tid">The text ID</param>
        public string GetTextFor(string tid)
        {
            var localizationData = Resources.Load<LocalizationData>("LocalizationData-"+GetLanguage());
            if (localizationData == null)
            {
                Debug.LogError($"Error {tid} for {GetLanguage()}. Couldn't found LocalizationData-{GetLanguage()}");
                return tid;
            }
            var obj = localizationData.content.FirstOrDefault(c => c.tid == tid);
            if (string.IsNullOrEmpty(obj.tid))
            {
                Debug.LogError($"Error {tid} for {GetLanguage()}");
                return tid;
            }
            return obj.text;
        }

        /// <summary>
        /// Get Current Language or the first one of the available
        /// </summary>
        public string GetLanguage()
        {
            if (!PlayerPrefs.HasKey("Language"))
            {
                var languages = Resources.Load<LanguagesAvailable>("LanguagesAvailable");
                PlayerPrefs.SetString("Language", languages.languages.First());
            }
            return NormalizeLanguageName(PlayerPrefs.GetString("Language"));
        }

        /// <summary>
        /// Set new Language and update all TIDs
        /// </summary>
        public void SetLanguage(string language)
        {
            PlayerPrefs.SetString("Language", NormalizeLanguageName(language));
            foreach (var tidTextMeshProUGUI in FindObjectsByType<TID_TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                tidTextMeshProUGUI.ForceUpdate();
            }
        }
        
        private string NormalizeLanguageName(string language)
        {
            if (string.IsNullOrEmpty(language))
                return language;

            string normalizedLanguage = language;

            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalidChars)
            {
                normalizedLanguage = normalizedLanguage.Replace(c.ToString(), "");
            }

            normalizedLanguage = RemoveDiacritics(normalizedLanguage);

            normalizedLanguage = normalizedLanguage.Replace(" ", "_");

            if (!string.IsNullOrEmpty(normalizedLanguage))
            {
                normalizedLanguage = char.ToUpper(normalizedLanguage[0]) + normalizedLanguage.Substring(1).ToLower();
            }

            return normalizedLanguage;
        }
        
        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }
    }
}
