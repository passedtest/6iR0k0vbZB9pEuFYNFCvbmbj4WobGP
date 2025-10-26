using System;
using Code.GameManagement;
using Code.State;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    internal sealed class Card : MonoBehaviour
    {
        private event Action<BoardLocation> _clicked;

        [SerializeField] private Image _image;

        [SerializeField] private Button _button;

        private BoardLocation _location;

        internal void Init(Sprite sprite, BoardLocation location, Action<BoardLocation> clicked)
        {
            _image.sprite = sprite;
            _location = location;
            _clicked = clicked;
            _button.onClick.AddListener(OnClick);
        }

        internal void UpdateState(BoardCellState state)
        {
            _image.enabled = !state.IsResolved;
        }

        private void OnClick() =>
            _clicked?.Invoke(_location);
    }
}