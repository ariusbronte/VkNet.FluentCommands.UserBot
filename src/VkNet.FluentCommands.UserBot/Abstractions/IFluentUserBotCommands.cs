using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using VkNet.Abstractions;
using VkNet.FluentCommands.UserBot.Storage;
using VkNet.Model;

namespace VkNet.FluentCommands.UserBot.Abstractions
{
    public interface IFluentGroupBotCommands
    {
        /// <summary>
        ///     Authorize of the page bot with extended parameters.
        /// </summary>
        /// <param name="apiAuthParams">Authorization parameter.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if apiAuthParams is null.</exception>
        Task InitBotAsync(IApiAuthParams apiAuthParams);

        /// <summary>
        ///     Authorize of the page bot with access token only.
        /// </summary>
        /// <param name="login">Login of user account.</param>
        /// <param name="password">Password of user account.</param>
        /// <exception cref="System.ArgumentException">Thrown if login is null or whitespace.</exception>
        /// <exception cref="System.ArgumentException">Thrown if password is null or whitespace.</exception>
        Task InitBotAsync(string login, string password);

        /// <summary>
        ///     Configure <see cref="VkNet.FluentCommands.UserBot.UserLongPollConfiguration"/> with extended parameters.
        /// </summary>
        /// <param name="configuration">Custom long poll configuration.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if configuration is null.</exception>
        void ConfigureUserLongPoll(IUserLongPollConfiguration configuration);

