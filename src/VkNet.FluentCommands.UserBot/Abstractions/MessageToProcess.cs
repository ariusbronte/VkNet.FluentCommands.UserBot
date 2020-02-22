using VkNet.Abstractions;
using VkNet.Model;

namespace VkNet.FluentCommands.UserBot.Abstractions
{
    internal class MessageToProcess
    {
        public IVkApi BotClient { get; }
        
        public Message Message { get; }

        public MessageToProcess(IVkApi botClient, Message message)
        {
            BotClient = botClient;
            Message = message;
        }
    }
}