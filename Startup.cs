using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using stanbots.Services;
using Telegram.Bot;

[assembly: FunctionsStartup(typeof(stanbots.Startup))]

namespace stanbots
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Register ILogger<T> and ILoggerFactory
            builder.Services.AddLogging();

            var token = Environment
                .GetEnvironmentVariable("TelegramBotToken", EnvironmentVariableTarget.Process)
                    ?? throw new ArgumentException("Can not get token. Set token in environment setting");

            builder.Services.AddHttpClient("tgclient")
                .AddTypedClient<ITelegramBotClient>(httpClient
                    => new TelegramBotClient(token, httpClient));
            
            builder.Services.AddApplicationInsightsTelemetry();
            
            // Dummy business-logic service
            builder.Services.AddScoped<TelegramUpdateService>();
        }
    }
}