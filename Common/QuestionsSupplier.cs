using System;
using System.Collections.Generic;
using System.IO;
using Azure.Storage.Blobs;
using Newtonsoft.Json;
using stanbots.Models;

namespace stanbots.Common;

public sealed class QuestionsSupplier
{
    private static readonly Lazy<QuestionsSupplier> instance =
        new Lazy<QuestionsSupplier>(() => new QuestionsSupplier());

    public static QuestionsSupplier Instance => instance.Value;

    private readonly List<JoinRequestVerifySingleAnswerQuestion> _questions;

    private QuestionsSupplier()
    {
        /*string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        // Create a BlobServiceClient using the connection string
        var blobServiceClient = new BlobServiceClient(connectionString);

        // Get a BlobContainerClient for your container
        var containerClient = blobServiceClient.GetBlobContainerClient("function-settings");

        // Get a BlobClient for your blob
        var blobClient = containerClient.GetBlobClient("questions.json");

        // Read the blob content
        using var reader = new StreamReader(blobClient.OpenRead());
        string blobContent = reader.ReadToEnd();*/


        _questions =
            JsonConvert.DeserializeObject<List<JoinRequestVerifySingleAnswerQuestion>>(QuestionsTextJson);
    }

    public List<JoinRequestVerifySingleAnswerQuestion> GetQuestions()
    {
        return _questions;
    }

    public static string QuestionsTextJson = @"
    [
        {
            ""question"": ""Оберіть коректне завершення фрази(прислівки): 'Путін....!'?"",
            ""answers"": [""Наш президент"", ""Гарний парубок"", ""Хуйло"", ""Хуйло ла-ла-ла-ла""],
            ""correctanswer"": ""Хуйло""
        },
        {
            ""question"": ""Чий Крим?"",
            ""answers"": [""Российский"", ""Український"", ""Не все так однозначно"", ""Турецкий""],
            ""correctanswer"": ""Український""
        },
        {
            ""question"": ""Оберіть коректне завершення фрази: 'Российский военный корабль...'!"",
            ""answers"": [""Москва"", ""Купить"", ""Иди на хуй"", ""Самый большой""],
            ""correctanswer"": ""Иди на хуй""
        },
        {
            ""question"": ""Хто цяпер з'яўляецца законным прэзідэнтам Беларусі?"",
            ""answers"": [""Путін"", ""Лукашенко"", ""Тихановская"", ""Мяшок бульбяны""],
            ""correctanswer"": ""Тихановская""
        },
        {
            ""question"": ""The largest city in the south of Ukraine?"",
            ""answers"": [""Odessa"", ""Dnipro"", ""Kherson"", ""Odesa""],
            ""correctanswer"": ""Odesa""
        }
    ]
    ";
}