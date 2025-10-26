using UnityEngine;
using UnityEngine.UI;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    internal sealed class Card : MonoBehaviour
    {
        [SerializeField]
        private Image _image;
        
        [SerializeField]
        private Button _button;

        internal void Init(Sprite sprite)
        {
            _image.sprite = sprite;
            _button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            Debug.Log("OnClick");
        }
    }
}
