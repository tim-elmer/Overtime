using Discord.WebSocket;

namespace Overtime.TrapHandler
{
    public interface IButtonHandler
    {
        public string ButtonId { get; }
        public Task OnButtonExecuted(SocketMessageComponent messageComponent);
    }
}