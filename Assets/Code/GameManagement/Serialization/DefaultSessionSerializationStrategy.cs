using System;
using System.IO;
using Code.State;

namespace Code.GameManagement.Serialization
{
    /// <summary>
    /// A default (most straightforward) serialization strategy implementation for the <see cref="GameSession"/>.
    /// There are some optimization opportunities here.
    /// </summary>
    public sealed class DefaultSessionSerializationStrategy : ISessionSerializationStrategy
    {
        public unsafe byte[] Serialize(GameSession session)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);

            writer.Write(session.Turns);
            writer.Write(session.Matches);
            writer.Write(session.Combo);
            writer.Write(session.Time);
            writer.Write(session.Rows);
            writer.Write(session.Columns);

            for (var row = 0; row < session.Rows; row++)
            {
                for (var column = 0; column < session.Columns; column++)
                {
                    // TODO: Binary serialize the entire column at once (instead of per-element Write() calls).
                    ref var state = ref session.GetState(row, column);
                    fixed (BoardCellState* ptr = &state)
                    {
                        var memSpan = new ReadOnlySpan<byte>(ptr, length: sizeof(BoardCellState));
                        writer.Write(memSpan);
                    }
                }
            }

            return memoryStream.ToArray();
        }

        public unsafe GameSessionState Deserialize(byte[] data)
        {
            using var memoryStream = new MemoryStream(data);
            using var reader = new BinaryReader(memoryStream);

            var turns = reader.ReadInt32();
            var matches = reader.ReadInt32();
            var combo = reader.ReadInt32();
            var time = reader.ReadSingle();
            var rows = reader.ReadInt32();
            var columns = reader.ReadInt32();

            var internalState = new BoardCellState[rows, columns];

            for (var row = 0; row < rows; row++)
            {
                for (var column = 0; column < columns; column++)
                {
                    // TODO: Should probably read to span (bit better performance).
                    var serializedState = reader.ReadBytes(count: sizeof(BoardCellState));
                    fixed (byte* ptr = serializedState)
                    {
                        ref var state = ref internalState[row, column];
                        state = *(BoardCellState*)ptr;
                    }
                }
            }

            var boardState = new BoardState(internalState);
            return new GameSessionState(turns, matches, combo, time, boardState);
        }
    }
}