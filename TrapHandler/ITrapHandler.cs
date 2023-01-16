using Discord.WebSocket;

namespace Overtime.TrapHandler
{
    public interface ITrapHandler
    {
        /// <summary>
        /// A handler for the message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task Trap(SocketMessage message);
    }
}