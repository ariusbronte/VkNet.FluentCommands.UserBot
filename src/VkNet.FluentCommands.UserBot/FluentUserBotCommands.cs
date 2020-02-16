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

        /// <summary>
        ///     Get user events.
        /// </summary>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Returns group events.</returns>
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