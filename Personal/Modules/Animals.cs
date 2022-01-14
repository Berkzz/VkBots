using Core;
using Core.Attributes;
using Core.Interfaces;
using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace Personal.Modules
{
    [MessageModule]
    public class Animals : IMessageModule
    {
        private VkClient _client;

        const string ShibeUrl = "https://shibe.online/api/shibes?count=1&urls=true";
        const string CatUrl = "https://shibe.online/api/cats?count=1&urls=true";
        const string BirdUrl = "https://shibe.online/api/birds?count=1&urls=true";

        private Dictionary<string, string> _commands = new()
        {
            { "собака", ShibeUrl },
            { "котик", CatUrl },
            { "птичка", BirdUrl }
        };

        public Animals(VkApi api, VkClient client)
        {
            _client = client;
        }

        public long? ProcessMessage(Message message)
        {
            if (!_commands.ContainsKey(message.Text.ToLower()))
            {
                return null;
            }

            if (!Directory.Exists("/temp"))
            {
                Directory.CreateDirectory("/temp");
            }

            var uploadServer = _client.GetUploadServer();

            return _client.SendPhotoByUrl(uploadServer, message.PeerId, _commands[message.Text.ToLower()]);
        }
    }
}
