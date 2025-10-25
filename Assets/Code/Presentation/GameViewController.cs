using System;
using Code.GameManagement;
using UnityEngine;

namespace Code.Presentation
{
    [DisallowMultipleComponent]
    public sealed class GameViewController : MonoBehaviour
    {
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
            _presentationDrawer = new PrototypePresentationDrawer(_gameManager);
        }

        private void OnGUI()
        {
            _presentationDrawer.OnGUI();
        }
    }
}