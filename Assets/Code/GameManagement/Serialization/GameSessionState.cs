using Code.State;

namespace Code.GameManagement.Serialization
{
    public struct GameSessionState
    {
        public readonly int Turns;
        public readonly int Matches;
        public readonly int Combo;
        public readonly float Time;
        public readonly BoardState BoardState;

        internal GameSessionState(int turns, int matches, int combo, float time, BoardState boardState)
        {
            Turns = turns;
            Matches = matches;
            Combo = combo;
            Time = time;
            BoardState = boardState;
        }
    }
}