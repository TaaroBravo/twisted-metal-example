using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LocalizeProTool.Scripts.DeepL;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace LocalizeProTool.Scripts.Editor
{
    public class LocalizationTool : EditorWindow
    {
        private const string SPREADSHEET_ID_KEY = "Localization.SpreadsheetId";
        private const string SHEET_IDS_KEY = "Localization.SheetIds";
        private const string DeepLSavedPrefsKey = "LocalizationTool.DeepLConfigs";

        private string spreadsheetId = "";
        private List<string> sheetIds = new List<string>();
        private Vector2 scrollPosition;
        private bool _manualUpload;
        private bool _showSheetIds = true;
        private bool _isDownloading;
        private float _downloadProgress;
        private bool _showAdvancedSettings = false;

        private static bool _useDeepLIntegration = false;
        private static bool _forceDeepLTranslations = false;
        private static List<DeepLTranslationConfig> _deeplLanguages = new();

        private bool _isTranslating;
        private float _translationProgress;
        private string _currentTranslationLanguage = "";

        [MenuItem("Tools/LocalizePro")]
        public static void OpenWindow()
        {
            GetWindow<LocalizationTool>("LocalizePro").Show();
        }

        private void OnEnable()
        {
            LoadData();
            LoadDeepLConfigs();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            DrawHeaderWithHelpButton();
            EditorGUILayout.Space();

            DrawSpreadsheetIdField();
            EditorGUILayout.Space();

            DrawSheetIdsSection();
            EditorGUILayout.Space();

            DrawDeepLIntegrationSection();
            EditorGUILayout.Space();

            DrawDownloadButton();

            if (_isTranslating)
            {
                Rect rect = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(rect, _translationProgress,
                    $"Translating with DeepL... {_currentTranslationLanguage}");
                GUILayout.Space(18);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();

            DrawManualUploadAndHardResetSection();

        }

        private void DrawHeaderWithHelpButton()
        {
            EditorGUILayout.BeginHorizontal();

            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("LocalizePro", headerStyle, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("?", GUILayout.Width(30), GUILayout.Height(20)))
            {
                ShowHelpWindow();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ShowHelpWindow()
        {
            EditorUtility.DisplayDialog(
                "Help - How to get Spreadsheet ID and Sheet IDs",
                "**Spreadsheet ID:**\n" +
                "1. Open your Google Sheet.\n" +
                "2. Copy the part of the URL after '/d/' and before '/edit'.\n" +
                "Example: https://docs.google.com/spreadsheets/d/SPREADSHEET_ID/edit\n\n" +
                "**Sheet IDs:**\n" +
                "1. Open your Google Sheet.\n" +
                "2. Click on the sheet tab you want to use.\n" +
                "3. Copy the 'gid' from the URL.\n" +
                "Example: https://docs.google.com/spreadsheets/d/SPREADSHEET_ID/edit#gid=SHEET_ID",
                "OK"
            );
        }

        private void DrawSpreadsheetIdField()
        {
            EditorGUILayout.LabelField("Google Spreadsheet", EditorStyles.boldLabel);

            GUIContent spreadsheetIdContent = new GUIContent(
                "Spreadsheet URL or ID",
                "To get the Spreadsheet ID:\n" +
                "1. Open your Google Sheet.\n" +
                "2. Copy the part of the URL after '/d/' and before '/edit'.\n" +
                "Example: https://docs.google.com/spreadsheets/d/SPREADSHEET_ID/edit"
            );
            string newSpreadsheetId = EditorGUILayout.TextField(spreadsheetIdContent, spreadsheetId);
            if (newSpreadsheetId != spreadsheetId)
            {
                spreadsheetId = newSpreadsheetId;
                if (spreadsheetId.Contains("docs.google.com"))
                {
                    spreadsheetId = ExtractSpreadsheetId(spreadsheetId);
                }
            }
        }

        private void DrawSheetIdsSection()
        {
            _showSheetIds = EditorGUILayout.Foldout(_showSheetIds, "Sheet IDs", true);
            if (_showSheetIds)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
                for (int i = 0; i < sheetIds.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    GUIContent sheetIdContent = new GUIContent(
                        $"Sheet ID {i + 1}:",
                        "To get the Sheet ID:\n" +
                        "1. Open your Google Sheet.\n" +
                        "2. Click on the sheet tab you want to use.\n" +
                        "3. Copy the 'gid' from the URL.\n" +
                        "Example: https://docs.google.com/spreadsheets/d/SPREADSHEET_ID/edit#gid=SHEET_ID"
                    );
                    sheetIds[i] = EditorGUILayout.TextField(sheetIdContent, sheetIds[i]);

                    if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    {
                        sheetIds.RemoveAt(i);
                        i--;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("Add Sheet ID"))
                {
                    sheetIds.Add("");
                }

                EditorGUILayout.EndVertical();
            }
        }


        private void DrawDownloadButton()
        {
            if (_isDownloading)
            {
                Rect rect = EditorGUILayout.BeginVertical();
                EditorGUI.ProgressBar(rect, _downloadProgress, "Downloading...");
                GUILayout.Space(18);
                EditorGUILayout.EndVertical();
            }
            else
            {
                if (GUILayout.Button("Download and Localize", GUILayout.Height(30)))
                {
                    SaveData();
                    CoroutineHandler.Instance.StartDownloadCoroutine(
                        DownloadAllSheetsAsTSV(CreateAllLocalizationsData));
                }
            }
        }

        private void DrawManualUploadAndHardResetSection()
        {
            _showAdvancedSettings = EditorGUILayout.Foldout(_showAdvancedSettings, "Advanced Settings", true);
            if (_showAdvancedSettings)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                GUIContent manualUploadContent = new GUIContent(
                    $"Enable Manual Upload:",
                    "In case you do not want to or cannot download from Google Sheets, you can do the manual localization:\n" +
                    "1. Export a .tsv and convert it to .csv.\n" +
                    "2. Drag the file to the Localizations folder.\n" +
                    "3. Format it as “Language-Localization.csv” e.g. “English-Localization.csv”.\n" +
                    "4. Click on “Update All Manually and Locally”.\n"
                );
                _manualUpload = EditorGUILayout.Toggle(manualUploadContent, _manualUpload);
                if (_manualUpload)
                {
                    if (GUILayout.Button("Update All Manually and Locally", GUILayout.Height(25)))
                    {
                        CreateAllLocalizationsData();
                    }
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Clear All and Start Over", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog(
                            "Clear All and Start Over",
                            "Are you sure you want to delete all localizations and files?\n" +
                            "This action cannot be undone.",
                            "Yes, Clear Everything",
                            "Cancel"))
                    {
                        HardReset();
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void HardReset()
        {
            string localizationManagerPath = FindLocalizationFolder();
            string localizationsPath = Path.Combine(localizationManagerPath, "Localizations");
            string resourcesPath = Path.Combine(localizationManagerPath, "Resources");

            if (Directory.Exists(localizationsPath))
            {
                foreach (var file in Directory.GetFiles(localizationsPath))
                {
                    File.Delete(file);
                }

                foreach (var metaFile in Directory.GetFiles(localizationsPath, "*.meta"))
                {
                    File.Delete(metaFile);
                }

                Debug.Log("Hard Reset: All CSV files in Localizations folder deleted.");
            }

            if (Directory.Exists(resourcesPath))
            {
                foreach (var file in Directory.GetFiles(resourcesPath))
                {
                    File.Delete(file);
                }

                foreach (var metaFile in Directory.GetFiles(resourcesPath, "*.meta"))
                {
                    File.Delete(metaFile);
                }

                Debug.Log("Hard Reset: All files in Resources folder deleted.");
            }

            PlayerPrefs.DeleteKey("Language");
            AssetDatabase.Refresh();
        }

        private void CreateAllLocalizationsData()
        {
            var allAssetsPaths = AssetDatabase.GetAllAssetPaths()
                .Where(x => x.Contains("Localizations"))
                .Where(x => x.EndsWith(".csv"))
                .ToArray();

            foreach (var path in allAssetsPaths)
                UpdateFileFrom(path);

            UpdateLanguagesAvailable(allAssetsPaths);

            if (_useDeepLIntegration && _deeplLanguages.Count > 0)
                CoroutineHandler.Instance.StartCoroutine(RunDeepLTranslations(allAssetsPaths));

            Debug.Log("All localizations created successfully");
        }


        private static void UpdateLanguagesAvailable(string[] allAssetsPaths)
        {
            var languages = new List<string>();

            foreach (var assetPath in allAssetsPaths)
            {
                var lines = File.ReadAllLines(assetPath);

                foreach (var line in lines)
                {
                    var columns = line.Split('\t');

                    if (columns.Length > 0 && columns[0].Trim().ToLower() == "language")
                    {
                        if (columns.Length > 1)
                        {
                            string languageName = columns[1].Trim();
                            if (!string.IsNullOrEmpty(languageName))
                                languages.Add(languageName);
                            break;
                        }
                    }
                }
            }

            foreach (var config in _deeplLanguages)
            {
                string readableName = DeepLTargetLanguageHelper.GetLanguageName(config.targetLanguage);
                if (!string.IsNullOrEmpty(readableName))
                    languages.Add(readableName);
            }

            var finalLanguages = languages.Distinct().ToList();

            string languagesAvailablePath = $"{FindLocalizationFolder()}/Resources/LanguagesAvailable.asset";
            LanguagesAvailable languagesAvailable =
                AssetDatabase.LoadAssetAtPath<LanguagesAvailable>(languagesAvailablePath);

            if (languagesAvailable == null)
            {
                languagesAvailable = CreateInstance<LanguagesAvailable>();
                AssetDatabase.CreateAsset(languagesAvailable, languagesAvailablePath);
            }

            languagesAvailable.languages = finalLanguages.ToArray();

            EditorUtility.SetDirty(languagesAvailable);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("LanguagesAvailable updated with: " + string.Join(", ", finalLanguages));
        }


        private void SaveData()
        {
            EditorPrefs.SetString(SPREADSHEET_ID_KEY, spreadsheetId);

            string sheetIdsString = string.Join(",", sheetIds);
            EditorPrefs.SetString(SHEET_IDS_KEY, sheetIdsString);
        }

        private void LoadData()
        {
            spreadsheetId = EditorPrefs.GetString(SPREADSHEET_ID_KEY, "");

            string sheetIdsString = EditorPrefs.GetString(SHEET_IDS_KEY, "");
            string[] ids = sheetIdsString.Split(new char[] {','}, System.StringSplitOptions.RemoveEmptyEntries);

            sheetIds.Clear();
            foreach (string id in ids)
            {
                sheetIds.Add(id.Trim());
            }
        }

        private static void UpdateFileFrom(string assetPath)
        {
            var lines = File.ReadAllLines(assetPath);

            var data = CreateInstance<LocalizationData>();

            var linesCount = lines.Length;
            var allLinesContent = new List<LocalizationContent>();

            for (var i = 1; i < linesCount; i++)
            {
                var lineContent = lines[i].Split('\t');
                var content = new LocalizationContent
                {
                    tid = lineContent[0],
                    text = lineContent[1],
                };
                allLinesContent.Add(content);
            }

            data.language = allLinesContent.First(l => l.tid.ToLower().Equals("language")).text;
            data.content = allLinesContent.ToArray();

            string normalizedLanguage = NormalizeFileName(data.language);

            string localizationManagerPath = FindLocalizationFolder();
            string assetFilePath = Path.Combine(localizationManagerPath,
                $"Resources/LocalizationData-{normalizedLanguage}.asset");
            string resourcesPath = Path.Combine(localizationManagerPath, "Resources");

            if (!Directory.Exists(resourcesPath))
            {
                Directory.CreateDirectory(resourcesPath);
            }

            AssetDatabase.CreateAsset(data, assetFilePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Localization for {data.language} created and saved in: {assetFilePath}");
        }

        private static string FindLocalizationFolder()
        {
            string[] guids = AssetDatabase.FindAssets("LocalizeProTool");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (AssetDatabase.IsValidFolder(path) && Path.GetFileName(path) == "LocalizeProTool")
                {
                    return path;
                }
            }

            return null;
        }

        IEnumerator DownloadAllSheetsAsTSV(Action onFinish)
        {
            _isDownloading = true;
            var sheetsIdsCopy = new List<string>(sheetIds);

            for (var index = 0; index < sheetsIdsCopy.Count; index++)
            {
                var sheetId = sheetsIdsCopy[index];
                string url = $"https://docs.google.com/spreadsheets/d/{spreadsheetId}/export?format=tsv&gid={sheetId}";

                _downloadProgress = (float) index / sheetsIdsCopy.Count;
                Repaint();

                using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                        webRequest.result == UnityWebRequest.Result.ProtocolError)
                    {
                        if (webRequest.error.Contains("401"))
                        {
                            Debug.LogError(
                                $"Error downloading sheet {sheetId}. 401 Unauthorized. Did you change the privacy of your Google Sheet to make it public? For that you have to click on “Share” and set that anyone can access it with the link.");
                        }
                        else
                        {
                            Debug.LogError("Error downloading sheet " + sheetId + ": " + webRequest.error);
                        }
                    }
                    else
                    {
                        string tsvData = webRequest.downloadHandler.text;
                        string fileName = GetFileNameFromTSV(tsvData);

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            SaveFile(fileName, tsvData);
                            Debug.Log(
                                $"Sheet {sheetId} | Language: {fileName} downloaded and saved in: {FindLocalizationFolder()}/Localizations/{fileName}-Localization.csv");
                        }
                        else
                        {
                            Debug.LogError("No row with 'language' found in sheet " + sheetId);
                        }
                    }
                }
            }

            Debug.Log("All sheets downloaded successfully");
            _isDownloading = false;
            _downloadProgress = 0f;
            Repaint();

            CoroutineHandler.Instance.Clear();
            onFinish();
        }

        string GetFileNameFromTSV(string tsvData)
        {
            string[] rows = tsvData.Split('\n');

            foreach (string row in rows)
            {
                string[] columns = row.Split('\t');

                if (columns.Length > 0 && columns[0].Trim().ToLower() == "language")
                {
                    if (columns.Length > 1)
                    {
                        return columns[1].Trim();
                    }
                }
            }

            return null;
        }

        private void SaveFile(string fileName, string tsvData)
        {
            string normalizedFileName = NormalizeFileName(fileName);

            string directoryPath = $"{FindLocalizationFolder()}/Localizations";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, $"{normalizedFileName}-Localization.csv");
            File.WriteAllText(filePath, tsvData);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        private static string NormalizeFileName(string fileName)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c.ToString(), "");
            }

            fileName = RemoveDiacritics(fileName);

            fileName = fileName.Replace(" ", "_");

            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = char.ToUpper(fileName[0]) + fileName.Substring(1).ToLower();
            }

            return fileName;
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

        private static string ExtractSpreadsheetId(string url)
        {
            if (string.IsNullOrEmpty(url) || !url.Contains("spreadsheets/d/"))
            {
                return null;
            }

            int startIndex = url.IndexOf("/d/") + 3;

            int endIndex = url.IndexOf('/', startIndex);
            if (endIndex == -1)
            {
                endIndex = url.IndexOf('?', startIndex);
            }

            if (endIndex == -1)
            {
                endIndex = url.Length;
            }

            string spreadsheetId = url.Substring(startIndex, endIndex - startIndex);

            return spreadsheetId;
        }

        private void DrawDeepLIntegrationSection()
        {
            EditorGUILayout.Space();
            _useDeepLIntegration = EditorGUILayout.ToggleLeft("DeepL Translate Integration", _useDeepLIntegration);
        
            if (_useDeepLIntegration)
            {
                var deeplSettings = Resources.Load<DeepLSettingsSO>("Translator/DeepLSettings");
                if (deeplSettings == null || string.IsNullOrEmpty(deeplSettings.apiKey))
                {
                    EditorGUILayout.HelpBox("DeepL API Key is not set in Translator/DeepLSettings asset.", MessageType.Error);
                    return;
                }
        
                EditorGUILayout.BeginVertical(GUI.skin.box);
        
                int? indexToRemove = null;
        
                for (int i = 0; i < _deeplLanguages.Count; i++)
                {
                    var lang = _deeplLanguages[i];
                    EditorGUILayout.BeginVertical(GUI.skin.window);
        
                    EditorGUILayout.BeginHorizontal();
        
                    var displayNames = Enum.GetValues(typeof(DeepLTargetLanguage))
                        .Cast<DeepLTargetLanguage>()
                        .Select(DeepLTargetLanguageHelper.GetLanguageName)
                        .ToArray();
        
                    var enumValues = Enum.GetValues(typeof(DeepLTargetLanguage)).Cast<DeepLTargetLanguage>().ToList();
                    int selected = enumValues.IndexOf(lang.targetLanguage);
                    int newSelected = EditorGUILayout.Popup(new GUIContent("Target Language", "Language to translate into."),
                        selected, displayNames);
                    lang.targetLanguage = enumValues[newSelected];
        
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        indexToRemove = i;
                    }
        
                    EditorGUILayout.EndHorizontal();
        
                    lang.showCustomOptions = EditorGUILayout.Foldout(lang.showCustomOptions, "Custom Options", true);
        
                    if (lang.showCustomOptions)
                    {
                        EditorGUI.BeginDisabledGroup(!lang.SupportsFormality);
                        int index = GetFormalityIndex(lang.formality);
                        int newIndex = EditorGUILayout.Popup(
                            new GUIContent("Formality",
                                "default: normal\nmore: more formal\nless: more informal\nprefer_more: prefer formal if supported\nprefer_less: prefer informal if supported"),
                            index,
                            new[] {"default", "more", "less", "prefer_more", "prefer_less"});
                        lang.formality = FormalityFromIndex(newIndex);
                        if (!lang.SupportsFormality)
                            EditorGUILayout.HelpBox("Formality is not available for this language.", MessageType.Info);
                        EditorGUI.EndDisabledGroup();
        
                        lang.preserveFormatting = EditorGUILayout.Toggle(
                            new GUIContent("Preserve Formatting",
                                "If enabled, the formatting of the original text will be preserved (e.g., punctuation, casing). Useful for UI text."),
                            lang.preserveFormatting);
        
                        int splitIndex = SplitIndex(lang.splitSentences);
                        int newSplit = EditorGUILayout.Popup(
                            new GUIContent("Split Sentences",
                                "0: No splitting, treat whole input as one sentence\n1: Split on punctuation and newlines\nnonewlines: Ignore newlines, split only on punctuation"),
                            splitIndex, new[] {"0", "1", "nonewlines"});
                        lang.splitSentences = new[] {"0", "1", "nonewlines"}[newSplit];
                    }
        
                    EditorGUILayout.EndVertical();
                }
        
                if (indexToRemove.HasValue)
                    _deeplLanguages.RemoveAt(indexToRemove.Value);
        
                if (GUILayout.Button("Add Language to Translate"))
                    _deeplLanguages.Add(new DeepLTranslationConfig());
        
                _forceDeepLTranslations = EditorGUILayout.ToggleLeft(
                    new GUIContent("Force Translations", 
                        "To avoid unnecessary API calls and stay within DeepL usage limits, the tool skips translation for any TID that already exists in the target language. Enable this option to force re-translation of all entries, even if they already exist."),
                    _forceDeepLTranslations);
                SaveDeepLConfigs();
                EditorGUILayout.EndVertical();
            }
            
            
        }

        private int GetFormalityIndex(string f)
        {
            return f switch
            {
                "more" => 1,
                "less" => 2,
                "prefer_more" => 3,
                "prefer_less" => 4,
                _ => 0,
            };
        }

        private string FormalityFromIndex(int index)
        {
            return index switch
            {
                1 => "more",
                2 => "less",
                3 => "prefer_more",
                4 => "prefer_less",
                _ => "default",
            };
        }

        private int SplitIndex(string s)
        {
            return s switch
            {
                "0" => 0,
                "nonewlines" => 2,
                _ => 1,
            };
        }

        private IEnumerator RunDeepLTranslations(string[] allAssetsPaths)
        {
            var translator = new DeepLTranslator();

            string firstCsvPath = allAssetsPaths.FirstOrDefault();
            if (string.IsNullOrEmpty(firstCsvPath)) yield break;

            _isTranslating = true;

            for (int i = 0; i < _deeplLanguages.Count; i++)
            {
                var config = _deeplLanguages[i];
                string targetLanguageName = DeepLTargetLanguageHelper.GetLanguageName(config.targetLanguage);

                _currentTranslationLanguage = targetLanguageName;
                _translationProgress = (float) i / _deeplLanguages.Count;
                Repaint();

                string normalizedLang = targetLanguageName;
                string targetAssetPath = Path.Combine(FindLocalizationFolder(),
                    $"Resources/LocalizationData-{normalizedLang}.asset");

                if (!_forceDeepLTranslations && File.Exists(targetAssetPath))
                {
                    Debug.Log($"Skipped {normalizedLang}, asset already exists.");
                    continue;
                }

                var lines = File.ReadAllLines(firstCsvPath)
                    .Skip(1)
                    .Select(l => l.Split('\t'))
                    .Where(c => c.Length >= 2 && !string.IsNullOrWhiteSpace(c[0]) &&
                                !string.Equals(c[0], "language", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                List<string> tids = new() {"language"};
                List<string> texts = new() {targetLanguageName};

                foreach (var line in lines)
                {
                    tids.Add(line[0]);
                    texts.Add(line[1]);
                }

                var options = new DeepLTranslationOptions(config.targetLanguage)
                {
                    Formality = config.formality,
                    PreserveFormatting = config.preserveFormatting,
                    SplitSentences = config.splitSentences
                };

                bool done = false;
                List<string> result = null;
                string error = null;

                yield return translator.TranslateText(texts, options,
                    onSuccess: r =>
                    {
                        result = r;
                        done = true;
                    },
                    onError: e =>
                    {
                        error = e;
                        done = true;
                    });

                while (!done) yield return null;

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"Failed to translate {config.targetLanguage}: {error}");
                    continue;
                }

                var data = ScriptableObject.CreateInstance<LocalizationData>();
                data.language = targetLanguageName;
                data.content = tids.Zip(result, (tid, txt) => new LocalizationContent {tid = tid, text = txt})
                    .ToArray();

                if (!Directory.Exists(Path.GetDirectoryName(targetAssetPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(targetAssetPath));

                AssetDatabase.CreateAsset(data, targetAssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"DeepL translation created for {targetLanguageName} in {targetAssetPath}");
            }

            _isTranslating = false;
            _translationProgress = 0;
            _currentTranslationLanguage = "";
            Repaint();

            AssetDatabase.Refresh();
        }


        private void SaveDeepLConfigs()
        {
            var wrapper = new DeepLTranslationConfigListWrapper { configs = _deeplLanguages };
            string json = JsonUtility.ToJson(wrapper);
            EditorPrefs.SetString(DeepLSavedPrefsKey, json);
        }

        private void LoadDeepLConfigs()
        {
            if (EditorPrefs.HasKey(DeepLSavedPrefsKey))
            {
                string json = EditorPrefs.GetString(DeepLSavedPrefsKey);
                var wrapper = JsonUtility.FromJson<DeepLTranslationConfigListWrapper>(json);
                if (wrapper != null && wrapper.configs != null)
                {
                    _deeplLanguages = wrapper.configs;
                }
            }
        }

    }


}