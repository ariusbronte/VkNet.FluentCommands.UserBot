using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;

// ReSharper disable MemberCanBeProtected.Global

namespace VkNet.FluentCommands.UserBot
{
    /// <inheritdoc />
    public class FluentUserBotCommands : FluentUserBotCommands<IVkApi>
    {
        /// <summary>
        ///      Initializes a new instance of the <see cref="FluentUserBotCommands"/> class.
        /// </summary>
        public FluentUserBotCommands() : base(botClient: () =>
        {
            var services = new ServiceCollection();
            services.AddAudioBypass();
            return new VkApi(services);
        }) { }
    }

    public class FluentUserBotCommands<TBotClient> : IFluentUserBotCommands where TBotClient : IVkApi
    {
        /// <summary>
        ///     Implementation of interaction with VK.
        /// </summary>
        private readonly TBotClient _botClient;

        /// <summary>
        ///    Long poll configuration
        /// </summary>
        private UserLongPollConfiguration _longPollConfiguration = new UserLongPollConfiguration();

        /// <summary>
        ///     Text commands storage.
        /// </summary>
        private readonly ConcurrentDictionary<(long?, string, RegexOptions), Func<IVkApi, Message, CancellationToken, Task>>
            _textCommands = new ConcurrentDictionary<(long?, string, RegexOptions), Func<IVkApi, Message, CancellationToken, Task>>();

        /// <summary>
        ///     Stores the message logic exception handler
        /// </summary>
        private Func<IVkApi, Message, System.Exception, CancellationToken, Task> _botException;

        /// <summary>
        ///     Stores the library exception handler.
        /// </summary>
        private Func<System.Exception, CancellationToken, Task> _exception;

        /// <summary>
        ///      Initializes a new instance of the <see cref="FluentUserBotCommands{TBotClient}"/> class.
        /// </summary>
        /// <param name="botClient">Implementation of interaction with VK.</param>
        public FluentUserBotCommands(Func<TBotClient> botClient)
        {
            _botClient = botClient();
        }

        /// <inheritdoc />
        public async Task InitBotAsync(IApiAuthParams apiAuthParams)
        {
            if (apiAuthParams == null)
            {
                throw new ArgumentNullException(paramName: nameof(apiAuthParams));
            }

            await _botClient.AuthorizeAsync(@params: apiAuthParams);
        }

        /// <inheritdoc />
        public void ConfigureUserLongPoll(UserLongPollConfiguration configuration)
        {
            _longPollConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc />
        public void OnText(string pattern, Func<IVkApi, Message, CancellationToken, Task> func)
        {
            OnText(tuple: (pattern, RegexOptions.None), func: func);
        }
        
        /// <inheritdoc />
        public void OnText((string pattern, RegexOptions options) tuple, Func<IVkApi, Message, CancellationToken, Task> func)
        {
            OnTextHandler(tuple: (null, tuple.pattern, tuple.options), func: func);
        }
        
        /// <inheritdoc />
        public void OnText((long peerId, string pattern) tuple, Func<IVkApi, Message, CancellationToken, Task> func)
        {
            OnText(tuple: (tuple.peerId, tuple.pattern, RegexOptions.None), func: func);
        }

        /// <inheritdoc />
        public void OnText((long peerId, string pattern, RegexOptions options) tuple, Func<IVkApi, Message, CancellationToken, Task> func)
        {
            if (tuple.peerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tuple.peerId));
            }
            
            OnTextHandler(tuple: tuple, func: func);
        }
        
        /// <summary>
        ///     The main handler for all incoming message triggers
        /// </summary>
        /// <param name="tuple">Regular expression and Regex options.</param>
        /// <param name="func">Trigger actions performed.</param>
        /// <exception cref="ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="ArgumentNullException">Thrown if trigger actions in null.</exception>
        private void OnTextHandler((long? peerId, string pattern, RegexOptions options) tuple, Func<IVkApi, Message, CancellationToken, Task> func)
        {
            if (string.IsNullOrWhiteSpace(value: tuple.pattern))
            {
                throw new ArgumentException(message: "Value cannot be null or whitespace.", paramName: nameof(tuple.pattern));
            }

            if (!Enum.IsDefined(enumType: typeof(RegexOptions), value: tuple.options))
            {
                throw new InvalidEnumArgumentException(argumentName: nameof(tuple.options), invalidValue: (int) tuple.options, enumClass: typeof(RegexOptions));
            }

            if (func == null)
            {
                throw new ArgumentNullException(paramName: nameof(func));
            }

            _textCommands.TryAdd(key: (tuple.peerId, tuple.pattern, tuple.options), value: func);
        }

