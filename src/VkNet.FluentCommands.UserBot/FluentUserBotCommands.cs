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
        public void OnText((string pattern, RegexOptions options) tuple,
            Func<IVkApi, Message, CancellationToken, Task> func)
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
        public void OnException(Func<System.Exception, CancellationToken, Task> exception)
        {
            _exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }

        /// <inheritdoc />
        public async Task ReceiveMessageAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var longPollServer = await GetLongPollServerAsync(cancellationToken: cancellationToken);
            
            var pts = longPollServer.Pts;
            var ts = ulong.Parse(s: longPollServer.Ts);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var longPollHistory =
                        await GetLongPollHistoryAsync(ts: ts, pts: pts, cancellationToken: cancellationToken);
                    if (longPollHistory?.Messages == null)
                    {
                        continue;
                    }

                    foreach (var update in longPollHistory.Messages)
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
                            await (_botException?.Invoke(_botClient, update, e, cancellationToken) ?? throw e);
                        }
                    }

                    pts = longPollHistory.NewPts;
                }
                catch (System.Exception e)
                {
                    await (_exception?.Invoke(e, cancellationToken) ?? throw e);
                }
            }
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