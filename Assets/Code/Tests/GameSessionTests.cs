using Code.GameManagement;
using NUnit.Framework;

namespace Code.Tests
{
    public sealed class GameSessionTests
    {
        [Test]
        public void SerializationTest()
        {
            var originalGameSession = new GameSession(rows: 5, columns: 6);
            originalGameSession.GetState(row: 2, column: 2).IsResolved = true;
            originalGameSession.GetState(row: 4, column: 1).IsResolved = true;

            var blob = originalGameSession.Serialize();
            var newGameSession = new GameSession(blob);

            Assert.AreEqual(originalGameSession.Rows, newGameSession.Rows);
            Assert.AreEqual(originalGameSession.Columns, newGameSession.Columns);

            for (var rowIndex = 0; rowIndex < originalGameSession.Rows; rowIndex++)
            {
                for (var columnIndex = 0; columnIndex < originalGameSession.Columns; columnIndex++)
                {
                    ref var originalState = ref originalGameSession.GetState(rowIndex, columnIndex);
                    ref var newState = ref newGameSession.GetState(rowIndex, columnIndex);

                    Assert.AreEqual(originalState.Type, newState.Type);
                    Assert.AreEqual(originalState.IsResolved, newState.IsResolved);
                }
            }
        }
    }
}