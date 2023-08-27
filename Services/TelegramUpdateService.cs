using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Collections.Generic;

namespace stanbots.Services
{
    public class TelegramUpdateService
    {
        private readonly ITelegramBotClient _botClient;

        public const string VerifyChatMemberMessageQuestion = "Ви можете закінчити вираз 'Путін....!' (слово з 5-и букв)?";
        public const string VerifyChatMemberMessageResponse = "хуйло";
        public const string RefuseJoinRequestsToBotsMessageResponse = "Bots are now welcome!";

        static Dictionary<long, ChatJoinRequest> _pendingJoinRequests = new Dictionary<long, ChatJoinRequest>();

        public TelegramUpdateService(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task ProcessUpdateMessage(Update update, CancellationToken cancellationToken)
        {
            var handler = update switch
            {
                { ChatJoinRequest: { } joinRequest } => BotOnJoinReceived(update, joinRequest, cancellationToken),
                { Message: { } message } => BotOnReplyToMessageAboutJoinRequest(update, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update, cancellationToken)
            };

            await handler;
        }

        async Task BotOnJoinReceived(Update update, ChatJoinRequest request, CancellationToken cancellationToken)
        {
            var user = request.From;
            if(!user.IsBot)
            {
                await _botClient.SendTextMessageAsync(request.UserChatId, VerifyChatMemberMessageQuestion);
                _pendingJoinRequests[user.Id] = request;
            }
            else
            {
                await _botClient.SendTextMessageAsync(request.UserChatId, RefuseJoinRequestsToBotsMessageResponse);
            }
        }

        async Task BotOnReplyToMessageAboutJoinRequest(Update update, CancellationToken cancellationToken)
        {
            var userId = update.Message.From.Id;

            if (_pendingJoinRequests.TryGetValue(userId, out var pendingRequest))
            {
                string response = update.Message.Text;
                if (!string.IsNullOrWhiteSpace(response)
                    && response.Trim().Equals(VerifyChatMemberMessageResponse, System.StringComparison.OrdinalIgnoreCase))
                {
                    await _botClient.ApproveChatJoinRequest(pendingRequest.Chat.Id, userId, cancellationToken);
                    await _botClient.SendTextMessageAsync(pendingRequest.UserChatId, "Your join request has been approved!");
                }
                else
                {
                    await _botClient.DeclineChatJoinRequest(pendingRequest.Chat.Id, userId, cancellationToken);
                    await _botClient.SendTextMessageAsync(pendingRequest.UserChatId, "Your join request has been declined.");
                }

                // Remove the pending request
                _pendingJoinRequests.Remove(userId);
            }
        }

        async Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}

