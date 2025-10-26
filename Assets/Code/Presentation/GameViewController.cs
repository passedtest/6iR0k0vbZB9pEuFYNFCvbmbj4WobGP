using System;
using Code.GameManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    public sealed class GameViewController : MonoBehaviour
    {
        [SerializeField, Range(GameSession.MIN_ROWS, GameSession.MAX_ROWS)] 
        private byte _desiredRows = 5;
        
        [SerializeField, Range(GameSession.MIN_COLUMNS, GameSession.MAX_COLUMNS)] 
        private byte _desiredColumns = 6;
        
        [SerializeField] private Button _startStopButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;

        [SerializeField] private GameBoardView _gameBoardView;
        [SerializeField] private ScoreView _scoreView;

        [SerializeField] private CellsVisualData _visualData;

        [SerializeField] private CardView _cardViewPrefab;

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
            session.InputAccepted += OnInputAccepted;

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

        private void OnSessionSaved() =>
            _loadButton.interactable = true;

        private void OnCardClicked(BoardLocation boardLocation) =>
            _gameManager.CurrentGameSession.OnInput(boardLocation);

        private void OnStartStopButtonPressed()
        {
            if (_gameManager.CurrentGameSession == null)
                _gameManager.StartOrRestartGame(rows: _desiredRows, columns: _desiredColumns);
            else
                _gameManager.TryStopCurrentSession();
        }

        private void OnSaveButtonPressed() =>
            _gameManager.TrySaveCurrentSession();

        private void OnLoadButtonPressed() =>
            _gameManager.TryLoadSessionFromSaveData();

        private void OnSessionStarted()
        {
            _gameBoardView.OnSessionStarted();
        }

        private void OnTurnFinished(bool isMatch)
        {
            UpdateScoreView();
            _gameBoardView.OnTurnFinished(isMatch);
        }

        private void OnTurnStarted() =>
            _gameBoardView.OnTurnStarted();

        private void OnCurrentSessionCellResolved(BoardLocation boardLocation)
        {
            _gameBoardView.SetCardVisible(boardLocation, true);
            _gameBoardView.SetCardInteractable(boardLocation, false);
        }

        private void OnInputAccepted(BoardLocation boardLocation) =>
            _gameBoardView.HandleInput(boardLocation);

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
            // Time should be updated via reactive property.
            _scoreView.UpdateTime(_gameManager.CurrentGameSession.Time);
        }

        private void OnGUI() =>
            _presentationDrawer.OnGUI();
    }
}