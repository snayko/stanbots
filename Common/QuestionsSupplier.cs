using System;
using System.Collections.Generic;
using System.IO;
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
        _questions =
            JsonConvert.DeserializeObject<List<JoinRequestVerifySingleAnswerQuestion>>(
                File.ReadAllText("./Data/questions.json"));
    }

    public List<JoinRequestVerifySingleAnswerQuestion> GetQuestions()
    {
        return _questions;
    }
}