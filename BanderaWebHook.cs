using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using stanbots.Services;
using System.Threading;

namespace stanbots
{
    public class BanderaWebHook
    {
        private readonly TelegramUpdateService _updateService;
        private readonly ILogger<BanderaWebHook> _logger;

        public BanderaWebHook(TelegramUpdateService updateService, ILogger<BanderaWebHook> logger)
        {
            _updateService = updateService;
            _logger = logger;
        }

        [FunctionName("BanderaWebHook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var body = await request.ReadAsStringAsync();

                _logger.LogInformation("BanderaWebHook received message {0}:", body);
                
                var update = JsonConvert.DeserializeObject<Update>(body);
                if (update != null)
                {
                    await _updateService.ProcessUpdateMessage(update, cancellationToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Exception: " + e.Message);
            }

            return new OkResult();
        }
    }
}
