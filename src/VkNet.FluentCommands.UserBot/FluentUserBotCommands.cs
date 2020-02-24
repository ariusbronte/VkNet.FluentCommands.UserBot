﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.FluentCommands.UserBot.Abstractions;
using VkNet.FluentCommands.UserBot.Handlers;
using VkNet.FluentCommands.UserBot.Storage;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;

// ReSharper disable MemberCanBeProtected.Global

namespace VkNet.FluentCommands.UserBot
{
    public class FluentUserBotCommands : IFluentGroupBotCommands
    {
        /// <summary>
        ///     Implementation of interaction with VK.
        /// </summary>
        private readonly IVkApi _botClient;
        private IUserLongPollConfiguration _longPollConfiguration = UserLongPollConfiguration.Default;
        
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private readonly Random _random = new Random();
        
        private readonly TextCommandsStore _textCommands = new TextCommandsStore();
        private readonly ReplyCommandsStore _replyCommands = new ReplyCommandsStore();
        private readonly StickerCommandsStore _stickerCommands = new StickerCommandsStore();
        
        private readonly PhotoEventStore _photoEvent = new PhotoEventStore();
        private readonly VoiceEventStore _voiceEvent = new VoiceEventStore();
        private readonly ForwardEventStore _forwardEvent = new ForwardEventStore();

        private readonly ChatCreateEventStore _chatCreateEvent = new ChatCreateEventStore();
        private readonly ChatInviteUserEventStore _chatInviteUserEvent = new ChatInviteUserEventStore();
        private readonly ChatKickUserEventStore _chatKickUserEvent = new ChatKickUserEventStore();
        private readonly ChatPhotoRemoveEventStore _chatPhotoRemoveEvent = new ChatPhotoRemoveEventStore();
        private readonly ChatPhotoUpdateEventStore _chatPhotoUpdateEvent = new ChatPhotoUpdateEventStore();
        private readonly ChatPinMessageEventStore _chatPinMessageEvent = new ChatPinMessageEventStore();
        private readonly ChatTitleUpdateEventStore _chatTitleUpdateEvent = new ChatTitleUpdateEventStore();
        private readonly ChatUnpinMessageEventStore _chatUnpinMessageEvent = new ChatUnpinMessageEventStore();
        private readonly ChatInviteUserByLinkEventStore _chatInviteUserByLinkEvent = new ChatInviteUserByLinkEventStore();

        private readonly BotExceptionEventStore _botExceptionEvent = new BotExceptionEventStore();
        private readonly ExceptionEventStore _exceptionEvent = new ExceptionEventStore();

        /// <summary>
        ///      Initializes a new instance of the <see cref="VkNet.FluentCommands.UserBot.FluentUserBotCommands"/> class.
        /// </summary>
        /// <param name="botClient">Implementation of interaction with VK.</param>
        public FluentUserBotCommands(Func<IVkApi> botClient)
        {
            _botClient = botClient();
        }
        
        /// <summary>
        ///     Initializes a new instance of the
        ///     <see cref="VkNet.FluentCommands.UserBot.FluentUserBotCommands"/> class without parameters.
        /// </summary>
        public FluentUserBotCommands()
        {
            _botClient = new VkApi(new ServiceCollection().AddAudioBypass());
        }

        /// <inheritdoc />
        public async Task InitBotAsync(IApiAuthParams apiAuthParams)
        {
            if (apiAuthParams == null) throw new ArgumentNullException(nameof(apiAuthParams));

            await _botClient.AuthorizeAsync(apiAuthParams).ConfigureAwait(false);
        }
        
        /// <inheritdoc />
        public async Task InitBotAsync(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(login));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(password));
            
