# Telegram bot api usage example (WebHook by using Azure Functions)

Simple verification process for new chat joiners

## Build Status (GitHub Actions)

| Image | Status | 
| ------------- | ------------- |
| AzureWebHook Status |  [![AzureWebHook Status](https://github.com/snayko/stanbots/actions/workflows/master_stanbots.yml/badge.svg)](https://github.com/snayko/stanbots/actions/workflows/master_stanbots.yml) |


## Setup Instructions

1. Register your bot via [BotFather](https://core.telegram.org/bots#botfather) on Telegram and obtain the API token.

2. Register WebHook by sending a POST request using `curl`:

   ```bash
   curl --request POST \
     --url https://api.telegram.org/bot{myBotToken}/setWebhook \
     --header 'content-type: application/json' \
     --data '{"url": "https://myazurefunctionwebapp.azurewebsites.net/api/MyAzureWebHookFunction"}'

