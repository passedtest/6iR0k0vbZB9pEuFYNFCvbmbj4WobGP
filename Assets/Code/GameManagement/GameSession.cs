using System;
using System.Collections.Generic;
using Code.GameManagement.Serialization;
using Code.State;
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
        private static readonly ISessionSerializationStrategy serializationStrategy =
            new DefaultSessionSerializationStrategy();

        /// <summary>
        /// Called when the game is started.
        /// </summary>
        public event Action Started = delegate { };

        /// <summary>
        /// Invokes when used turn is started.
        /// </summary>
        public event Action TurnStarted = delegate { };

        /// <summary>
        /// Invokes when used turn is finished.
        /// Bool represents whether it was a match or not.
        /// </summary>
        public event Action<bool> TurnFinished = delegate { };

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

        /// <summary>
        /// A total session time in seconds.
        /// </summary>
        public float Time { get; private set; }

        public bool AllowToMakeTurn => Mathf.Approximately(_startTimout, 0f);

        private float _startTimout;

        private readonly BoardState _boardState;
        private BoardLocation? _currentSelectedLocation;

        /// <summary>
        /// Initialize a new game session with explicit rows/columns arguments.
        /// </summary>
        internal GameSession(int rows, int columns, float startTimout = 0f)
        {
            if (rows < MIN_ROWS || rows > MAX_ROWS || columns < MIN_COLUMNS || columns > MAX_COLUMNS)
                throw new IndexOutOfRangeException(
                    $"Invalid board data r:{rows} c:{columns}. Only the following range is allowed r:[{MIN_ROWS}, {MAX_ROWS}] and c:[{MIN_COLUMNS}, {MAX_COLUMNS}].");

            var totalStates = rows * columns;
            if (totalStates % 2 != 0)
                throw new ArgumentException(
                    "The total number of board cells (rows * cols) must be even (pairing required");

            _startTimout = Mathf.Max(startTimout, 0f);

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
        /// Initialize a new game session from the binary blob using <see cref="ISessionSerializationStrategy"/> implementation.
        /// </summary>
        internal GameSession(byte[] bytes, float startTimout = 0f)
        {
            _startTimout = Mathf.Max(startTimout, 0f);

            var sessionState = serializationStrategy.Deserialize(bytes);
            Turns = sessionState.Turns;
            Matches = sessionState.Matches;
            Time = sessionState.Time;
            _boardState = sessionState.BoardState;
        }

        /// <summary>
        /// This basically represent the user input from the UI.
        /// </summary>
        /// <param name="inputLocation"></param>
        public void OnInput(in BoardLocation inputLocation)
        {
            if (!AllowToMakeTurn)
            {
                Debug.LogError($"Unable to make a move yet, current timeout is '{_startTimout}'.");
                return;
            }

            ref var desiredState = ref GetState(inputLocation.Row, inputLocation.Column);
            if (desiredState.IsResolved)
            {
                Debug.LogError($"Unable to select the location of '{inputLocation}' as its already resolved.");
                return;
            }

            if (_currentSelectedLocation == null)
            {
                // Handle 1st move input.
                _currentSelectedLocation = inputLocation;
                TurnStarted();
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

                    var isMatch = state0.Type == state1.Type;
                    // Check if matched.
                    if (isMatch)
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
                    TurnFinished(isMatch);
                }
            }
        }

        internal void Update(float deltaTime)
        {
            if (_startTimout > 0)
            {
                _startTimout -= deltaTime;
                if (_startTimout <= 0)
                {
                    _startTimout = 0;
                    Started();
                }
            }
            else
            {
                Time += deltaTime;
            }
        }

        public ref BoardCellState GetState(int row, int column) =>
            ref _boardState.GetStateRef(row, column);

        /// <summary>
        /// Saves the session sate as binary blob, using see <see cref="ISessionSerializationStrategy"/> implmenetation.
        /// </summary>
        /// <returns></returns>
        public byte[] Serialize() =>
            serializationStrategy.Serialize(session: this);
    }
}