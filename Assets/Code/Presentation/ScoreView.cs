using UnityEngine;
using UnityEngine.UI;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ScoreView : MonoBehaviour
    {
        [SerializeField] private Text _turnsText;
        [SerializeField] private Text _matchesText;
        [SerializeField] private Text _timeText;

        private string _turnsTextPattern;
        private string _matchesTextPattern;
        private string _timeTextPattern;

        private void Awake()
        {
            _turnsTextPattern = _turnsText.text;
            _matchesTextPattern = _matchesText.text;
            _timeTextPattern = _timeText.text;
        }

        internal void UpdateTurnsCount(int value) =>
            _turnsText.text = string.Format(_turnsTextPattern, value);

        internal void UpdateMatchesCount(int value) =>
            _matchesText.text = string.Format(_matchesTextPattern, value);

        internal void UpdateTime(float value) =>
            _timeText.text = string.Format(_timeTextPattern, value.ToString(format: "f0"));
    }
}