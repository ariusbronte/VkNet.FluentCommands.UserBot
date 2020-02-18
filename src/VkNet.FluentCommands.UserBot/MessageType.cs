﻿﻿namespace VkNet.FluentCommands.UserBot
{
    /// <summary>
    ///     The type of incoming message.
    /// </summary>
    internal enum MessageType
    {
        /// <summary>
        ///     Unknown type.
        /// </summary>
        None,
        
        /// <summary>
        ///     Text message type.
        /// </summary>
        Message,
        
        /// <summary>
        ///     Sticker message type.
        /// </summary>
        Sticker
    }
}