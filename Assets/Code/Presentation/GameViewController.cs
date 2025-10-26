using System;
using Code.GameManagement;
using UnityEngine;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    public sealed class GameViewController : MonoBehaviour
    {
        [SerializeField] private GameBoardView _gameBoardView;

        [SerializeField] private CellsVisualData _visualData;

        [SerializeField] private Card _cardPrefab;

        private GameManager _gameManager;
        private PrototypePresentationDrawer _presentationDrawer;

        /// <summary>
        /// Will be called from bootstrap
        /// </summary>
        public void Initialize(GameManager gameManager)
        {
            if (_gameManager != null)
                throw new InvalidOperationException($"{nameof(GameViewController)} is already initialized.");

            _gameManager = gameManager;
            _gameManager.SessionStarted += OnSessionStarted;
            _gameManager.SessionStopped += OnSessionStopped;

            _gameBoardView.Initialize(_cardPrefab, _visualData);
            _gameBoardView.CardClicked += OnCardClicked;

            _presentationDrawer = new PrototypePresentationDrawer(_gameManager);
        }

        private void OnSessionStarted()
        {
            var session = _gameManager.CurrentGameSession;
            session.TurnFinished += OnTurnFinished;
            session.CellResolved += OnCurrentSessionCellResolved;

            _gameBoardView.InitializeSession(session);
        }

        private void OnTurnFinished()
        {
        }

        private void OnCurrentSessionCellResolved(BoardLocation boardLocation)
        {
            _gameBoardView.UpdateCardState(boardLocation);
        }

        private void OnSessionStopped()
        {
            _gameBoardView.TryReleaseSession();
        }

        private void OnCardClicked(BoardLocation boardLocation)
        {
            _gameManager.CurrentGameSession.OnInput(boardLocation);
        }

        private void OnGUI() =>
            _presentationDrawer.OnGUI();
    }
}