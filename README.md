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

await commands.ReceiveMessageAsync();
```
``` C#
commands.OnSticker("sticker triggered");
commands.OnSticker(163, "orejas triggered");
commands.OnPhoto("photo triggered");
commands.OnVoice("voice triggered");
commands.OnReply("reply triggered");
commands.OnReply("^ping$", "pong"); 
commands.OnForward("forward triggered");
```
## Extended logic
``` C# 
commands.OnText("^ping$", async (api, message, token) =>
{
    await api.Messages.SendAsync(new MessagesSendParams
    {
        PeerId = message.PeerId,
        Message = "pong",
        RandomId = random.Next(int.MinValue, int.MaxValue)
    });
});
```
*this applies to all triggers
## Regular expression configuration
``` C#
commands.OnText(("^ping$", RegexOptions.IgnoreCase), async (api, update, token) => {});
commands.OnText(("^ping$", RegexOptions.IgnoreCase), "pong");
```
*this applies to all triggers
## Individual logic
``` C#
commands.OnText((2_000_000_000 + 1, "^ping$", RegexOptions.IgnoreCase), "pong1");
commands.OnText((2_000_000_000 + 2, "^ping$"), async (api, update, token) => {});
```
*this applies to all triggers
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
## Custom configurations 
``` C#
commands.ConfigureUserLongPoll(new UserLongPollConfiguration { });
// the configuration has a convenient property that allows you to configure the incoming message filter
// if you want to process only sent messages, set the value MessageType.Sended
// (convenient if you use the bot as an extension of the basic VK function)
commands.ConfigureUserLongPoll(new UserLongPollConfiguration
{
    MessageType = MessageType.Sended
});
```
