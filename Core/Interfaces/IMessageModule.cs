using VkNet.Model;

namespace Core.Interfaces
{
    public interface IMessageModule
    {
        long? ProcessMessage(Message message);
    }
}
