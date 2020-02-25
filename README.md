# VkNet.FluentCommands.UserBot
Extension for [VkNet](https://github.com/vknet/vk) to quickly create bots.

[![NuGet](https://img.shields.io/nuget/v/VkNet.FluentCommands.UserBot.svg)](https://www.nuget.org/packages/VkNet.FluentCommands.UserBot/)
[![NuGet](https://img.shields.io/nuget/dt/VkNet.FluentCommands.UserBot.svg)](https://www.nuget.org/packages/VkNet.FluentCommands.UserBot/)

## How to use?
### Add the package to the project
**Package Manager**
``` powershell
PM> Install-Package VkNet.FluentCommands.UserBot
```
**.NET CLI**
``` bash
> dotnet add package VkNet.FluentCommands.UserBot
```
``` C#
using VkNet.FluentCommands.UserBot;

//...

FluentUserBotCommands commands = new FluentUserBotCommands();
await commands.InitBotAsync("login", "very hard password");

commands.OnText("^ping$", "pong");
commands.OnText("^hello$", new[] {"hi!", "hey!", "good day!"});
commands.OnText("command not found");

commands.OnException((e, token) =>
{
    Console.WriteLine("Wake up, everything is broken");
    Console.WriteLine($"[{DateTime.UtcNow}] {e.Message} {Environment.NewLine} {e.StackTrace}");
    return Task.CompletedTask;
});

await commands.ReceiveMessageAsync();
```

See the [wiki](https://github.com/ariusbronte/VkNet.FluentCommands.UserBot/wiki) for all features.
