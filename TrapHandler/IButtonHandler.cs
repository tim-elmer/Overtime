using Discord.WebSocket;

namespace Overtime.TrapHandler;

public interface IButtonHandler
{
    public List<string> ButtonIdList { get; }
    public Task OnButtonExecuted(SocketMessageComponent messageComponent);
}