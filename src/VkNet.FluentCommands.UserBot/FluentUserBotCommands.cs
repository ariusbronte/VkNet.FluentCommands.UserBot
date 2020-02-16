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
        private readonly ConcurrentDictionary<(string, RegexOptions), Func<IVkApi, Message, CancellationToken, Task>>
            _textCommands = new ConcurrentDictionary<(string, RegexOptions), Func<IVkApi, Message, CancellationToken, Task>>();

        /// <summary>
        ///     Stores the message logic exception handler
        /// </summary>
        private Func<IVkApi, Message, System.Exception, CancellationToken, Task> _botException;

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

            _textCommands.TryAdd(key: (tuple.pattern, tuple.options), value: func);
        }

        /// <inheritdoc />
        public void OnBotException(Func<IVkApi, Message, System.Exception, CancellationToken, Task> botException)
        {
            _botException = botException ?? throw new ArgumentNullException(nameof(botException));
        }

        /// <inheritdoc />
        public async Task ReceiveMessageAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var longPollServer = await GetLongPollServerAsync(
                needPts: _longPollConfiguration.NeedPts,
                lpVersion: _longPollConfiguration.LpVersion,
                cancellationToken: cancellationToken);

            var pts = longPollServer.Pts;
            var ts = ulong.Parse(s: longPollServer.Ts);

            while (!cancellationToken.IsCancellationRequested)
            {
                var longPollHistory = await GetLongPollHistoryAsync(
                    fields: _longPollConfiguration.Fields,
                    ts: ts,
                    pts: pts,
                    previewLength: _longPollConfiguration.PreviewLength,
                    onlines: _longPollConfiguration.Onlines,
                    eventsLimit: _longPollConfiguration.EventsLimit,
                    msgsLimit: _longPollConfiguration.MsgsLimit,
                    maxMsgId: _longPollConfiguration.MaxMsgId,
                    lpVersion: _longPollConfiguration.LpVersion,
                    cancellationToken: cancellationToken);

                if (longPollHistory?.Messages == null)
                {
                    continue;
                }

                Parallel.ForEach(source: longPollHistory.Messages, body: async update =>
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(update.Text))
                        {
                            var command = _textCommands
                                .Where(predicate: x =>
                                    Regex.IsMatch(input: update.Text, pattern: x.Key.Item1, options: x.Key.Item2))
                                .Select(selector: x => x.Value)
                                .SingleOrDefault();

                            if (command == null)
                            {
                                return;
                            }

                            await command(arg1: _botClient, arg2: update, arg3: cancellationToken);
                        }
                    }
                    catch (System.Exception e)
                    {
                        if (_botException != null)
                        {
                            await _botException.Invoke(_botClient, update, e, cancellationToken);
                        }
                    }
                });

                pts = longPollHistory.NewPts;
            }
        }

        /// <summary>
        ///     Get data for the connection.
        /// </summary>
        /// <param name="needPts">Pts.</param>
        /// <param name="lpVersion">Version.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Returns data for the connection to long poll</returns>
        private async Task<LongPollServerResponse> GetLongPollServerAsync(
            bool needPts,
            uint lpVersion,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _botClient.Messages.GetLongPollServerAsync(needPts: needPts, lpVersion: lpVersion);
        }

        ///  <summary>
        ///      Get user events.
        ///  </summary>
        ///  <param name="fields">List of additional fields to return.</param>
        ///  <param name="ts">
        ///      The last value of the ts parameter received from the long Poll server or using
        ///      of the messages method.getLongPollServer.
        ///  </param>
        ///  <param name="pts">
        ///      The last value of the new_pts parameter received from the server's Long Poll,
        ///      used to get actions that
        ///      always stored.
        ///  </param>
        ///  <param name="previewLength">
        ///      The number of characters you want to trim the message. Enter 0 if You
        ///      don't want to crop the message. (across
        ///      by default, messages are not truncated).
        ///  </param>
        ///  <param name="onlines">
        ///      If you pass a value of 1 to this parameter, the history will be returned only from those
        ///      users who are currently
        ///      online. the flag can take the values 1 or 0.
        ///  </param>
        ///  <param name="eventsLimit">
        ///      If the number of events in the history exceeds this value, it will be returned error.
        ///      Positive number. The default is 1000. The minimum value is 1000.
        /// </param>
        ///  <param name="msgsLimit">The number of messages to return.</param>
        ///  <param name="maxMsgId">
        ///     Maximum message ID among those already available in the local copy.
        ///     It is necessary to consider how the messages,
        ///     received via API methods (for example, messages.getDialogs,
        ///     messages.getHistory), and data obtained from Long.
        /// </param>
        ///  <param name="lpVersion">Version for connecting to Long Poll. Actual version: 3.</param>
        ///  <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        ///  <returns>Returns group events.</returns>
        private async Task<LongPollHistoryResponse> GetLongPollHistoryAsync(
            UsersFields fields,
            ulong ts,
            ulong? pts,
            long? previewLength,
            bool? onlines,
            long? eventsLimit,
            long? msgsLimit,
            long? maxMsgId,
            ulong? lpVersion,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _botClient.Messages.GetLongPollHistoryAsync(@params: new MessagesGetLongPollHistoryParams
            {
                Fields = fields,
                Ts = ts,
                Pts = pts,
                PreviewLength = previewLength,
                Onlines = onlines,
                EventsLimit = eventsLimit,
                MsgsLimit = msgsLimit,
                MaxMsgId = maxMsgId,
                LpVersion = lpVersion
            });
        }

        /// <inheritdoc cref="IDisposable" />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}