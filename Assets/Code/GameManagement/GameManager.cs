using System;

namespace Code.GameManagement
{
    /// <summary>
    /// Will represent a high level manager, which will be responsible for the <see cref="GameSession"/> lifetime.
    /// <seealso cref="GameSession"/>
    /// </summary>
    public sealed class GameManager
    {
        public event Action SessionStarted = delegate { };
        public event Action SessionStopped = delegate { };

        public GameSession CurrentGameSession { get; private set; }

        public void StartOrRestartGame()
        {
            TryStopCurrentSession();
            CurrentGameSession = new GameSession(rows: 5, columns: 6);
            SessionStarted();
        }

        public void StartOrRestartGame(byte[] blob)
        {
            TryStopCurrentSession();
            CurrentGameSession = new GameSession(blob);
            SessionStarted();
        }

        public void TryStopCurrentSession()
        {
            if (CurrentGameSession == null) 
                return;
            
            CurrentGameSession = null;
            SessionStopped();
        }
    }
}