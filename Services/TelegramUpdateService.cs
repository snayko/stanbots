using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace stanbots.Services
{
    public class TelegramUpdateService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramUpdateService> _logger;

        public const string VerifyChatMemberMessageQuestion = "Ви можете закінчити вираз 'Путін....!' (слово з 5-и букв)?";
        public const string VerifyChatMemberMessageResponse = "хуйло";
        public const string RefuseJoinRequestsToBotsMessageResponse = "Ботам тут не раді!";

        public const string JoinRequestsWelcomeMessage = "Ласкаво просимо!";
        public const string JoinRequestsRefuseMessage = "Вибачте, це приватна група, і ви не пройшли перевірку!";

        //Add real db storage cause it's not reliable storage
        static Dictionary<long, ChatJoinRequest> _pendingJoinRequests = new Dictionary<long, ChatJoinRequest>();

        public TelegramUpdateService(ITelegramBotClient botClient, ILogger<TelegramUpdateService> logger)
        {
            _botClient = botClient;
            _logger = logger;
        }

        public async Task ProcessUpdateMessage(Update update, CancellationToken cancellationToken)
        {
            var handler = update switch
            {
                { ChatJoinRequest: { } joinRequest } => BotOnJoinReceived(joinRequest, cancellationToken),
                { Message: { } message } => BotOnReplyToMessageAboutJoinRequest(message, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update, cancellationToken)
            };

            await handler;
        }

        async Task BotOnJoinReceived(ChatJoinRequest request, CancellationToken cancellationToken)
        {
            var user = request.From;

            if (!user.IsBot)
            {
                await _botClient.SendTextMessageAsync(request.UserChatId, VerifyChatMemberMessageQuestion);
                _pendingJoinRequests[user.Id] = request;
            }
            else
            {
                await _botClient.SendTextMessageAsync(request.UserChatId, RefuseJoinRequestsToBotsMessageResponse);
                await _botClient.DeclineChatJoinRequest(request.UserChatId, user.Id, cancellationToken);
            }
        }

        async Task BotOnReplyToMessageAboutJoinRequest(Message message, CancellationToken cancellationToken)
        {
            var userId = message.From.Id;
            
            if (_pendingJoinRequests.TryGetValue(userId, out var pendingRequest))
            {
                string response = message.Text;
                if (!string.IsNullOrWhiteSpace(response)
                    && response.Trim().Contains(VerifyChatMemberMessageResponse, System.StringComparison.OrdinalIgnoreCase))
                {
                    await _botClient.SendTextMessageAsync(pendingRequest.UserChatId, JoinRequestsWelcomeMessage);
                    await _botClient.ApproveChatJoinRequest(pendingRequest.Chat.Id, userId, cancellationToken);
                }
                else
                {
                    await _botClient.SendTextMessageAsync(pendingRequest.UserChatId, JoinRequestsRefuseMessage);
                    await _botClient.DeclineChatJoinRequest(pendingRequest.Chat.Id, userId, cancellationToken);
                }

                // Remove the pending request
                _pendingJoinRequests.Remove(userId);
            }
        }

        async Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Unrecognized message: {0}", JsonConvert.SerializeObject(update));
            await Task.CompletedTask;
        }
    }
}

