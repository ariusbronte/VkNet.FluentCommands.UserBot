using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;

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

    /// <summary>
    ///     Main entry class to use VkNet.FluentCommands.UserBot.
    /// </summary>
    /// <typeparam name="TBotClient">Custom implementation of interaction with VK.</typeparam>
    public class FluentUserBotCommands<TBotClient> where TBotClient : IVkApi
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

        /// <summary>
        ///     Authorize of the user bot.
        /// </summary>
        /// <param name="apiAuthParams">Authorization parameter.</param>
        /// <exception cref="ArgumentNullException">Thrown if apiAuthParams is null.</exception>
        public async Task InitBotAsync(IApiAuthParams apiAuthParams)
        {
            if (apiAuthParams == null)
            {
                throw new ArgumentNullException(paramName: nameof(apiAuthParams));
            }

            await _botClient.AuthorizeAsync(@params: apiAuthParams);
        }

        /// <summary>
        ///     Method to set custom <see cref="VkNet.FluentCommands.UserBot.UserLongPollConfiguration"/>.
        /// </summary>
        /// <param name="configuration">Custom long poll configuration.</param>
        public void ConfigureUserLongPoll(UserLongPollConfiguration configuration)
        {
            _longPollConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        ///     Trigger on a text command.
        /// </summary>
        /// <param name="pattern">Regular expression.</param>
        /// <param name="func">Trigger actions performed.</param>
        /// <exception cref="ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="ArgumentNullException">Thrown if trigger actions in null.</exception>
        public void OnText(string pattern, Func<IVkApi, Message, CancellationToken, Task> func)
        {
            OnText(tuple: (pattern, RegexOptions.None), func: func);
        }

        /// <summary>
        ///     Trigger on a text command.
        /// </summary>
        /// <param name="tuple">Regular expression and Regex options.</param>
        /// <param name="func">Trigger actions performed.</param>
        /// <exception cref="ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="ArgumentNullException">Thrown if trigger actions in null.</exception>
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
    }
}