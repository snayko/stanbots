using Telegram.Bot.Types;

namespace stanbots.Models;

public class ChatJoinRequestContext
{
    public ChatJoinRequest JoinRequest { get; set; }
    public JoinRequestVerifySingleAnswerQuestion Question { get; set; }
}