using TMPro;
using UnityEngine;

namespace LocalizeProTool.Scripts
{
    /// <summary>
    /// Class TID_TextMeshProUGUI allows you to add it as a component to a TextMeshPro and automatically locate the texts.
    /// </summary>
    public class TID_TextMeshProUGUI : MonoBehaviour
    {
        [SerializeField] private string tid;

        private TextMeshProUGUI _textMeshPro;
    
        private void Start()
        {
            AssignTextMeshPro();
            _textMeshPro.text = LocalizePro.Instance.GetTextFor(tid);
        }

        /// <summary>
        /// Force to set new text to TextMeshPro with current TID
        /// </summary>
        /// <param name="newTid"></param>
        public void ForceUpdate()
        {
            if (!_textMeshPro)
                AssignTextMeshPro();
            _textMeshPro.text = LocalizePro.Instance.GetTextFor(tid);
        }

        /// <summary>
        /// Force to set new text to TextMeshPro with a new TID
        /// </summary>
        /// <param name="newTid"></param>
        public void ForceUpdate(string newTid)
        {
            if (!_textMeshPro)
                AssignTextMeshPro();
            _textMeshPro.text = LocalizePro.Instance.GetTextFor(newTid);
        }

        private void AssignTextMeshPro()
        {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
        }
    }
}