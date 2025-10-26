using Code.State;

namespace Code.GameManagement.Serialization
{
    public struct GameSessionState
    {
        public readonly int Turns;
        public readonly int Matches;
        public readonly float Time;
        public readonly BoardState BoardState;

        internal GameSessionState(int turns, int matches, float time, BoardState boardState)
        {
            Turns = turns;
            Matches = matches;
            Time = time;
            BoardState = boardState;
        }
    }
}