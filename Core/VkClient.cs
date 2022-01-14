using System.Collections.ObjectModel;
using System.Net;
using System.Text;
using VkNet;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace Core
{
    public class VkClient
    {
        private VkApi _api;
        private Random _random;
        private HttpClient _http;

        public VkClient(VkApi api)
        {
            _api = api;
            _random = new Random();
            _http = new HttpClient();
        }

        #region private
        private string ClearString(string str)
        {
            return str.Replace("[", "").Replace("\"", "").Replace("]", "");
        }

        private ReadOnlyCollection<Photo> GetPhotosByUrl(string url, UploadServerInfo uploadServer)
        {
            var wc = new WebClient();
            var photoUrl = ClearString(_http.GetAsync(url).Result.Content.ReadAsStringAsync().Result);
            wc.DownloadFile(photoUrl, $@"/temp/photo{uploadServer.UserId}.png");
            var responseImg = Encoding.ASCII.GetString(wc.UploadFile(uploadServer.UploadUrl, $@"/temp/photo{uploadServer.UserId}.png"));
            var photo = _api.Photo.SaveMessagesPhoto(responseImg);
            return photo;
        }

        private ReadOnlyCollection<Photo> GetPhotosByPath(string path, UploadServerInfo uploadServer)
        {
            var wc = new WebClient();
            var responseImg = Encoding.ASCII.GetString(wc.UploadFile(uploadServer.UploadUrl, path));
            var photo = _api.Photo.SaveMessagesPhoto(responseImg);
            return photo;
        }
        #endregion

        public long Send(string text, long? peerId, IEnumerable<MediaAttachment> attachments = null)
        {
            return _api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams { PeerId = peerId, RandomId = _random.NextInt64(), Message = text, Attachments = attachments });
        }

        public long Send(string text, Message message, IEnumerable<MediaAttachment> attachments = null)
        {
            if (message.PeerId == null)
            {
                return -1;
            }
            return Send(text, (long)message.PeerId, attachments);
        }

        public long Send(Message message)
        {
            return _api.Messages.Send(new VkNet.Model.RequestParams.MessagesSendParams { PeerId = message.PeerId, RandomId = _random.NextInt64(), Message = message.Text, Attachments = (IEnumerable<MediaAttachment>)message.Attachments });
        }

        public long SendPhoto(UploadServerInfo uploadServer, long? peerId, ReadOnlyCollection<Photo> photos, string message = "")
        {
            return Send(message, peerId, photos);
        }

        public long SendPhotoByUrl(UploadServerInfo uploadServer, long? peerId, string url)
        {
            return SendPhoto(uploadServer, peerId, GetPhotosByUrl(url, uploadServer));
        }

        public long SendPhotoByPath(UploadServerInfo uploadServer, long? peerId, string path)
        {
            return SendPhoto(uploadServer, peerId, GetPhotosByPath(path, uploadServer));
        }

        public UploadServerInfo GetUploadServer()
        {
            return _api.Photo.GetMessagesUploadServer(null);
        }
    }
}
