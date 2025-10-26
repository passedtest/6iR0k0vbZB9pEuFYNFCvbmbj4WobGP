using System;
using System.Collections.Generic;
using Code.GameManagement;
using UnityEngine;
using UnityEngine.UI;

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

        [SerializeField] private Button _startStopButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;

        [SerializeField] private GameBoardView _gameBoardView;
        [SerializeField] private ScoreView _scoreView;

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
            _gameManager.SessionSaved += OnSessionSaved;

            _gameBoardView.Initialize(_cardViewPrefab, _visualData);
            _gameBoardView.CardClicked += OnCardClicked;

            _startStopButton.onClick.AddListener(OnStartStopButtonPressed);
            _saveButton.onClick.AddListener(OnSaveButtonPressed);
            _saveButton.interactable = false;

            _loadButton.onClick.AddListener(OnLoadButtonPressed);
            _loadButton.interactable = false;

            _presentationDrawer = new PrototypePresentationDrawer(_gameManager);

            UpdateStartStopButtonText();
            UpdateSaveButtonState();
            UpdateScoreViewVisibility();
        }

        private void OnStartStopButtonPressed()
        {
            if (_gameManager.CurrentGameSession == null)
                _gameManager.StartOrRestartGame(rows: 5, columns: 6);
            else
                _gameManager.TryStopCurrentSession();
        }

        private void OnSaveButtonPressed() =>
            _gameManager.TrySaveCurrentSession();

        private void OnLoadButtonPressed() =>
            _gameManager.TryLoadSessionFromSaveData();

        private void OnSessionSaved() =>
            _loadButton.interactable = true;

        private void OnSessionInitialized()
        {
            UpdateStartStopButtonText();
            UpdateSaveButtonState();
            UpdateScoreViewVisibility();

            var session = _gameManager.CurrentGameSession;
            session.Started += OnSessionStarted;
            session.TurnStarted += OnTurnStarted;
            session.TurnFinished += OnTurnFinished;
            session.CellResolved += OnCurrentSessionCellResolved;

            _gameBoardView.InitializeSession(session);
            UpdateScoreView();
        }

        private void OnSessionReleased()
        {
            UpdateStartStopButtonText();
            UpdateSaveButtonState();
            UpdateScoreViewVisibility();

            _gameBoardView.TryReleaseSession();
        }

        private void OnSessionStarted()
        {
            _gameBoardView.OnSessionStarted();
        }

        private void OnTurnFinished(bool isMatch)
        {
            UpdateScoreView();

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
                _gameBoardView.SetVisible(delayedDisabledCard.Location, false);

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
            _gameBoardView.SetInteractable(boardLocation, false);
        }

        private void UpdateStartStopButtonText() =>
            _startStopButton.GetComponentInChildren<Text>().text =
                _gameManager.CurrentGameSession == null ? "Start" : "Stop";

        private void UpdateSaveButtonState() =>
            _saveButton.interactable = _gameManager.CurrentGameSession != null;

        private void UpdateScoreViewVisibility() =>
            _scoreView.gameObject.SetActive(_gameManager.CurrentGameSession != null);

        private void UpdateScoreView()
        {
            _scoreView.UpdateTurnsCount(_gameManager.CurrentGameSession.Turns);
            _scoreView.UpdateMatchesCount(_gameManager.CurrentGameSession.Matches);
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