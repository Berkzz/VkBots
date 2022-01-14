using Core;
using Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.Model;

namespace Personal
{
    public class Dependency
    {
        public IServiceProvider Initialize()
        {
            var serviceCollection = new ServiceCollection();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("config.json", optional: true, reloadOnChange: true);

            var config = builder.Build();

            var api = new VkApi();

            api.Authorize(new ApiAuthParams
            {
                AccessToken = config.GetSection("token").Value
            });

            serviceCollection.AddSingleton<IConfiguration>(config);
            serviceCollection.AddSingleton<MainModule>();
            serviceCollection.AddSingleton(api);
            serviceCollection.AddSingleton<ModulesCollection>();
            serviceCollection.AddSingleton<VkClient>();
            serviceCollection.AddSingleton<IServiceProvider>(serviceCollection.BuildServiceProvider());

            return serviceCollection.BuildServiceProvider();
        }
    }
}
