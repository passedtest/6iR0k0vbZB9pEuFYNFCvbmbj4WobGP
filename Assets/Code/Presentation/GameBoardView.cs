using System;
using Code.GameManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    public sealed class GameBoardView : GridLayoutGroup
    {
        public event Action<BoardLocation> CardClicked = delegate { };

        [SerializeField] private int _rows;

        [SerializeField] private int _columns;

        [SerializeField] private RectTransform _rectTransform;

        private GameSession _gameSession;
        private CardPool _cardPool;
        private CellsVisualData _visualData;
        private CardView[,] _cardInstances;

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();
        }

        internal void Initialize(CardView cardViewPrefab, CellsVisualData cellsVisualData)
        {
            _cardPool ??= new CardPool(cardViewPrefab, transform);
            _visualData = cellsVisualData;
        }

        internal void InitializeSession(GameSession session)
        {
            if (_gameSession != null)
                throw new InvalidOperationException("View is already initialized. Call 'Release' before initializing it again.");

            _columns = session.Columns;
            constraint = Constraint.FixedRowCount;
            constraintCount = session.Rows;

            _gameSession = session;
            _cardInstances = new CardView[_rows, _columns];

            for (var row = 0; row < session.Rows; row++)
            {
                for (var column = 0; column < session.Columns; column++)
                {
                    var state = session.GetState(row, column);
                    _cardInstances[row, column] = _cardPool.Get();
                    _cardInstances[row, column].Init(_visualData.CellVisualData[state.Type], new BoardLocation(row, column), OnCardClicked);
                }
            }
        }

        private void OnCardClicked(BoardLocation boardLocation) =>
            CardClicked(boardLocation);

        internal void TryReleaseSession()
        {
            if (_gameSession == null)
                return;

            _gameSession = null;
            foreach (var card in _cardInstances)
                _cardPool.Release(card);
            _cardInstances = null;
        }

        internal void UpdateCardState(BoardLocation boardLocation)
        {
            ref var card = ref _cardInstances[boardLocation.Row, boardLocation.Column];
            ref var state = ref _gameSession.GetState(boardLocation.Row, boardLocation.Column);

            card.UpdateState(state);
        }

        public override void SetLayoutVertical()
        {
            var width = _rectTransform.rect.width - padding.horizontal - (_columns - 1) * spacing.x;
            var cellWidth = width / _columns;
            cellSize = new Vector2(cellWidth, cellWidth);

            base.SetLayoutVertical();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            constraint = Constraint.FixedRowCount;
            constraintCount = _rows;
            _rectTransform = GetComponent<RectTransform>();
        }
#endif
    }
}