            await _botClient.AuthorizeAsync(new ApiAuthParams
            {
                Login = login,
                Password = password
            }).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void ConfigureUserLongPoll(IUserLongPollConfiguration configuration)
        {
            _longPollConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        #region OnTextHandlers
        /// <inheritdoc />
        public void OnText(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _textCommands.SetHandler(handler);
        }
        
        /// <inheritdoc />
        public void OnText(string pattern, Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _textCommands.Store((null, pattern, RegexOptions.None), handler);
        }
        
        /// <inheritdoc />
        public void OnText((string pattern, RegexOptions options) tuple, Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _textCommands.Store((null, tuple.pattern, tuple.options), handler);
        }
        
        /// <inheritdoc />
        public void OnText((long peerId, string pattern) tuple, Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            OnText((tuple.peerId, tuple.pattern, RegexOptions.None), handler);
        }
        
        /// <inheritdoc />
        public void OnText((long peerId, string pattern, RegexOptions options) tuple, Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            if (tuple.peerId <= 0) throw new ArgumentOutOfRangeException(nameof(tuple.peerId));

            _textCommands.Store(tuple, handler);
        }
        
        /// <inheritdoc />
        public void OnText(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(answer));
            
            OnText(async (api, update, token) =>
            {
                token.ThrowIfCancellationRequested();
                await SendAsync(update.PeerId, answer);
            });
        }

        /// <inheritdoc />
        public void OnText(string pattern, string answer)
        {
            OnTextHandler((null, pattern, RegexOptions.None), answer);
        }
        
        /// <inheritdoc />
        public void OnText((string pattern, RegexOptions options) tuple, string answer)
        {
            OnTextHandler((null, tuple.pattern, tuple.options), answer);
        }
        
        /// <inheritdoc />
        public void OnText((long peerId, string pattern) tuple, string answer)
        {
            OnText((tuple.peerId, tuple.pattern, RegexOptions.None), answer);
        }
        
        /// <inheritdoc />
        public void OnText((long peerId, string pattern, RegexOptions options) tuple, string answer)
        { 
            if (tuple.peerId <= 0) throw new ArgumentOutOfRangeException(nameof(tuple.peerId));

            OnTextHandler(tuple, answer);
        }
        
