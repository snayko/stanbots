using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace stanbots.Services
{
    public class TelegramUpdateService
    {
        private readonly ITelegramBotClient _botClient;

        public TelegramUpdateService(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task EchoAsync(Update update)
        {
            if (update is null)
                return;

            if (!(update.Message is { } message)) return;

            await _botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: $"Echo : {message.Text}");
        }
    }
}

