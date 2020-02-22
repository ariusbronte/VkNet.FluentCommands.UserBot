using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.Model;

namespace VkNet.FluentCommands.UserBot.Storage
{
    internal class StickerCommandsStore : BaseStore<(long? peerId, long stickerId),
            IVkApi, Message, CancellationToken, Task>
    {
        public void Store((long? peerId, long stickerId) key,
            Func<IVkApi, Message, CancellationToken, Task> value)
        {
            if (key.stickerId <= 0) throw new ArgumentOutOfRangeException(nameof(key.stickerId));

            StoreValue(key, value);
        }

        public ConcurrentDictionary<(long? peerId, long stickerId),
            Func<IVkApi, Message, CancellationToken, Task>> Retrieve()
        {
            return RetrieveValues();
        }
        
        public void SetHandler(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));
            SetEventHandler(handler);
        }

        public async Task TriggerHandler(IVkApi botClient, Message message, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (botClient == null) throw new ArgumentNullException(nameof(botClient));
            if (message == null) throw new ArgumentNullException(nameof(message));
            await TriggerEventHandler(botClient, message, cancellationToken).ConfigureAwait(false);
        }
    }
}