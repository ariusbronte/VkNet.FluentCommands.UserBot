using System;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.FluentCommands.UserBot.Abstractions;
using VkNet.Model;

namespace VkNet.FluentCommands.UserBot.Storage
{
    internal class ChatPinMessageEventStore : BaseEventStore<IVkApi, Message, CancellationToken, Task>
    {
        public void SetHandler(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            SetEventHandler(handler);
        }

        public async Task TriggerHandler(MessageToProcess messageToProcess, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (messageToProcess.BotClient == null) throw new ArgumentNullException(nameof(messageToProcess.BotClient));
            if (messageToProcess.Message == null) throw new ArgumentNullException(nameof(Message));
            await TriggerEventHandler(messageToProcess.BotClient, messageToProcess.Message, cancellationToken).ConfigureAwait(false);
        }
    }
}