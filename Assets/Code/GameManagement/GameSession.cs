using System;
using System.Collections.Generic;
using Code.State;
using Code.State.Serialization;
using UnityEngine;

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
        private static readonly System.Random _randomSource = new();

        /// <summary>
        /// Declaring the global serialization strategy.
        /// NOTE that serialization strategy has to be stateless.
        /// </summary>
        private static readonly IBoardSerializationStrategy serializationStrategy =
            new DefaultBoardSerializationStrategy();

        /// <summary>
        /// Current board rows.
        /// </summary>
        public int Rows => _boardState.Rows;

        /// <summary>
        /// Current board columns.
        /// </summary>
        public int Columns => _boardState.Columns;

        private readonly BoardState _boardState;
        private BoardLocation? _currentSelectedLocation;

        /// <summary>
        /// Initialize a new game session with explicit rows/columns arguments.
        /// </summary>
        internal GameSession(int rows, int columns)
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

        /// <summary>
        /// Initialize a new game session from the binary blob using <see cref="IBoardSerializationStrategy"/> implementation.
        /// </summary>
        internal GameSession(byte[] bytes) =>
            _boardState = serializationStrategy.Deserialize(bytes);

        /// <summary>
        /// This basically represent the user input from the UI.
        /// </summary>
        /// <param name="externalInput"></param>
        public void OnInput(in BoardLocation externalInput)
        {
            if (_currentSelectedLocation == null)
            {
                // Handle 1st move input.
                _currentSelectedLocation = externalInput;
            }
            else
            {
                if (_currentSelectedLocation.Value.Equals(externalInput))
                {
                    // This is just a sanity check. This should never happen as UI should handle interactivity.
                    Debug.LogError($"Unable to select the location of '{externalInput}' as its already selected.");
                }
                else
                {
                    // Now, when the second location received, we could check if cards are matched.
                    var input0 = _currentSelectedLocation.Value;
                    var input1 = externalInput;

                    ref var state0 = ref GetState(input0.Row, input0.Column);
                    ref var state1 = ref GetState(input1.Row, input1.Column);

                    // Check if matched.
                    if (state0.Type == state1.Type)
                    {
                        // Match!

                        // Mark state as resolved.
                        state0.IsResolved = true;
                        state1.IsResolved = true;

                        // TODO: implement OnCellResolved event.
                        // TODO: implement score system.

                        Debug.Log("ITS A MATCH");
                    }
                    else
                    {
                        Debug.LogError("NOT A MATCH :(");
                    }

                    _currentSelectedLocation = null;
                }
            }
        }

        public ref BoardCellState GetState(int row, int column) =>
            ref _boardState.GetStateRef(row, column);

        /// <summary>
        /// Saves the session sate as binary blob, using see <see cref="IBoardSerializationStrategy"/> implmenetation.
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize() =>
            serializationStrategy.Serialize(_boardState);
    }
}