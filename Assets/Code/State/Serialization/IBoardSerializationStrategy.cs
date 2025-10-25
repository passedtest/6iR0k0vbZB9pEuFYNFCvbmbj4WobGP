namespace Code.State.Serialization
{
    public interface IBoardSerializationStrategy
    {
        byte[] Serialize(BoardState board);
        BoardState Deserialize(byte[] data);
    }
}