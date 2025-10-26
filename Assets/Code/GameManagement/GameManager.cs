using System;

namespace Code.GameManagement
{
    /// <summary>
    /// Will represent a high level manager, which will be responsible for the <see cref="GameSession"/> lifetime.
    /// <seealso cref="GameSession"/>
    /// </summary>
    public sealed class GameManager
    {
        private const float DEFAULT_START_TIMEOUT = 3f;
        
        public event Action SessionInitialized = delegate { };
        public event Action SessionReleased = delegate { };
        public event Action SessionSaved = delegate { };
        
        public GameSession CurrentGameSession { get; private set; }
        
        private byte[] _lastSessionSerializedData;

        public void StartOrRestartGame(int rows, int columns)
        {
            TryStopCurrentSession();
            CurrentGameSession = new GameSession(rows, columns, DEFAULT_START_TIMEOUT);
            SessionInitialized();
        }

        public void StartOrRestartGame(byte[] blob)
        {
            TryStopCurrentSession();
            CurrentGameSession = new GameSession(blob, DEFAULT_START_TIMEOUT);
            SessionInitialized();
        }

        public void TrySaveCurrentSession()
        {
            if (CurrentGameSession == null) 
                return;
            
            _lastSessionSerializedData = CurrentGameSession.Serialize();
            SessionSaved();
        }
        
        public void TryLoadSessionFromSaveData()
        {
            if (_lastSessionSerializedData != null)
                StartOrRestartGame(_lastSessionSerializedData);
        }

        public void TryStopCurrentSession()
        {
            if (CurrentGameSession == null)
                return;

            CurrentGameSession = null;
            SessionReleased();
        }

        public void Update(float deltaTime) =>
            CurrentGameSession?.Update(deltaTime);
    }
}