        /// <inheritdoc />
        public void OnBotException(Func<IVkApi, Message, System.Exception, CancellationToken, Task> botException)
        {
            _botException = botException ?? throw new ArgumentNullException(nameof(botException));
        }

        /// <inheritdoc />
        public void OnException(Func<System.Exception, CancellationToken, Task> exception)
        {
            _exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        /// <inheritdoc />
        public async Task ReceiveMessageAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var longPollServer = await GetLongPollServerAsync(cancellationToken);

            var pts = longPollServer.Pts;
            var ts = ulong.Parse(longPollServer.Ts);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var longPollHistory = await GetLongPollHistoryAsync(ts, pts, cancellationToken);
                    if (!longPollHistory.Messages.Any())
                    {
                        continue;
                    }

                    foreach (var message in longPollHistory.Messages)
                    {
                        try
                        {
                            var type = GetMessageType(message);
                            switch (type)
                            {
                                case MessageType.Message:
                                    await OnTextMessage(message, cancellationToken);
                                    break;
                                case MessageType.None:
                                    break;
                            }
                        }
                        catch (System.Exception e)
                        {
                            await (_botException?.Invoke(_botClient, message, e, cancellationToken) ?? throw e);
                        }

                        pts = longPollHistory.NewPts;
                    }
                }
                catch (System.Exception e)
                {
                    await (_exception?.Invoke(e, cancellationToken) ?? throw e);
                }
            }
        }

        /// <summary>
        ///     This method returns the type of incoming message.
        /// </summary>
        /// <param name="message">Private message.</param>
        /// <returns>The type of incoming message.</returns>
        private static MessageType GetMessageType(Message message)
        {
            if (!string.IsNullOrWhiteSpace(message.Text))
            {
                return MessageType.Message;
            }

            return MessageType.None;
        }
        
        /// <summary>
        ///     This method has the logic of processing a new message.
        /// </summary>
        /// <param name="message">User updates</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        private async Task OnTextMessage(Message message, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var command = _textCommands
                .AsParallel()
                .Where(x =>
                {
                    var peerId = x.Key.Item1;
                    var pattern = x.Key.Item2;
                    var options = x.Key.Item3;

                    if (peerId == message.PeerId)
                    {
                        return Regex.IsMatch(message.Text, pattern, options);
                    }
                    
                    return !peerId.HasValue && Regex.IsMatch(message.Text, pattern, options);
                })
                .Select(x => x.Value)
                .FirstOrDefault();

            if (command == null)
            {
                return;
            }

            await command(_botClient, message, cancellationToken);
        }
        
        /// <summary>
        ///     Get data for the connection.
        /// </summary>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Returns data for the connection to long poll</returns>
        private async Task<LongPollServerResponse> GetLongPollServerAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _botClient.Messages.GetLongPollServerAsync(needPts: _longPollConfiguration.NeedPts, lpVersion: _longPollConfiguration.LpVersion);
        }

        ///  <summary>
        ///      Get user events.
        ///  </summary>
        ///  <param name="ts">
        ///      The last value of the ts parameter received from the long Poll server or using
        ///      of the messages method.getLongPollServer.
        ///  </param>
        ///  <param name="pts">
        ///      The last value of the new_pts parameter received from the server's Long Poll,
        ///      used to get actions that
        ///      always stored.
        ///  </param>
        ///  <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        ///  <returns>Returns group events.</returns>
        private async Task<LongPollHistoryResponse> GetLongPollHistoryAsync(
            ulong ts,
            ulong? pts,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _botClient.Messages.GetLongPollHistoryAsync(@params: new MessagesGetLongPollHistoryParams
            {
                Ts = ts,
                Pts = pts,
                Fields = _longPollConfiguration.Fields,
                PreviewLength = _longPollConfiguration.PreviewLength,
                Onlines = _longPollConfiguration.Onlines,
                EventsLimit = _longPollConfiguration.EventsLimit,
                MsgsLimit = _longPollConfiguration.MsgsLimit,
                MaxMsgId = _longPollConfiguration.MaxMsgId,
                LpVersion = _longPollConfiguration.LpVersion
            });
        }

        /// <inheritdoc cref="IDisposable" />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}