using Core;
using Core.Interfaces;
using Core.Modules;
using Microsoft.Extensions.Configuration;
using VkNet;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model.RequestParams;

namespace Alexey.Modules
{
    public class MainModule
    {
        private VkApi _api;
        private Dictionary<long?, long?> _lastMsg = new();
        private IEnumerable<IMessageModule> _modules;
        private IConfiguration _config;

        public MainModule(VkApi api, IServiceProvider serviceProvider, ModulesCollection collection, IConfiguration config)
        {
            _api = api;
            _modules = collection.GetModules(serviceProvider);
            _config = config;
        }

        private void UpdateId(long? peerId, long? msgId)
        {
            if (peerId == null || msgId == null)
            {
                return;
            }
            if(!_lastMsg.ContainsKey(peerId))
            {
                _lastMsg.Add(peerId, msgId);
            }
            if(msgId > _lastMsg[peerId])
            {
                _lastMsg[peerId] = msgId;
            }
        }

        public void LongPoll()
        {
            var groupId = ulong.Parse(_config.GetSection("groupId").Value);
            var s = _api.Groups.GetLongPollServer(groupId);
            var prms = new BotsLongPollHistoryParams { Key = s.Key, Server = s.Server, Ts = s.Ts, Wait = 100 };
            while (true)
            {
                try
                {
                    prms.Key = s.Key;
                    prms.Ts = s.Ts;
                    prms.Server = s.Server;
                    var poll = _api.Groups.GetBotsLongPollHistory(prms);
                    foreach (var update in poll.Updates.Where(x => x.Type == GroupUpdateType.MessageNew)) // обработка всех новых сообщений
                    {
                        var message = update.MessageNew.Message;
                        if (!_lastMsg.ContainsKey(message.PeerId) || message.ConversationMessageId > _lastMsg[message.PeerId])
                        {
                            Console.WriteLine($"{message.PeerId} : {message.ConversationMessageId}");
                            foreach (var module in _modules) // обработка сообщения всеми модулями
                            {
                                try
                                {
                                    UpdateId(message.PeerId, module.ProcessMessage(message));
                                }
                                catch (LongPollKeyExpiredException)
                                {
                                    Console.WriteLine("Key expired, updating server");
                                    s = _api.Groups.GetLongPollServer(groupId);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                            }
                            UpdateId(message.PeerId, message.ConversationMessageId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
