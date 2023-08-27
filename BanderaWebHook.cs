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
        private readonly TelegramUpdateService _updateService;

        public BanderaWebHook(TelegramUpdateService updateService)
        {
            _updateService = updateService;
        }

        [FunctionName("BanderaWebHook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request)
        {
            try
            {
                var body = await request.ReadAsStringAsync();
                var update = JsonConvert.DeserializeObject<Update>(body);
                if (update is null)
                {
                }

                await _updateService.EchoAsync(update);
            }
            catch (Exception e)
            {
            }

            return new OkResult();
        }
    }
}
