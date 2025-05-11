namespace LocalizeProTool.Scripts.DeepL
{
    public class DeepLTranslationOptions
    {
        public DeepLTargetLanguage TargetLanguage;
        public DeepLTargetLanguage? SourceLanguage = null;

        public string Formality = "default"; // default, more, less, prefer_more, prefer_less
        public bool PreserveFormatting = false;
        public string SplitSentences = "1"; // "0", "1", "nonewlines"

        public DeepLTranslationOptions(DeepLTargetLanguage target)
        {
            TargetLanguage = target;
        }
    }
}