using Code.GameManagement;
using UnityEngine;

namespace Code.Presentation
{
    /// <summary>
    /// NOTE: this temporary code to test the basic logic, and board initialization. Will be removed later.
    /// </summary>
    internal sealed class PrototypePresentationDrawer
    {
        private readonly GameManager _gameManager;
        private byte[] _currentSavedSessionBlob;

        public PrototypePresentationDrawer(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        internal void OnGUI()
        {
            if (_gameManager.CurrentGameSession != null)
            {
                if (GUILayout.Button("Save"))
                {
                    _currentSavedSessionBlob = _gameManager.CurrentGameSession.Serialize();
                }
            }

            if (_currentSavedSessionBlob != null && GUILayout.Button("Load"))
            {
                _gameManager.StartOrRestartGame(_currentSavedSessionBlob);
            }

            if (_gameManager.CurrentGameSession == null)
            {
                if (GUILayout.Button("Start"))
                {
                    _gameManager.StartOrRestartGame();
                }
            }
            else
            {
                if (GUILayout.Button("Stop"))
                {
                    _gameManager.TryStopCurrentSession();
                    return;
                }

                GUILayout.BeginHorizontal();
                for (var column = 0; column < _gameManager.CurrentGameSession.Columns; column++)
                {
                    GUILayout.BeginVertical();
                    for (var row = 0; row < _gameManager.CurrentGameSession.Rows; row++)
                    {
                        var state = _gameManager.CurrentGameSession.GetState(row, column);
                        if (state.IsResolved)
                            GUILayout.Button("#");
                        else
                        {
                            if (GUILayout.Button(state.Type.ToString()))
                                _gameManager.CurrentGameSession.OnInput(new BoardLocation(row, column));
                        }
                    }

                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}