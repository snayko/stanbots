using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using stanbots.Common;
using stanbots.Models;
using Telegram.Bot.Types.ReplyMarkups;

namespace stanbots.Services
{
    public class TelegramUpdateService
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramUpdateService> _logger;

        public const string JoinRequestsWelcomeMessage = "Ласкаво просимо!";

        public const string JoinRequestsRefuseMessageWrongAnswer = "Відхилено запит на вступ до групи для: {0}, мова: {1}, бот: {2}, username: {3}, питання: {4}, відповідь: {5} - москалику спалився!";
        public const string JoinRequestsRefuseMessageTimeout = "Відхилено запит на вступ до групи для: {0}, мова: {1}, бот: {2} питання: {3} за таймаутом!";

        public const string JoinRequestsRefuseToBotMessageTimeout =
            "Відхилено запит на вступ до групи для бота: {0}, username: {1}, мова: {2}!";
        
        //Add real db storage cause it's not reliable storage
        static ConcurrentDictionary<long, ChatJoinRequestContext> _pendingJoinRequests = new ConcurrentDictionary<long, ChatJoinRequestContext>();

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
            var chatId = request.Chat.Id;
            var userId = user.Id;

            if (!user.IsBot)
            {
                var questions = QuestionsSupplier.Instance.GetQuestions();
                
                // Randomly select a question
                var random = new Random();
                var selectedQuestion = questions[random.Next(questions.Count)];

                // Extract the question and answers
                var questionText = selectedQuestion.Question;
                // Shuffle the answer options for randomness
                var answerOptions = selectedQuestion.Answers.OrderBy(x => random.Next()).ToList();
                
                // Send a choice question with predefined answers as buttons to the group chat
                // Create a ReplyKeyboardMarkup with the shuffled answer options
                var keyboard = new ReplyKeyboardMarkup(answerOptions.Select(option => new KeyboardButton(option)));

                // Send the question to the group chat
                await _botClient.SendTextMessageAsync(chatId, questionText, replyMarkup: keyboard);

                // Start a timer for 2 minutes to wait for the user's response
                var timer = new System.Timers.Timer(120000);

                timer.Elapsed += async (sender, e) =>
                {
                    if (_pendingJoinRequests.TryGetValue(userId, out var stillPendingRequest))
                    {
                        var userInReq = stillPendingRequest.JoinRequest.From;

                        await _botClient.DeclineChatJoinRequest(chatId, userId, cancellationToken);
                        await _botClient.SendTextMessageAsync(chatId,
                            string.Format(JoinRequestsRefuseMessageTimeout, userInReq.GetFullName(),
                                userInReq.LanguageCode, userInReq.IsBot, stillPendingRequest.Question));
                    }
                };
                
                timer.Start();

                _pendingJoinRequests[user.Id] = new ChatJoinRequestContext()
                    { JoinRequest = request, Question = selectedQuestion };
            }
            else
            {
                await _botClient.DeclineChatJoinRequest(chatId, userId, cancellationToken);
                await _botClient.SendTextMessageAsync(chatId,
                    string.Format(JoinRequestsRefuseToBotMessageTimeout, user.GetFullName(),
                        user.Username, user.LanguageCode));
            }
        }

        async Task BotOnReplyToMessageAboutJoinRequest(Message message, CancellationToken cancellationToken)
        {
            var userId = message.From.Id;
            
            if (_pendingJoinRequests.TryGetValue(userId, out var pendingRequest))
            {
                string response = message.Text;
                var req = pendingRequest.JoinRequest;
                var quest = pendingRequest.Question;
                    
                if (!string.IsNullOrWhiteSpace(response)
                    && response.Trim().Contains(quest.CorrectAnswer, System.StringComparison.OrdinalIgnoreCase))
                {
                    await _botClient.ApproveChatJoinRequest(req.Chat.Id, userId, cancellationToken);
                    await _botClient.SendTextMessageAsync(req.Chat.Id, JoinRequestsWelcomeMessage);
                    
                }
                else
                {
                    await _botClient.DeclineChatJoinRequest(req.Chat.Id, userId, cancellationToken);
                    await _botClient.SendTextMessageAsync(req.Chat.Id,
                        string.Format(JoinRequestsRefuseMessageWrongAnswer, req.From.GetFullName(),
                            req.From.LanguageCode, req.From.IsBot, req.From.Username, quest.Question, response));
                }

                // Remove the pending request
                _pendingJoinRequests.Remove(userId, out var removedItem);
            }
        }

        async Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
        {
            _logger.LogWarning("Unrecognized message: {0}", JsonConvert.SerializeObject(update));
            await Task.CompletedTask;
        }
    }
}

