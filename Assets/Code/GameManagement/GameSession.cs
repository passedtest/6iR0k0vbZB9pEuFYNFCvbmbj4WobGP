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
        /// Invokes when used turn is finished.
        /// </summary>
        public event Action TurnFinished = delegate { };

        /// <summary>
        /// Invokes when the specific cell was resolved.
        /// </summary>
        public event Action<BoardLocation> CellResolved = delegate { };

        /// <summary>
        /// Current board rows.
        /// </summary>
        public int Rows => _boardState.Rows;

        /// <summary>
        /// Current board columns.
        /// </summary>
        public int Columns => _boardState.Columns;

        /// <summary>
        /// An amount of turns user has been done so far.
        /// </summary>
        public int Turns { get; private set; }

        /// <summary>
        /// Amount of matches.
        /// </summary>
        public int Matches { get; private set; }

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

            // Shuffle all the card states.
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
        internal GameSession(byte[] bytes)
        {
            // TODO: Should also serialize turns and matches.
            _boardState = serializationStrategy.Deserialize(bytes);
        }

        /// <summary>
        /// Initialize a new game session with an explicit state.
        /// </summary>
        internal GameSession(int turns, int matches, BoardState boardState)
        {
            Turns = turns;
            Matches = matches;
            _boardState = boardState;
        }

        /// <summary>
        /// This basically represent the user input from the UI.
        /// </summary>
        /// <param name="inputLocation"></param>
        public void OnInput(in BoardLocation inputLocation)
        {
            if (_currentSelectedLocation == null)
            {
                // Handle 1st move input.
                _currentSelectedLocation = inputLocation;
            }
            else
            {
                if (_currentSelectedLocation.Value.Equals(inputLocation))
                {
                    // This is just a sanity check. This should never happen as UI should handle interactivity.
                    Debug.LogError($"Unable to select the location of '{inputLocation}' as its already selected.");
                }
                else
                {
                    Turns++;

                    // Now, when the second location received, we could check if cards are matched.
                    var location0 = _currentSelectedLocation.Value;
                    var location1 = inputLocation;

                    ref var state0 = ref GetState(location0.Row, location0.Column);
                    ref var state1 = ref GetState(location1.Row, location1.Column);

                    // Reset the current selected location before next move.
                    _currentSelectedLocation = null;

                    // Check if matched.
                    if (state0.Type == state1.Type)
                    {
                        // Match!
                        Matches++;

                        // Mark state as resolved, and invoke the events.
                        state0.IsResolved = true;
                        CellResolved(location0);

                        state1.IsResolved = true;
                        CellResolved(location1);

                        Debug.Log("ITS A MATCH");

                        if (Matches == _boardState.Columns * _boardState.Rows / 2)
                        {
                            Debug.Log("GAME WON");
                        }
                    }
                    else
                    {
                        Debug.LogError("NOT A MATCH :(");
                    }

                    // State was changed (not necessary a match).
                    TurnFinished();
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