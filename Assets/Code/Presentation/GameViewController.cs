using System;
using System.Collections.Generic;
using Code.GameManagement;
using UnityEngine;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    public sealed class GameViewController : MonoBehaviour
    {
        private struct DelayedDisabledCard
        {
            internal BoardLocation Location;
            internal float Time;
        }

        [SerializeField] private GameBoardView _gameBoardView;

        [SerializeField] private CellsVisualData _visualData;

        [SerializeField] private CardView _cardViewPrefab;

        private GameManager _gameManager;
        private PrototypePresentationDrawer _presentationDrawer;
        private readonly Queue<BoardLocation> _currentTurnClickedCards = new();
        private readonly List<DelayedDisabledCard> _delayedDisabledCards = new();

        /// <summary>
        /// Will be called from bootstrap
        /// </summary>
        public void Initialize(GameManager gameManager)
        {
            if (_gameManager != null)
                throw new InvalidOperationException($"{nameof(GameViewController)} is already initialized.");

            _gameManager = gameManager;
            _gameManager.SessionInitialized += OnSessionInitialized;
            _gameManager.SessionReleased += OnSessionReleased;

            _gameBoardView.Initialize(_cardViewPrefab, _visualData);
            _gameBoardView.CardClicked += OnCardClicked;

            _presentationDrawer = new PrototypePresentationDrawer(_gameManager);
        }

        private void OnSessionInitialized()
        {
            var session = _gameManager.CurrentGameSession;
            session.Started += OnSessionStarted;
            session.TurnStarted += OnTurnStarted;
            session.TurnFinished += OnTurnFinished;
            session.CellResolved += OnCurrentSessionCellResolved;

            _gameBoardView.InitializeSession(session);
        }

        private void OnSessionStarted()
        {
            _gameBoardView.UpdateView();
        }

        private void OnTurnFinished()
        {
            while (_currentTurnClickedCards.TryDequeue(out var boardLocation))
            {
                var state = _gameManager.CurrentGameSession.GetState(boardLocation.Row, boardLocation.Column);
                if (!state.IsResolved)
                    _delayedDisabledCards.Add(new DelayedDisabledCard() { Location = boardLocation, Time = 1f });
            }
        }

        private void OnTurnStarted()
        {
            foreach (var delayedDisabledCard in _delayedDisabledCards)
            {
                _gameBoardView.SetVisible(delayedDisabledCard.Location, false);
            }

            _delayedDisabledCards.Clear();
        }

        private void Update()
        {
            for (var i = _delayedDisabledCards.Count - 1; i >= 0; i--)
            {
                var delayedDisabledCard = _delayedDisabledCards[i];
                delayedDisabledCard.Time -= Time.deltaTime;
                if (delayedDisabledCard.Time <= 0)
                {
                    _gameBoardView.SetVisible(delayedDisabledCard.Location, false);
                    _delayedDisabledCards.RemoveAt(i);
                }
                else
                {
                    _delayedDisabledCards[i] = delayedDisabledCard;
                }
            }
        }

        private void OnCurrentSessionCellResolved(BoardLocation boardLocation)
        {
            _gameBoardView.SetVisible(boardLocation, true);
        }

        private void OnSessionReleased()
        {
            _gameBoardView.TryReleaseSession();
        }

        private void OnCardClicked(BoardLocation boardLocation)
        {
            _currentTurnClickedCards.Enqueue(boardLocation);
            _gameBoardView.SetVisible(boardLocation, true);
            _gameManager.CurrentGameSession.OnInput(boardLocation);
        }

        private void OnGUI() =>
            _presentationDrawer.OnGUI();
    }
}