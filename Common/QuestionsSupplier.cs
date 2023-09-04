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
        string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        // Create a BlobServiceClient using the connection string
        var blobServiceClient = new BlobServiceClient(connectionString);

        // Get a BlobContainerClient for your container
        var containerClient = blobServiceClient.GetBlobContainerClient("function-settings");

        // Get a BlobClient for your blob
        var blobClient = containerClient.GetBlobClient("questions.json");

        // Read the blob content
        using var reader = new StreamReader(blobClient.OpenRead());
        string blobContent = reader.ReadToEnd();

        _questions =
            JsonConvert.DeserializeObject<List<JoinRequestVerifySingleAnswerQuestion>>(blobContent);
    }

    public List<JoinRequestVerifySingleAnswerQuestion> GetQuestions()
    {
        return _questions;
    }
}