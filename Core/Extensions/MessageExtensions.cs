using VkNet.Model;

namespace Core.Extensions
{
    public static class MessageExtensions
    {
        public static bool TextEquals(this Message msg, string str)
        {
            return msg.Text.ToLower().Equals(str.ToLower());
        }
    }
}
