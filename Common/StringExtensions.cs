using Telegram.Bot.Types;

namespace stanbots.Common;

public static class StringExtensions
{
    public static string GetFullName(this User user)
    {
        return user.FirstName + " " + user.LastName;
    }
}