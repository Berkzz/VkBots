using Microsoft.Extensions.DependencyInjection;
using Personal;
using Core;

var dependency = new Dependency();

var collection = dependency.Initialize();

var main = collection.GetService<MainModule>();
main.LongPoll();

