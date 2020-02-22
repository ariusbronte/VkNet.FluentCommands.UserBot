using System;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.FluentCommands.UserBot.Abstractions;
using VkNet.Model;

namespace VkNet.FluentCommands.UserBot.Storage
{
    internal class BotExceptionEventStore : BaseEventStore<IVkApi, Message, System.Exception, CancellationToken, Task>
    {
        public void SetHandler(Func<IVkApi, Message, System.Exception, CancellationToken, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            SetEventHandler(handler);
        }

        public async Task TriggerHandler(MessageToProcess messageToProcess, System.Exception exception, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (messageToProcess.BotClient == null) throw new ArgumentNullException(nameof(messageToProcess.BotClient));
            if (messageToProcess.Message == null) throw new ArgumentNullException(nameof(messageToProcess.Message));
            if (exception == null) throw new ArgumentNullException(nameof(exception));
            await TriggerEventHandler(messageToProcess.BotClient, messageToProcess.Message, exception, cancellationToken).ConfigureAwait(false);
        }
    }
}