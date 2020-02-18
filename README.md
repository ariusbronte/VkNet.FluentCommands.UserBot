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

await commands.InitBotAsync(new ApiAuthParams
{
    Login = "login",
    Password = "very hard password"
});

commands.OnText("^ping$", async (api, message, token) =>
{
    await api.Messages.SendAsync(new MessagesSendParams
    {
        PeerId = message.PeerId,
        Message = "pong",
        RandomId = random.Next(int.MinValue, int.MaxValue)
    });
});

await commands.ReceiveMessageAsync();
```
## Regular expression configuration
``` C#
commands.OnText(("^ping$", RegexOptions.IgnoreCase), async (api, update, token) => {});
```

## Bot exception handler
``` C#
commands.OnBotException(async (api, update, e, token) => {});
```

## Library exception handler
``` C#
commands.OnException((e, token) =>
{
    Console.WriteLine(e.Message);
    return Task.CompletedTask;
});
```
