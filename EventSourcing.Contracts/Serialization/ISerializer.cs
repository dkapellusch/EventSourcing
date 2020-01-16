using Google.Protobuf;

namespace EventSourcing.Contracts.Serialization
{
    public interface ISerializer
    {
        T Deserialize<T>(byte[] serializedData);

        byte[] Serialize<T>(T dataToSerialize);
    }

    public interface IMessageSerializer<T> : ISerializer<T> where T : IMessage<T>, new()
    {
    }

    public interface ISerializer<T>
    {
        T Deserialize(byte[] serializedData);

        byte[] Serialize(T dataToSerialize);
    }
}