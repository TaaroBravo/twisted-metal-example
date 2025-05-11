using UnityEngine;
using UnityEngine.UI;

namespace LocalizeProTool.Demo
{
    public class DemoDialogueSystem : MonoBehaviour
    {
        public Button firstDialogueButton;
        public Button secondDialogueButton;
        public DemoDialogue firstDialogue;
        public DemoDialogue secondDialogue;

        private void Start()
        {
            firstDialogueButton.onClick.AddListener(() => firstDialogue.Show());
            secondDialogueButton.onClick.AddListener(() => secondDialogue.Show());
        }
    }
}