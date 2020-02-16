using VkNet.Enums.Filters;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace VkNet.FluentCommands.UserBot
{
    /// <summary>
    ///    Long poll configuration
    /// </summary>
    public class UserLongPollConfiguration
    {
        /// <summary>
        ///     <c>true</c> - return the pts field required for the method to work messages.getLongPollHistory.
        /// </summary>
        public bool NeedPts { get; set; } = true;

        /// <summary>
        ///     Version for connecting to Long Poll. Actual version: 3.
        /// </summary>
        public uint LpVersion { get; set; } = 3U;

        /// <summary>
        ///    List of additional fields to return.
        /// </summary>
        public UsersFields Fields { get; set; }

        /// <summary>
        ///     The number of characters you want to trim the message. Enter 0 if You
        ///     don't want to crop the message. (across
        ///     by default, messages are not truncated).
        /// </summary>
        public long? PreviewLength { get; set; }
        
        /// <summary>
        ///     If you pass a value of 1 to this parameter, the history will be returned only from those
        ///     users who are currently
        ///     online. the flag can take the values 1 or 0.
        /// </summary>
        public bool? Onlines { get; set; }
        
        /// <summary>
        ///     If the number of events in the history exceeds this value, it will be returned error.
        ///     Positive number. The default is 1000. The minimum value is 1000.
        /// </summary>
        public long? EventsLimit { get; set; }

        /// <summary>
        ///     The number of messages to return.
        /// </summary>
        public long? MsgsLimit { get; set; } = 200;
        
        /// <summary>
        ///     Maximum message ID among those already available in the local copy.
        ///     It is necessary to consider how the messages,
        ///     received via API methods (for example, messages.getDialogs,
        ///     messages.getHistory), and data obtained from Long.
        /// </summary>
        public long? MaxMsgId { get; set; }
    }
}