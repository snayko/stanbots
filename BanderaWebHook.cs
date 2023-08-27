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

namespace stanbots
{
    public class BanderaWebHook
    {
        private readonly ILogger _logger;
        private readonly TelegramUpdateService _updateService;

        public BanderaWebHook(ILogger logger, TelegramUpdateService updateService)
        {
            _logger = logger;
            _updateService = updateService;
        }

        [FunctionName("BanderaWebHook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest request)
        {
            try
            {
                var body = await request.ReadAsStringAsync();
                var update = JsonConvert.DeserializeObject<Update>(body);
                if (update is null)
                {
                    _logger.LogWarning("Unable to deserialize Update object.");
                }

                await _updateService.EchoAsync(update);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception: " + e.Message);
            }

            return new OkResult();
        }
    }
}
