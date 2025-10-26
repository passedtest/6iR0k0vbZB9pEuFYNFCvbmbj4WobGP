using UnityEngine;
using UnityEngine.UI;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ScoreView : MonoBehaviour
    {
        [SerializeField] private Text _turnsText;
        [SerializeField] private Text _matchesText;

        private string _turnsTextPattern;
        private string _matchesTextPattern;

        private void Awake()
        {
            _turnsTextPattern = _turnsText.text;
            _matchesTextPattern = _matchesText.text;
        }

        internal void UpdateTurnsCount(int value) =>
            _turnsText.text = string.Format(_turnsTextPattern, value);

        internal void UpdateMatchesCount(int value) =>
            _matchesText.text = string.Format(_matchesTextPattern, value);
    }
}