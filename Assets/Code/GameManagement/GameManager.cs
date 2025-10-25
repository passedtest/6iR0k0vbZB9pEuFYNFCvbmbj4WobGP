namespace Code.GameManagement
{
    /// <summary>
    /// Will represent a high level manager, which will be responsible for the <see cref="GameSession"/> lifetime.
    /// <seealso cref="GameSession"/>
    /// </summary>
    public sealed class GameManager
    {
        public GameSession CurrentGameSession { get; private set; }

        public void StartOrRestartGame()
        {
            CurrentGameSession = new GameSession(rows: 5, columns: 6);
        }

        public void StartOrRestartGame(byte[] blob)
        {
            CurrentGameSession = new GameSession(blob);
        }

        public void StopGame()
        {
            CurrentGameSession = null;
        }
    }
}