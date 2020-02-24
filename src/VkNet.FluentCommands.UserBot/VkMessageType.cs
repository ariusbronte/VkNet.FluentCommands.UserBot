﻿﻿namespace VkNet.FluentCommands.UserBot
{
    /// <summary>
    ///     The type of incoming message.
    /// </summary>
    internal enum VkMessageType
    {
        /// <summary>
        ///     Text message type.
        /// </summary>
        Message,
        
        /// <summary>
        ///     Reply message type.
        /// </summary>
        Reply,

        /// <summary>
        ///     Forward message type.
        /// </summary>
        Forward,
        
        /// <summary>
        ///     Sticker message type.
        /// </summary>
        Sticker,
        
        /// <summary>
        ///     Photo message type.
        /// </summary>
        Photo,
        
        /// <summary>
        ///     Voice message type.
        /// </summary>
        Voice,
        
        /// <summary>
        ///     User created chat action.
        /// </summary>
        ChatCreate,
        
        /// <summary>
        ///     User invited action.
        /// </summary>
        ChatInviteUser,
        
        /// <summary>
        ///     User kicked action.
        /// </summary>
        ChatKickUser,
        
        /// <summary>
        ///     Photo removed action.
        /// </summary>
        ChatPhotoRemove,
        
        /// <summary>
        ///     Photo updated action.
        /// </summary>
        ChatPhotoUpdate,
        
        /// <summary>
        ///     Message pinned action.
        /// </summary>
        ChatPinMessage,
        
        /// <summary>
        ///     Title updated action.
        /// </summary>
        ChatTitleUpdate,
        
        /// <summary>
        ///     Message unpinned action.
        /// </summary>
        ChatUnpinMessage,
        
        /// <summary>
        ///     User invited by link action.
        /// </summary>
        ChatInviteUserByLink
    }
}