using VkNet.Enums;
using VkNet.Enums.Filters;
// ReSharper disable UnusedMember.Global

namespace VkNet.FluentCommands.UserBot.Abstractions
{
    /// <summary>
    ///    Long poll configuration
    /// </summary>
    public interface IUserLongPollConfiguration
    {
        /// <summary>
        ///     <c>true</c> - return the pts field required for the method to work messages.getLongPollHistory.
        /// </summary>
        bool NeedPts { get; set; }

        /// <summary>
        ///     Version for connecting to Long Poll. Actual version: 3.
        /// </summary>
        uint LpVersion { get; set; }

        /// <summary>
        ///    List of additional fields to return.
        /// </summary>
        UsersFields Fields { get; set; }

        /// <summary>
        ///     The number of characters you want to trim the message. Enter 0 if You
        ///     don't want to crop the message. (across
        ///     by default, messages are not truncated).
        /// </summary>
        long? PreviewLength { get; set; }

        /// <summary>
        ///     If you pass a value of 1 to this parameter, the history will be returned only from those
        ///     users who are currently
        ///     online. the flag can take the values 1 or 0.
        /// </summary>
        bool? Onlines { get; set; }

        /// <summary>
        ///     If the number of events in the history exceeds this value, it will be returned error.
        ///     Positive number. The default is 1000. The minimum value is 1000.
        /// </summary>
        long? EventsLimit { get; set; }

        /// <summary>
        ///     The number of messages to return.
        /// </summary>
        long? MsgsLimit { get; set; }

        /// <summary>
        ///     Maximum message ID among those already available in the local copy.
        ///     It is necessary to consider how the messages,
        ///     received via API methods (for example, messages.getDialogs,
        ///     messages.getHistory), and data obtained from Long.
        /// </summary>
        long? MaxMsgId { get; set; }

        /// <summary>
        ///     Set <c>Received</c> if you only want to receive incoming messages.
        ///     Set <c>Sended</c> if you want to receive only sent messages.
        /// </summary>
        MessageType? MessageType { get; set; }
    }
}