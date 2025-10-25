using System;
using System.IO;

namespace Code.State.Serialization
{
    /// <summary>
    /// A default (most straightforward) serialization strategy implementation for the <see cref="BoardState"/>.
    /// There are some optimization opportunities here.
    /// </summary>
    public sealed class DefaultBoardSerializationStrategy : IBoardSerializationStrategy
    {
        public unsafe byte[] Serialize(BoardState board)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new BinaryWriter(memoryStream);

            writer.Write(board.Rows);
            writer.Write(board.Columns);

            for (var row = 0; row < board.Rows; row++)
            {
                for (var column = 0; column < board.Columns; column++)
                {
                    // TODO: Binary serialize the entire column at once (instead of per-element Write() calls).
                    ref var state = ref board.GetStateRef(row, column);
                    fixed (BoardCellState* ptr = &state)
                    {
                        var memSpan = new ReadOnlySpan<byte>(ptr, length: sizeof(BoardCellState));
                        writer.Write(memSpan);
                    }
                }
            }

            return memoryStream.ToArray();
        }

        public unsafe BoardState Deserialize(byte[] data)
        {
            using var memoryStream = new MemoryStream(data);
            using var reader = new BinaryReader(memoryStream);

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

            return new BoardState(internalState);
        }
    }
}