namespace Code.State
{
    public struct BoardCellState
    {
        /// <summary>
        /// A type of the cell. The requirement is to support 5x6 layout at max.
        /// meaning that we should have at least 15 unique cards.
        /// This index will be also used by presentation logic to pick the corresponded sprite.
        /// </summary>
        public readonly byte Type;

        /// <summary>
        /// Mutable state - indicates that the cell was resolved.
        /// </summary>
        public bool IsResolved;

        public BoardCellState(byte type)
        {
            Type = type;
            IsResolved = false;
        }
    }
}