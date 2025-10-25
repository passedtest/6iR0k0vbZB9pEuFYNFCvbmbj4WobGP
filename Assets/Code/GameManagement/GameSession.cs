using System;
using System.Collections.Generic;
using Code.State;

namespace Code.GameManagement
{
    /// <summary>
    /// Will represent a current ongoing game session.
    /// </summary>
    public sealed class GameSession
    {
        // NOTE: The following constants are declared based on .pdf document.
        private const int MIN_ROWS = 2;
        private const int MIN_COLUMNS = 2;

        private const int MAX_ROWS = 5;
        private const int MAX_COLUMNS = 6;

        /// <summary>
        /// Declaring the global random state.
        /// Should be reconsidered if it will be required to restore the session by the seed.
        /// </summary>
        private static readonly Random _randomSource = new();

        /// <summary>
        /// Current board rows.
        /// </summary>
        public int Rows => _boardState.Rows;

        /// <summary>
        /// Current board columns.
        /// </summary>
        public int Columns => _boardState.Columns;

        private readonly BoardState _boardState;

        public GameSession(int rows, int columns)
        {
            if (rows < MIN_ROWS || rows > MAX_ROWS || columns < MIN_COLUMNS || columns > MAX_COLUMNS)
                throw new IndexOutOfRangeException(
                    $"Invalid board data r:{rows} c:{columns}. Only the following range is allowed r:[{MIN_ROWS}, {MAX_ROWS}] and c:[{MIN_COLUMNS}, {MAX_COLUMNS}].");

            var totalStates = rows * columns;
            if (totalStates % 2 != 0)
                throw new ArgumentException(
                    "The total number of board cells (rows * cols) must be even (pairing required");

            var internalState = new BoardCellState[rows, columns];
            var numberOfPairs = totalStates / 2;

            // Here we store all the card states linearly.
            var allValues = new List<byte>();

            // Iterate of the unique card types count [0 to numberOfPairs - 1] and add to the collection of the sates. Each cart type will appear twice.
            for (byte stateTypeIndex = 0; stateTypeIndex < numberOfPairs; stateTypeIndex++)
            {
                // Add pair to the list.
                allValues.Add(stateTypeIndex);
                allValues.Add(stateTypeIndex);
            }

            // Shuffle all the car states.
            var cnt = allValues.Count;
            while (cnt > 1)
            {
                cnt--;
                var rndIndex = _randomSource.Next(maxValue: cnt + 1);
                (allValues[rndIndex], allValues[cnt]) = (allValues[cnt], allValues[rndIndex]);
            }

            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    // Calculate the flat (1d) index.
                    var linearIndex = row * columns + column;

                    // Initialize state.
                    internalState[row, column] = new BoardCellState(allValues[linearIndex]);
                }
            }

            _boardState = new BoardState(internalState);
        }

        public ref BoardCellState GetState(int row, int column) =>
            ref _boardState.GetStateRef(row, column);
    }
}