using Core;
using Core.Interfaces;
using Core.Modules;
using VkNet;
using VkNet.Model.RequestParams;

namespace Personal
{
    public class MainModule
    {
        private VkApi _api;
        private long? _lastMsgId = 0;
        private IEnumerable<IMessageModule> _modules;

        public MainModule(VkApi api, IServiceProvider serviceProvider, ModulesCollection collection, VkClient client)
        {
            _api = api;
            _modules = collection.GetModules(serviceProvider);
        }

        private void UpdateId(long? id)
        {
            if (id != null && id > _lastMsgId)
            {
                _lastMsgId = id;
            }
        }

        public void LongPoll()
        {
            var s = _api.Messages.GetLongPollServer(needPts: true);
            var ts = ulong.Parse(s.Ts);
            var prms = new MessagesGetLongPollHistoryParams { MaxMsgId = _lastMsgId, Pts = s.Pts, Ts = ts, MsgsLimit = 200 };
            while (true)
            {
                try
                {
                    prms.MaxMsgId = _lastMsgId;
                    prms.Ts = ulong.Parse(s.Ts);
                    prms.Pts = s.Pts;
                    var poll = _api.Messages.GetLongPollHistory(prms);
                    foreach (var message in poll.Messages.Where(x => x != null && x.Id > _lastMsgId)) // обработка всех новых сообщений
                    {
                        Console.WriteLine(message.Id);
                        foreach (var module in _modules) // обработка сообщения всеми модулями
                        {
                            try
                            {
                                UpdateId(module.ProcessMessage(message));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                        UpdateId(message.Id);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                Thread.Sleep(500);
            }
        }
    }
}
