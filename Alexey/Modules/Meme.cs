using Core;
using Core.Attributes;
using Core.Interfaces;
using SixLabors.ImageSharp.Processing;
using VkNet.Model;

using Sl = SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace Alexey.Modules
{
    [MessageModule]
    public class Meme : IMessageModule
    {
        private VkClient _client;
        private Random _random;
        private string[] _allFiles;
        private int _maxLines;

        public Meme(VkClient client, IConfiguration config)
        {
            _client = client;
            _random = new Random();
            Directory.CreateDirectory("Conversations");
            Directory.CreateDirectory("Temp");
            _allFiles = Directory.GetFiles("Pictures");
            _maxLines = int.Parse(config.GetSection("maxLines").Value);
        }
        
        private Sl.Image GetRandomImage()
        {
            return Sl.Image.Load(_allFiles[_random.Next(0, _allFiles.Length)]);
        }

        private void CreateFile(long? peerId)
        {
            File.Create($"Conversations/{peerId}.txt").Close();
        }

        private bool FileExists(long? peerId)
        {
            return File.Exists($"Conversations/{peerId}.txt");
        }

        private string GetRandomPhrase(long? peerId)
        {
            if(!FileExists(peerId))
            {
                CreateFile(peerId);
                return "нихуя нет епти!";
            }
            var lines = File.ReadLines($"Conversations/{peerId}.txt").Where(x => !string.IsNullOrEmpty(x));
            var count = lines.Count();
            var line = _random.Next(0, count);
            return lines.Skip(line - 1).Take(1).First();
        }

        private string AddTextToImage(Sl.Image img, string text, long? peerId)
        {
            using (var image = img)
            {
                var fonts = new FontCollection();
                var font = fonts.Install("Fonts/tnr.ttf").CreateFont(17);
                var options = new TextOptions { WrapTextWidth = image.Width - 10 };
                image.Mutate(x => x.SetTextOptions(options).DrawText(text, font, Color.Black, new PointF(10, 10)));
                image.SaveAsPng($"Temp/{peerId}.png");
                return $"Temp/{peerId}.png";
            }
        }

        private void TryAppendLine(long? peerId, string text)
        {
            var path = $"Conversations/{peerId}.txt";
            var lines = File.ReadLines(path).Where(x => !string.IsNullOrEmpty(x));
            if (lines.Count() >= _maxLines)
            {
                File.WriteAllLines(path, lines.Skip(1).ToArray());
            }
            File.AppendAllLines(path, new[] { text });
        }

        public long? ProcessMessage(Message message)
        {
            if (message.Text == null)
            {
                return 0;
            }
            if(message.TextEquals("гаф"))
            {
                return _client.SendPhotoByPath(_client.GetUploadServer(), message.PeerId, AddTextToImage(GetRandomImage(), GetRandomPhrase(message.PeerId), message.PeerId));
            } else
            {
                if(!FileExists(message.PeerId))
                {
                    CreateFile(message.PeerId);
                }
                TryAppendLine(message.PeerId, message.Text);
                return 0;
            }
        }
    }
}
