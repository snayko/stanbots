using System;
using System.Collections.Generic;
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
using Microsoft.ApplicationInsights;
using stanbots.Common;

namespace stanbots
{
    public class BanderaWebHook
    {
        private readonly TelegramUpdateService _updateService;
        private readonly ILogger<BanderaWebHook> _logger;

        private readonly TelemetryClient _telemetryClient;

        public BanderaWebHook(TelegramUpdateService updateService, ILogger<BanderaWebHook> logger, TelemetryClient telemetryClient)
        {
            _updateService = updateService;
            _logger = logger;
            _telemetryClient = telemetryClient;
        }

        [FunctionName("BanderaWebHook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request, CancellationToken cancellationToken)
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
                
                return new OkResult();
            }
            catch (Exception e)
            {
                _logger.LogError("Exception: " + e.Message);
                
                var properties = new Dictionary<string, string>
                {
                    { "exceptionType", e.GetType().Name },
                    { "exceptionMessage", e.Message }
                };
                _telemetryClient.TrackEvent(AzureEvents.RequestError, properties);
            }

            return new BadRequestResult();
        }
    }
}