        /// <summary>
        ///     Global extended handler of all incoming messages.
        ///     Triggered if no matches are found or missing in the <see cref="TextCommandsStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if handler is null.</exception>
        void OnText(Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     The extended handler for the incoming message.
        ///     Compares the specified regular expression with the text of the incoming message.
        /// </summary>
        /// <param name="pattern">Regular expression.</param>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if handler in null.</exception>
        void OnText(string pattern, Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     The extended handler for the incoming message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming message based on the regular expression options.
        /// </summary>
        /// <param name="tuple">Regular expression with options.</param>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if handler in null.</exception>
        void OnText((string pattern, RegexOptions options) tuple, Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     The extended handler for the incoming message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming message. Only works for an individual conversation.
        /// </summary>
        /// <param name="tuple">Regular expression with individual conversation id.</param>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if handler in null.</exception>        
        void OnText((long peerId, string pattern) tuple, Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     The extended handler for the incoming message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming message based on the regular expression options.
        ///     Only works for an individual conversation.
        /// </summary>
        /// <param name="tuple">Regular expression with options and individual conversation id.</param>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if handler in null.</exception>
        void OnText((long peerId, string pattern, RegexOptions options) tuple, Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     Global <c>NOT</c> extended handler of all incoming messages.
        ///     Triggered if no matches are found or missing in the <see cref="TextCommandsStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentException">Thrown if answer is null or whitespace.</exception>
        void OnText(string answer);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming message.
        ///     Compares the specified regular expression with the text of the incoming message.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="pattern">Regular expression.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answer in null or whitespace.</exception>
        void OnText(string pattern, string answer);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming message based on the regular expression options.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="tuple">Regular expression with options.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answer in null or whitespace.</exception>
        void OnText((string pattern, RegexOptions options) tuple, string answer);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming message. Only works for an individual conversation.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="tuple">Regular expression with individual conversation id.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answer in null or whitespace.</exception>
        void OnText((long peerId, string pattern) tuple, string answer);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming message based on the regular expression options.
        ///     Only works for an individual conversation.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="tuple">Regular expression with options and individual conversation id.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if answer in null or whitespace.</exception>
        void OnText((long peerId, string pattern, RegexOptions options) tuple, string answer);

        /// <summary>
        ///     Global <c>NOT</c> extended handler of all incoming messages.
        ///     Triggered if no matches are found or missing in the <see cref="TextCommandsStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnText(params string[] answers);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming message.
        ///     Compares the specified regular expression with the text of the incoming message.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="pattern">Regular expression.</param>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnText(string pattern, params string[] answers);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming message based on the regular expression options.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="tuple">Regular expression with options.</param>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnText((string pattern, RegexOptions options) tuple, params string[] answers);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming message. Only works for an individual conversation.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="tuple">Regular expression with individual conversation id.</param>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnText((long peerId, string pattern) tuple, params string[] answers);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming message based on the regular expression options.
        ///     Only works for an individual conversation.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="tuple">Regular expression with options and individual conversation id.</param>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnText((long peerId, string pattern, RegexOptions options) tuple, params string[] answers);

        /// <summary>
        ///     Global extended handler of all incoming reply messages.
        ///     Triggered if no matches are found or missing in the <see cref="ReplyCommandsStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if handler is null.</exception>
        void OnReply(Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     The extended handler for the incoming reply message.
        ///     Compares the specified regular expression with the text of the incoming message.
        /// </summary>
        /// <param name="pattern">Regular expression.</param>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if handler in null.</exception>
        void OnReply(string pattern, Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     The extended handler for the incoming reply message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming reply message based on the regular expression options.
        /// </summary>
        /// <param name="tuple">Regular expression with options.</param>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if handler in null.</exception>
        void OnReply((string pattern, RegexOptions options) tuple, Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     The extended handler for the incoming reply message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming reply message. Only works for an individual conversation.
        /// </summary>
        /// <param name="tuple">Regular expression with individual conversation id.</param>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if handler in null.</exception>        
        void OnReply((long peerId, string pattern) tuple, Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     The extended handler for the incoming reply message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming replpy message based on the regular expression options.
        ///     Only works for an individual conversation.
        /// </summary>
        /// <param name="tuple">Regular expression with options and individual conversation id.</param>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if handler in null.</exception>
        void OnReply((long peerId, string pattern, RegexOptions options) tuple, Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     Global <c>NOT</c> extended handler of all incoming reply messages.
        ///     Triggered if no matches are found or missing in the <see cref="ReplyCommandsStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentException">Thrown if answer is null or whitespace.</exception>
        void OnReply(string answer);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming reply message.
        ///     Compares the specified regular expression with the text of the incoming message.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="pattern">Regular expression.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answer in null or whitespace.</exception>
        void OnReply(string pattern, string answer);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming reply message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming reply message based on the regular expression options.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="tuple">Regular expression with options.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answer in null or whitespace.</exception>
        void OnReply((string pattern, RegexOptions options) tuple, string answer);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming reply message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming reply message. Only works for an individual conversation.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="tuple">Regular expression with individual conversation id.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answer in null or whitespace.</exception>
        void OnReply((long peerId, string pattern) tuple, string answer);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming reply message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming reply message based on the regular expression options.
        ///     Only works for an individual conversation.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="tuple">Regular expression with options and individual conversation id.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if answer in null or whitespace.</exception>
        void OnReply((long peerId, string pattern, RegexOptions options) tuple, string answer);

        /// <summary>
        ///     Global <c>NOT</c> extended handler of all incoming reply messages.
        ///     Triggered if no matches are found or missing in the <see cref="ReplyCommandsStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnReply(params string[] answers);
        
        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming reply message.
        ///     Compares the specified regular expression with the text of the incoming message.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="pattern">Regular expression.</param>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnReply(string pattern, params string[] answers);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming reply message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming reply message based on the regular expression options.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="tuple">Regular expression with options.</param>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnReply((string pattern, RegexOptions options) tuple, params string[] answers);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming reply message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming reply message. Only works for an individual conversation.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="tuple">Regular expression with individual conversation id.</param>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnReply((long peerId, string pattern) tuple, params string[] answers);

        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming reply message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming reply message based on the regular expression options.
        ///     Only works for an individual conversation.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="tuple">Regular expression with options and individual conversation id.</param>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnReply((long peerId, string pattern, RegexOptions options) tuple, params string[] answers);

        /// <summary>
        ///     Global extended handler of all incoming sticker messages.
        ///     Triggered if no matches are found or missing in the <see cref="StickerCommandsStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if handler is null.</exception>
        void OnSticker(Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     The extended handler for the incoming sticker message.
        ///     Compares the specified regular expression with the text of the incoming message.
        /// </summary>
        /// <param name="stickerId">Sticker id.</param>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the stickerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if handler in null.</exception>
        void OnSticker(long stickerId, Func<IVkApi, Message, CancellationToken, Task> handler);
        
        /// <summary>
        ///     The extended handler for the incoming sticker message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming sticker message. Only works for an individual conversation.
        /// </summary>
        /// <param name="tuple">Sticker id with individual conversation id.</param>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if stickerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentNullException">Thrown if handler in null.</exception>        
        void OnSticker((long peerId, long stickerId) tuple, Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     Global <c>NOT</c> extended handler of all incoming sticker messages.
        ///     Triggered if no matches are found or missing in the <see cref="StickerCommandsStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if stickerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answer is null or whitespace.</exception>
        void OnSticker(string answer);
        
        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming sticker message.
        ///     Compares the specified regular expression with the text of the incoming message.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="stickerId">Sticker id.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if stickerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answer in null or whitespace.</exception>
        void OnSticker(long stickerId, string answer);
        
        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming sticker message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming sticker message. Only works for an individual conversation.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="tuple">Sticker id with individual conversation id.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if stickerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answer in null or whitespace.</exception>
        void OnSticker((long peerId, long stickerId) tuple, string answer);
        
        /// <summary>
        ///     Global <c>NOT</c> extended handler of all incoming sticker messages.
        ///     Triggered if no matches are found or missing in the <see cref="StickerCommandsStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnSticker(params string[] answers);
        
        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming sticker message.
        ///     Compares the specified regular expression with the text of the incoming message.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="stickerId">Sticker id.</param>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if stickerId is less than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnSticker(long stickerId, params string[] answers);
        
        /// <summary>
        ///     The <c>NOT</c> extended handler for the incoming sticker message.
        ///     Compares the specified regular expression with the text of
        ///     the incoming sticker message. Only works for an individual conversation.
        /// </summary>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="tuple">Sticker id with individual conversation id.</param>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if peerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if stickerId is less than or equal to zero.</exception>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnSticker((long peerId, long stickerId) tuple, params string[] answers);

        /// <summary>
        ///     Global extended handler of all incoming photo messages.
        ///     Triggered if no matches are found or missing in the <see cref="PhotoEventStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if handler is null.</exception>
        void OnPhoto(Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     Global <c>NOT</c> extended handler of all incoming photo messages.
        ///     Triggered if no matches are found or missing in the <see cref="PhotoEventStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentException">Thrown if answer is null or whitespace.</exception>
        void OnPhoto(string answer);
        
        /// <summary>
        ///     Global <c>NOT</c> extended handler of all incoming photo messages.
        ///     Triggered if no matches are found or missing in the <see cref="PhotoEventStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnPhoto(params string[] answers);

        /// <summary>
        ///     Global extended handler of all incoming voice messages.
        ///     Triggered if no matches are found or missing in the <see cref="VoiceEventStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if handler is null.</exception>
        void OnVoice(Func<IVkApi, Message, CancellationToken, Task> handler);

        /// <summary>
        ///     Global <c>NOT</c> extended handler of all incoming voice messages.
        ///     Triggered if no matches are found or missing in the <see cref="VoiceEventStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentException">Thrown if answer is null or whitespace.</exception>
        void OnVoice(string answer);
        
        /// <summary>
        ///     Global <c>NOT</c> extended handler of all incoming voice messages.
        ///     Triggered if no matches are found or missing in the <see cref="VoiceEventStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnVoice(params string[] answers);
        
        /// <summary>
        ///     Global extended handler of all incoming forward messages.
        ///     Triggered if no matches are found or missing in the <see cref="ForwardEventStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <param name="handler">Handler logic.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if handler is null.</exception>
        void OnForward(Func<IVkApi, Message, CancellationToken, Task> handler);
        
        /// <summary>
        ///     Global <c>NOT</c> extended handler of all incoming forward messages.
        ///     Triggered if no matches are found or missing in the <see cref="ForwardEventStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentException">Thrown if answer is null or whitespace.</exception>
        void OnForward(string answer);
        
        /// <summary>
        ///     Global <c>NOT</c> extended handler of all incoming forward messages.
        ///     Triggered if no matches are found or missing in the <see cref="ForwardEventStore"/>.
        /// </summary>
        /// <remarks>Is not required.</remarks>
        /// <remarks>Is an abstraction over the main handler.</remarks>
        /// <remarks>Selects a random string from the array to send the message to.</remarks>
        /// <param name="answers">Text responses.</param>
        /// <exception cref="ArgumentNullException">Thrown if answers is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answers is empty.</exception>
        void OnForward(params string[] answers);

        /// <summary>
        ///     The handler for all exceptions long poll.
        /// </summary>
        /// <param name="handler">Handler logic</param>
        /// <exception cref="System.ArgumentNullException">Thrown if handler in null.</exception>        
        void OnBotException(Func<IVkApi, Message, System.Exception, CancellationToken, Task> handler);

        /// <summary>
        ///     The handler for all exceptions.
        /// </summary>
        /// <param name="handler">Handler logic</param>
        /// <exception cref="System.ArgumentNullException">Thrown if handler in null.</exception>        
        void OnException(Func<System.Exception, CancellationToken, Task> handler);

        /// <summary>
        ///     Start receiving messages.
        /// </summary>
        Task ReceiveMessageAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc cref="IDisposable" />
        void Dispose();
    }
}