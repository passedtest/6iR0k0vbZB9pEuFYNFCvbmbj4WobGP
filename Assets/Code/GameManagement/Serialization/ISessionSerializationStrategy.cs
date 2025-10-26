namespace Code.GameManagement.Serialization
{
    public interface ISessionSerializationStrategy
    {
        byte[] Serialize(GameSession session);
        GameSessionState Deserialize(byte[] data);
    }
}