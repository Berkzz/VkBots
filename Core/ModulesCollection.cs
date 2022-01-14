using Core.Attributes;
using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Modules
{
    public class ModulesCollection
    {
        public IEnumerable<IMessageModule> GetModules(IServiceProvider provider)
        {
            foreach (Type type in Assembly.GetEntryAssembly().GetTypes().Where(x => x.GetCustomAttributes<MessageModuleAttribute>().Any()))
            {
                yield return (IMessageModule)ActivatorUtilities.CreateInstance(provider, type, new object[0]);
            }
        }
    }
}
