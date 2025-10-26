using System;
using Code.GameManagement;
using UnityEngine;
using UnityEngine.Pool;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    public sealed class GameViewController : MonoBehaviour
    {
        [SerializeField]
        private GameFieldInitializer _gameFieldInitializer;
        
        [SerializeField]
        private CellsVisualData _visualData;
        
        [SerializeField]
        private Card _cardPrefab;
        
        private GameManager _gameManager;
        private PrototypePresentationDrawer _presentationDrawer;
        private ObjectPool<Card> _cardPool;

        /// <summary>
        /// Will be called from bootstrap
        /// </summary>
        public void Initialize(GameManager gameManager)
        {
            if (_gameManager != null)
                throw new InvalidOperationException($"{nameof(GameViewController)} is already initialized.");

            _gameManager = gameManager;
            _gameManager.SessionStarted += OnSessionStarted;
            _presentationDrawer = new PrototypePresentationDrawer(_gameManager);
            _cardPool = new ObjectPool<Card>(CreateCard, actionOnRelease: OnCardRelease, actionOnGet: OnGetCard, defaultCapacity: 15);
        }

        private void OnCardRelease(Card card)
        {
            card.transform.SetParent(null, worldPositionStays: false);
            card.gameObject.SetActive(false);
        }

        private Card CreateCard()
        {
            var card = Instantiate(_cardPrefab);
            card.gameObject.SetActive(false);
            return card;
        }
        
        private void OnGetCard(Card card)
        {
            card.transform.SetParent(_gameFieldInitializer.transform, worldPositionStays: false);
            card.gameObject.SetActive(true);
        }
        
        private void OnSessionStarted()
        {
            var session = _gameManager.CurrentGameSession;
            foreach (var card in _gameFieldInitializer.GetComponentsInChildren<Card>())
            {
                _cardPool.Release(card);
            }
            
            _gameFieldInitializer.Init(rows: session.Rows, columns: session.Columns);

            for (var row = 0; row < session.Rows; row++)
            {
                for (var column = 0; column < session.Columns; column++)
                {
                    var state = session.GetState(row, column);
                    var c = _cardPool.Get();
                    c.Init(_visualData.CellVisualData[state.Type]);
                }
            }
        }

        private void OnGUI()
        {
            _presentationDrawer.OnGUI();
        }
    }
}