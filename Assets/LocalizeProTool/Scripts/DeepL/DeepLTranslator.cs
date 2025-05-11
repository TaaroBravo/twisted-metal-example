using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalizeProTool.Scripts.DeepL
{
    public class DeepLTranslator
    {
        private readonly string apiKey;
        private const string ApiUrl = "https://api-free.deepl.com/v2/translate";
        private const string SettingsResourcePath = "Translator/DeepLSettings";

        /// <summary>
        /// Initializes a new DeepLTranslator instance by automatically loading DeepLSettingsSO from Resources.
        /// </summary>
        public DeepLTranslator()
        {
            var settings = Resources.Load<DeepLSettingsSO>(SettingsResourcePath);
            if (settings == null || string.IsNullOrEmpty(settings.apiKey))
                throw new Exception($"Could not load DeepL API key from: Resources/{SettingsResourcePath}");
            apiKey = settings.apiKey;
        }

        /// <summary>
        /// Translates a single string using DeepL API and custom translation options.
        /// </summary>
        /// <param name="text">Text to translate</param>
        /// <param name="options">Custom options for DeepL</param>
        /// <param name="onSuccess">Callback with translated text</param>
        /// <param name="onError">Callback with error message</param>
        public IEnumerator TranslateText(
            string text,
            DeepLTranslationOptions options,
            Action<string> onSuccess,
            Action<string> onError)
        {
            return TranslateText(
                new List<string> {text},
                options,
                results => onSuccess?.Invoke(results[0]),
                onError
            );
        }

        /// <summary>
        /// Translates a list of strings using DeepL API and custom translation options.
        /// </summary>
        /// <param name="texts">Texts to translate</param>
        /// <param name="options">Custom options for DeepL</param>
        /// <param name="onSuccess">Callback with translated text</param>
        /// <param name="onError">Callback with error message</param>
        public IEnumerator TranslateText(
            List<string> texts,
            DeepLTranslationOptions options,
            Action<List<string>> onSuccess,
            Action<string> onError)
        {
            var wrapper = new RequestWrapper(texts, options);
            string jsonBody = JsonUtility.ToJson(wrapper);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            using (UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "DeepL-Auth-Key " + apiKey);

                yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                if (request.result == UnityWebRequest.Result.ConnectionError ||
                    request.result == UnityWebRequest.Result.ProtocolError)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
                {
                    onError?.Invoke($"Error {request.responseCode}: {request.downloadHandler.text}");
                }
                else
                {
                    try
                    {
                        var response = JsonUtility.FromJson<DeepLTranslationResponse>(request.downloadHandler.text);
                        List<string> translated = new List<string>();
                        foreach (var t in response.translations)
                            translated.Add(t.text);
                        onSuccess?.Invoke(translated);
                    }
                    catch (Exception ex)
                    {
                        onError?.Invoke("Failed to parse response: " + ex.Message);
                    }
                }
            }
        }
        
        /// <summary>
        /// Translates a list of texts with default translation options.
        /// </summary>
        /// <param name="texts">Texts to translate</param>
        /// <param name="targetLang">Target language</param>
        /// <param name="onSuccess">Callback with list of translated texts</param>
        /// <param name="onError">Callback with error message</param>
        public IEnumerator TranslateText(
            List<string> texts,
            DeepLTargetLanguage targetLang,
            Action<List<string>> onSuccess,
            Action<string> onError)
        {
            var defaultOptions = new DeepLTranslationOptions(targetLang);
            return TranslateText(texts, defaultOptions, onSuccess, onError);
        }

        /// <summary>
        /// Translates a single text with default translation options.
        /// </summary>
        /// <param name="text">Text to translate</param>
        /// <param name="targetLang">Target language</param>
        /// <param name="onSuccess">Callback with translated text</param>
        /// <param name="onError">Callback with error message</param>
        public IEnumerator TranslateText(
            string text,
            DeepLTargetLanguage targetLang,
            Action<string> onSuccess,
            Action<string> onError)
        {
            var defaultOptions = new DeepLTranslationOptions(targetLang);
            return TranslateText(text, defaultOptions, onSuccess, onError);
        }


        [Serializable]
        private class RequestWrapper
        {
            public string[] text;
            public string target_lang;
            public string source_lang;
            public string formality;
            public string split_sentences;
            public bool preserve_formatting;

            public RequestWrapper(List<string> texts, DeepLTranslationOptions options)
            {
                text = texts.ToArray();
                target_lang = options.TargetLanguage.ToString().Replace("_", "-");
                source_lang = options.SourceLanguage?.ToString();
                formality = options.Formality;
                split_sentences = options.SplitSentences;
                preserve_formatting = options.PreserveFormatting;
            }
        }
    }
}