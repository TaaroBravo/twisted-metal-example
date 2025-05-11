using LocalizeProTool.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace LocalizeProTool.Demo
{
    public class DemoLanguageChanger : MonoBehaviour
    {
        public Button leftButton;
        public Button rightButton;

        private LanguagesAvailable _languagesAvailable;
        private int _currentLanguageIndex = 0;

        private void Start()
        {
            _languagesAvailable = Resources.Load<LanguagesAvailable>("LanguagesAvailable");

            if (_languagesAvailable == null || _languagesAvailable.languages == null || _languagesAvailable.languages.Length == 0)
            {
                Debug.LogError("No languages found in LanguagesAvailable. Make sure it exists and has languages.");
                return;
            }

            leftButton.onClick.AddListener(LeftPressed);
            rightButton.onClick.AddListener(RightPressed);

            UpdateLanguageDisplay();
        }

        private void LeftPressed()
        {
            _currentLanguageIndex--;
            if (_currentLanguageIndex < 0)
            {
                _currentLanguageIndex = _languagesAvailable.languages.Length - 1;
            }

            UpdateLanguageDisplay();
        }

        private void RightPressed()
        {
            _currentLanguageIndex++;
            if (_currentLanguageIndex >= _languagesAvailable.languages.Length)
            {
                _currentLanguageIndex = 0; 
            }

            UpdateLanguageDisplay();
        }

        private void UpdateLanguageDisplay()
        {
            string currentLanguage = _languagesAvailable.languages[_currentLanguageIndex];

            LocalizePro.Instance.SetLanguage(currentLanguage);
        }
    }
}