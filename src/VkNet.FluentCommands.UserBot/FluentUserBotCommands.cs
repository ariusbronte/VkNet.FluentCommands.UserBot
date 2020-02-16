using System;
using System.Threading.Tasks;
using VkNet.Abstractions;
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
        public FluentUserBotCommands() : base(botClient: () => new VkApi())
        {
        }
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
    }
}
