using VkNet.Enums;
using VkNet.Enums.Filters;
using VkNet.FluentCommands.UserBot.Abstractions;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace VkNet.FluentCommands.UserBot
{
    /// <inheritdoc />
    public class UserLongPollConfiguration : IUserLongPollConfiguration
    {
        /// <inheritdoc />
        public bool NeedPts { get; set; } = true;

        /// <inheritdoc />
        public uint LpVersion { get; set; } = 3U;

        /// <inheritdoc />
        public UsersFields Fields { get; set; }

        /// <inheritdoc />
        public long? PreviewLength { get; set; }
        
        /// <inheritdoc />
        public bool? Onlines { get; set; }
        
        /// <inheritdoc />
        public long? EventsLimit { get; set; }
        
        /// <inheritdoc />
        public long? MsgsLimit { get; set; } = 200;
        
        /// <inheritdoc />
        public long? MaxMsgId { get; set; }

        /// <inheritdoc />
        public MessageType MessageType { get; set; } = MessageType.Received;
        
        /// <inheritdoc />
        public static UserLongPollConfiguration Default => new UserLongPollConfiguration();
    }
}