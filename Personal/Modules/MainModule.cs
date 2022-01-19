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
            while (true)
            {
                try
                {
                    var poll = _api.Messages.GetLongPollHistory(new MessagesGetLongPollHistoryParams { MaxMsgId = _lastMsgId, Pts = s.Pts, Ts = ulong.Parse(s.Ts) });
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
                                Console.WriteLine();
                                Console.WriteLine(ex);
                                s = _api.Messages.GetLongPollServer(needPts: true);
                            }
                        }
                        UpdateId(message.Id);
                        if (poll.Messages.Count() > 10)
                        {
                            s = _api.Messages.GetLongPollServer(needPts: true);
                            Console.WriteLine("Updating server");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine(ex);
                }
                Thread.Sleep(200);
            }
        }
    }
}