        /// <inheritdoc />
        public void OnText(params string[] answers)
        {
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));
            
            OnText(async (api, update, token) =>
            {
                token.ThrowIfCancellationRequested();
                await SendAsync(update.PeerId, answers[_random.Next(0, answers.Length)]);
            });
        }
        
        /// <inheritdoc />
        public void OnText(string pattern, params string[] answers)
        {
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));
            
            OnTextHandler((null, pattern, RegexOptions.None), answers[_random.Next(0, answers.Length)]);
        }
        
        /// <inheritdoc />
        public void OnText((string pattern, RegexOptions options) tuple, params string[] answers)
        {
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));
            
            OnTextHandler((null, tuple.pattern, tuple.options), answers[_random.Next(0, answers.Length)]);
        }
        
        /// <inheritdoc />
        public void OnText((long peerId, string pattern) tuple, params string[] answers)
        {
            OnText((tuple.peerId, tuple.pattern, RegexOptions.None), answers);
        }
        
        /// <inheritdoc />
        public void OnText((long peerId, string pattern, RegexOptions options) tuple, params string[] answers)
        {
            if (tuple.peerId <= 0) throw new ArgumentOutOfRangeException(nameof(tuple.peerId));
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));

            OnTextHandler(tuple, answers[_random.Next(0, answers.Length)]);
        }
        
        /// <summary>
        ///     Common logic of abstracted text handlers.
        /// </summary>
        /// <param name="tuple">Regular expression and Regex options.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answer is null or whitespace.</exception>
        private void OnTextHandler((long? peerId, string pattern, RegexOptions options) tuple, string answer)
        {
            if (string.IsNullOrWhiteSpace(answer)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(answer));
            
            _textCommands.Store(tuple, async (api, update, token) =>
            {
                token.ThrowIfCancellationRequested();
                await SendAsync(update.PeerId, answer);
            });
        }
        #endregion

        #region OnReplyHandlers
        /// <inheritdoc />
        public void OnReply(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _replyCommands.SetHandler(handler);
        }
        
        /// <inheritdoc />
        public void OnReply(string pattern, Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _replyCommands.Store((null, pattern, RegexOptions.None), handler);
        }
        
        /// <inheritdoc />
        public void OnReply((string pattern, RegexOptions options) tuple, Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _replyCommands.Store((null, tuple.pattern, tuple.options), handler);
        }
        
        /// <inheritdoc />
        public void OnReply((long peerId, string pattern) tuple, Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            OnReply((tuple.peerId, tuple.pattern, RegexOptions.None), handler);
        }
        
        /// <inheritdoc />
        public void OnReply((long peerId, string pattern, RegexOptions options) tuple, Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            if (tuple.peerId <= 0) throw new ArgumentOutOfRangeException(nameof(tuple.peerId));

            _replyCommands.Store(tuple, handler);
        }
        
        /// <inheritdoc />
        public void OnReply(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(answer));
            
            OnReply(async (api, update, token) =>
            {
                token.ThrowIfCancellationRequested();
                await SendAsync(update.PeerId, answer);
            });
        }
        
        /// <inheritdoc />
        public void OnReply(string pattern, string answer)
        {
            OnReplyHandler((null, pattern, RegexOptions.None), answer);
        }
        
        /// <inheritdoc />
        public void OnReply((string pattern, RegexOptions options) tuple, string answer)
        {
            OnReplyHandler((null, tuple.pattern, tuple.options), answer);
        }
        
        /// <inheritdoc />
        public void OnReply((long peerId, string pattern) tuple, string answer)
        {
            OnReply((tuple.peerId, tuple.pattern, RegexOptions.None), answer);
        }
        
        /// <inheritdoc />
        public void OnReply((long peerId, string pattern, RegexOptions options) tuple, string answer)
        { 
            if (tuple.peerId <= 0) throw new ArgumentOutOfRangeException(nameof(tuple.peerId));

            OnReplyHandler(tuple, answer);
        }
        
        /// <inheritdoc />
        public void OnReply(params string[] answers)
        {
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));

            OnReply(answers[_random.Next(0, answers.Length)]);
        } 
        
        /// <inheritdoc />
        public void OnReply(string pattern, params string[] answers)
        {
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));
            
            OnReplyHandler((null, pattern, RegexOptions.None), answers[_random.Next(0, answers.Length)]);
        }
        
        /// <inheritdoc />
        public void OnReply((string pattern, RegexOptions options) tuple, params string[] answers)
        {
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));
            
            OnReplyHandler((null, tuple.pattern, tuple.options), answers[_random.Next(0, answers.Length)]);
        }
        
        /// <inheritdoc />
        public void OnReply((long peerId, string pattern) tuple, params string[] answers)
        {
            OnReply((tuple.peerId, tuple.pattern, RegexOptions.None), answers);
        }
        
        /// <inheritdoc />
        public void OnReply((long peerId, string pattern, RegexOptions options) tuple, params string[] answers)
        {
            if (tuple.peerId <= 0) throw new ArgumentOutOfRangeException(nameof(tuple.peerId));
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));

            OnReplyHandler(tuple, answers[_random.Next(0, answers.Length)]);
        }

        /// <summary>
        ///     Common logic of abstracted reply handlers.
        /// </summary>
        /// <param name="tuple">Regular expression and Regex options.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="ArgumentException">Thrown if regular expression is null or whitespace.</exception>
        /// <exception cref="InvalidEnumArgumentException">Thrown if regex options is not defined.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answer is null or whitespace.</exception>
        private void OnReplyHandler((long? peerId, string pattern, RegexOptions options) tuple, string answer)
        {
            if (string.IsNullOrWhiteSpace(answer)) 
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(answer));
            
            _replyCommands.Store(tuple, async (api, update, token) =>
            {
                token.ThrowIfCancellationRequested();
                await SendAsync(update.PeerId, answer);
            });
        }
        #endregion
        
        #region StickerHandlers
        /// <inheritdoc />
        public void OnSticker(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _stickerCommands.SetHandler(handler);
        }

        /// <inheritdoc />
        public void OnSticker(long stickerId, Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _stickerCommands.Store((null, stickerId), handler);
        }

        /// <inheritdoc />
        public void OnSticker((long peerId, long stickerId) tuple, Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            if (tuple.peerId <= 0) throw new ArgumentOutOfRangeException(nameof(tuple.peerId));
            
            _stickerCommands.Store(tuple, handler);
        }
        
        /// <inheritdoc />
        public void OnSticker(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(answer));

            OnSticker(async (api, update, token) =>
            {
                token.ThrowIfCancellationRequested();
                await SendAsync(update.PeerId, answer);
            });
        }
        
        /// <inheritdoc />
        public void OnSticker(long stickerId, string answer)
        {
            OnStickerHandler((null, stickerId), answer);
        }

        /// <inheritdoc />
        public void OnSticker((long peerId, long stickerId) tuple, string answer)
        {
            if (tuple.peerId <= 0) throw new ArgumentOutOfRangeException(nameof(tuple.peerId));
            
            OnStickerHandler(tuple, answer);
        }

        /// <inheritdoc />
        public void OnSticker(params string[] answers)
        {
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));
            
            OnSticker(async (api, update, token) =>
            {
                token.ThrowIfCancellationRequested();
                await SendAsync(update.PeerId, answers[_random.Next(0, answers.Length)]);
            });
        }

        /// <inheritdoc />
        public void OnSticker(long stickerId, params string[] answers)
        {
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));
            
            OnStickerHandler((null, stickerId), answers[_random.Next(0, answers.Length)]);
        }

        /// <inheritdoc />
        public void OnSticker((long peerId, long stickerId) tuple, params string[] answers)
        {
            if (tuple.peerId <= 0) throw new ArgumentOutOfRangeException(nameof(tuple.peerId));
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));

            OnStickerHandler(tuple, answers[_random.Next(0, answers.Length)]);
        }

        /// <summary>
        ///     Common logic of abstracted reply handlers.
        /// </summary>
        /// <param name="tuple">Regular expression and Regex options.</param>
        /// <param name="answer">Text response.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown if the stickerId is less than or equal to zero.</exception>
        /// <exception cref="System.ArgumentException">Thrown if answer is null or whitespace.</exception>
        private void OnStickerHandler((long? peerId, long stickerId) tuple, string answer)
        {            
            if (string.IsNullOrWhiteSpace(answer))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(answer));

            _stickerCommands.Store(tuple, async (api, update, token) =>
            {
                token.ThrowIfCancellationRequested();
                await SendAsync(update.PeerId, answer);
            });
        }
        #endregion
        
        #region EventHandlers

        /// <inheritdoc />
        public void OnPhoto(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _photoEvent.SetHandler(handler);
        }

        /// <inheritdoc />
        public void OnPhoto(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(answer));

            OnPhoto(async (api, update, token) =>
            {
                token.ThrowIfCancellationRequested();
                await SendAsync(update.PeerId, answer);
            });
        }
        
        /// <inheritdoc />
        public void OnPhoto(params string[] answers)
        {
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));
            
            OnPhoto(answers[_random.Next(0, answers.Length)]);
        }
        
        /// <inheritdoc />
        public void OnVoice(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _voiceEvent.SetHandler(handler);
        }
        
        /// <inheritdoc />
        public void OnVoice(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(answer));
            
            OnVoice(async (api, update, token) =>
            {
                token.ThrowIfCancellationRequested();
                await SendAsync(update.PeerId, answer);
            });
        }
        
        /// <inheritdoc />
        public void OnVoice(params string[] answers)
        {
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));
            
            OnVoice(answers[_random.Next(0, answers.Length)]);
        }

        /// <inheritdoc />
        public void OnForward(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _forwardEvent.SetHandler(handler);
        }
        
        /// <inheritdoc />
        public void OnForward(string answer)
        {
            if (string.IsNullOrWhiteSpace(answer)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(answer));
            
            OnForward(async (api, update, token) =>
            {
                token.ThrowIfCancellationRequested();
                await SendAsync(update.PeerId, answer);
            });
        }
        
        /// <inheritdoc />
        public void OnForward(params string[] answers)
        {
            if (answers == null) throw new ArgumentNullException(nameof(answers));
            if (answers.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(answers));
            
            OnForward(answers[_random.Next(0, answers.Length)]);
        }
        
        #region EventHandlers
        /// <inheritdoc />
        public void OnChatCreateAction(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _chatCreateEvent.SetHandler(handler);
        }
        
        /// <inheritdoc />
        public void OnChatInviteUserAction(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _chatInviteUserEvent.SetHandler(handler);
        }

        /// <inheritdoc />
        public void OnChatKickUserAction(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _chatKickUserEvent.SetHandler(handler);
        }

        /// <inheritdoc />
        public void OnChatPhotoRemoveAction(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _chatPhotoRemoveEvent.SetHandler(handler);
        }

        /// <inheritdoc />
        public void OnChatPhotoUpdateAction(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _chatPhotoUpdateEvent.SetHandler(handler);
        }

        /// <inheritdoc />
        public void OnChatPinMessageAction(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _chatPinMessageEvent.SetHandler(handler);
        }

        /// <inheritdoc />
        public void OnChatTitleUpdateAction(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _chatTitleUpdateEvent.SetHandler(handler);
        }

        /// <inheritdoc />
        public void OnChatUnpinMessageAction(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _chatUnpinMessageEvent.SetHandler(handler);
        }

        /// <inheritdoc />
        public void OnChatInviteUserByLinkAction(Func<IVkApi, Message, CancellationToken, Task> handler)
        {
            _chatInviteUserByLinkEvent.SetHandler(handler);
        }
        #endregion

        /// <inheritdoc />
        public void OnBotException(Func<IVkApi, Message, System.Exception, CancellationToken, Task> handler)
        {
            _botExceptionEvent.SetHandler(handler);
        }
        
        /// <inheritdoc />
        public void OnException(Func<System.Exception, CancellationToken, Task> handler)
        {
            _exceptionEvent.SetHandler(handler);
        }
        #endregion

        /// <inheritdoc />
        public async Task ReceiveMessageAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var longPollServer = await GetLongPollServerAsync(cancellationToken).ConfigureAwait(false);

            var pts = longPollServer.Pts;
            var ts = ulong.Parse(longPollServer.Ts);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var longPollHistory = await GetLongPollHistoryAsync(ts, pts, cancellationToken).ConfigureAwait(false);
                    if (!longPollHistory.Messages.Any()) continue;

                    foreach (var update in longPollHistory.Messages)
                    {
                        try
                        {
                            if (update == null) continue;
                            
                            if (_longPollConfiguration?.MessageType != null)
                            {
                                if (update?.Type != _longPollConfiguration?.MessageType) continue;
                            }

                            if (!update.PeerId.HasValue) throw new System.Exception("No PeerId");
                            if (!update.FromId.HasValue) throw new System.Exception("No PeerId");
                            
                            var forwardedMessages = update.ForwardedMessages?.ToArray() ?? new Message[] { };
                            var attachments = update.Attachments?.ToArray() ?? new Attachment[] { };
                            var replyMessage = update.ReplyMessage;
                            var actionObject = update.Action;
                            var type = DetectMessageType(forwardedMessages, attachments, actionObject, replyMessage);
                            var messageToProcess = new MessageToProcess(_botClient, update);
                            
                            switch (type)
                            {
                                case VkMessageType.Message:
                                case VkMessageType.Reply:
                                case VkMessageType.Sticker:
                                    await CreateCommandHandler(type).Handle(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                case VkMessageType.Forward:
                                    await _forwardEvent.TriggerHandler(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                case VkMessageType.Photo:
                                    await _photoEvent.TriggerHandler(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                case VkMessageType.Voice:
                                    await _voiceEvent.TriggerHandler(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                case VkMessageType.ChatCreate:
                                    await _chatCreateEvent.TriggerHandler(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                case VkMessageType.ChatInviteUser:
                                    await _chatInviteUserEvent.TriggerHandler(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                case VkMessageType.ChatKickUser:
                                    await _chatKickUserEvent.TriggerHandler(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                case VkMessageType.ChatPhotoRemove:
                                    await _chatPhotoRemoveEvent.TriggerHandler(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                case VkMessageType.ChatPhotoUpdate:
                                    await _chatPhotoUpdateEvent.TriggerHandler(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                case VkMessageType.ChatPinMessage:
                                    await _chatPinMessageEvent.TriggerHandler(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                case VkMessageType.ChatTitleUpdate:
                                    await _chatTitleUpdateEvent.TriggerHandler(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                case VkMessageType.ChatUnpinMessage:
                                    await _chatUnpinMessageEvent.TriggerHandler(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                case VkMessageType.ChatInviteUserByLink:
                                    await _chatInviteUserByLinkEvent.TriggerHandler(messageToProcess, cancellationToken).ConfigureAwait(false);
                                    continue;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        catch (System.Exception e)
                        {
                            var messageToProcess = new MessageToProcess(_botClient, update);
                            await _botExceptionEvent.TriggerHandler(messageToProcess, e, cancellationToken).ConfigureAwait(false);
                            continue;
                        }
                    }
                    
                    pts = longPollHistory.NewPts;
                }
                catch (LongPollKeyExpiredException e)
                {
                    longPollServer = await GetLongPollServerAsync(cancellationToken).ConfigureAwait(false);

                    ts = ulong.Parse(longPollServer.Ts);
                    pts = longPollServer.Pts;

                    await _exceptionEvent.TriggerHandler(e, cancellationToken).ConfigureAwait(false);
                }
                catch (System.Exception e)
                {
                    await _exceptionEvent.TriggerHandler(e, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private static VkMessageType DetectMessageType(
            IReadOnlyCollection<Message> forwardMessages,
            IReadOnlyCollection<Attachment> attachments,
            MessageActionObject actionObject,
            Message replyMessage)
        {
            if (actionObject != null)
            {
                if (actionObject.Type == MessageAction.ChatCreate) return VkMessageType.ChatCreate;
                if (actionObject.Type == MessageAction.ChatInviteUser) return VkMessageType.ChatInviteUser;
                if (actionObject.Type == MessageAction.ChatKickUser) return VkMessageType.ChatKickUser;
                if (actionObject.Type == MessageAction.ChatPhotoRemove) return VkMessageType.ChatPhotoRemove;
                if (actionObject.Type == MessageAction.ChatPhotoUpdate) return VkMessageType.ChatPhotoUpdate;
                if (actionObject.Type == MessageAction.ChatPinMessage) return VkMessageType.ChatPinMessage;
                if (actionObject.Type == MessageAction.ChatTitleUpdate) return VkMessageType.ChatTitleUpdate;
                if (actionObject.Type == MessageAction.ChatUnpinMessage) return VkMessageType.ChatUnpinMessage;
                if (actionObject.Type == MessageAction.ChatInviteUserByLink) return VkMessageType.ChatInviteUserByLink;
                            
                throw new ArgumentException("action type not found");
            }
                        
            if (forwardMessages?.Count > 0) return VkMessageType.Forward;
            if (replyMessage != null) return VkMessageType.Reply;
            if (attachments.Any(x => x.Type == typeof(Sticker))) return VkMessageType.Sticker;
            if (attachments.Any(x => x.Type == typeof(Photo))) return VkMessageType.Photo;
            if (attachments.Any(x => x.Type == typeof(AudioMessage))) return VkMessageType.Voice;

            return VkMessageType.Message;
        }
        
        private ICommandHandler<MessageToProcess> CreateCommandHandler(VkMessageType vkMessageType)
        {
            if(vkMessageType == VkMessageType.Reply) return new ReplyCommandHandler(_replyCommands);
            if(vkMessageType == VkMessageType.Sticker) return new StickerCommandHandler(_stickerCommands);
            
            return new TextCommandHandler(_textCommands);
        }
        
        /// <summary>
        ///     Get data for the connection.
        /// </summary>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Returns data for the connection to long poll</returns>
        private async Task<LongPollServerResponse> GetLongPollServerAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await _botClient.Messages.GetLongPollServerAsync(_longPollConfiguration.NeedPts, _longPollConfiguration.LpVersion).ConfigureAwait(false);
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
            return await _botClient.Messages.GetLongPollHistoryAsync(new MessagesGetLongPollHistoryParams
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
            }).ConfigureAwait(false);
        }

        /// <summary>
        ///     Sends a private message.
        /// </summary>
        /// <param name="peerId">
        ///     The ID of the destination. For a group conversation: 2000000000 + conversation id. For
        ///     communities: - community id.
        /// </param>
        /// <param name="message">
        ///     Private message text (required if the parameter is not set attachment)
        /// </param>
        private async Task SendAsync(long? peerId, string message)
        {
            await _botClient.Messages.SendAsync(new MessagesSendParams
            {
                PeerId = peerId,
                Message = message,
                RandomId = GetRandomId()
            }).ConfigureAwait(false);
        }
        
        /// <summary>
        ///     Returns unique identifier, used to prevent re-sending same message.
        ///     Is saved with the message and is available in the message history.
        /// </summary>
        /// <code>
        ///    await _vkApi.Messages.SendAsync(new MessagesSendParams{ RandomId = GetRandomId() });
        /// </code>
        private int GetRandomId()
        {
            var intBytes = new byte[4];
            _rng.GetBytes(intBytes);

            return BitConverter.ToInt32(intBytes, 0);
        }

        /// <inheritdoc cref="IDisposable" />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}