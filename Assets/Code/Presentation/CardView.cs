using System;
using Code.GameManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    internal sealed class CardView : MonoBehaviour
    {
        private event Action<BoardLocation> _clicked;

        [SerializeField] private Image _image;

        [SerializeField] private Button _button;

        private BoardLocation _location;

        private void Awake() =>
            _button.onClick.AddListener(OnClick);

        internal void Init(Sprite sprite, BoardLocation location, Action<BoardLocation> clicked)
        {
            _image.sprite = sprite;
            _image.enabled = true;
            _location = location;
            _clicked = clicked;
        }

        internal void SetVisible(bool value)
        {
            _image.enabled = value;
        }

        private void OnClick() =>
            _clicked?.Invoke(_location);
    }
}