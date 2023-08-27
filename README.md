# Telegram bot api usage example (WebHook by using Azure Functions)

Simple verification process for new chat joiners

## Build Status (GitHub Actions)

| Image | Status | 
| ------------- | ------------- |
| AzureWebHook Status |  [![AzureWebHook Status](https://github.com/snayko/stanbots/actions/workflows/master_stanbots.yml/badge.svg)](https://github.com/snayko/stanbots/actions/workflows/master_stanbots.yml) |


## Setup Instructions

1. Register your bot via [BotFather](https://core.telegram.org/bots#botfather) on Telegram and obtain the API token.

2. Add your bot (which should be able to aprove and decline join requests) to your private group.

3. Prepare your function app (MyAzureWebHookFunction) on https://portal.azure.com/

3. Register WebHook by sending a POST request using `curl`:

   ```bash
   curl --request POST \
     --url https://api.telegram.org/bot{myBotToken}/setWebhook \
     --header 'content-type: application/json' \
     --data '{"url": "https://myazurefunctionwebapp.azurewebsites.net/api/MyAzureWebHookFunction"}'

4. For testing purposes you can get temporary forwarding to your local machine:

   Register your domain on NGROK (e.g. yourcustomdomain.ngrok-free.app)

   Use commands to run function locally and run ngrok locally and forward from telegram

   ```bash
   func start --build \
                      \
   ngrok http 7071 --domain=yourcustomdomain.ngrok-free.app \
                      \
   curl --request POST --url https://api.telegram.org/bot5704082266:AAHW2EBPv3LhDOERPTjrrvHZ3YnaPXDvUfA/setWebhook \
   --header 'content-type: application/json' --data '{"url": "yourcustomdomain.ngrok-free.app/api/MyAzureWebHookFunction"}'

