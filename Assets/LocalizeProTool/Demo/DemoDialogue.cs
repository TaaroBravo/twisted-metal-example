using LocalizeProTool.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LocalizeProTool.Demo
{
    public class DemoDialogue : MonoBehaviour
    {
        public string[] dialogueTIDs;
        public TextMeshProUGUI dialogue;
        public Button continueButton;
        public GameObject container;

        private int _currentIndex;
    
        public void Show()
        {
            container.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(Continue);
            _currentIndex = 0;
            SetDialogueText();
        }

        private void Continue()
        {
            _currentIndex++;
            if (_currentIndex >= dialogueTIDs.Length)
            {
                container.SetActive(false);
                _currentIndex = 0;
            }
            SetDialogueText();

        }

        private void SetDialogueText()
        {
            dialogue.text = LocalizePro.Instance.GetTextFor(dialogueTIDs[_currentIndex]);
        }
    }
}