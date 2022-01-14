using Microsoft.Extensions.DependencyInjection;
using Alexey;
using Alexey.Modules;

var dependency = new Dependency();

var collection = dependency.Initialize();

var main = collection.GetService<MainModule>();
main.LongPoll();

