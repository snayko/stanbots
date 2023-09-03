using System.Collections.Generic;

namespace stanbots.Models;

public class JoinRequestVerifySingleAnswerQuestion
{
    /* example
    {
    "question": "Оберіть коректне завершення фрази(прислівки): 'Путін....!'?",
    "answers": ["Наш президент", "Гарний парубок", "Хуйло", "Хуйло ла-ла-ла-ла"],
    "correct_answer": "Хуйло"
    },
    */
    
    public string Question { get; set; }
    public List<string> Answers { get; set; }
    public string CorrectAnswer { get; set; }
}