using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace SignalRServer
{
    class ChatHub : Hub
    {
        private static int contador;

        public async Task Send(string user, string message)
        {
            Console.WriteLine($"{user}: {message}");
            await Clients.AllExcept(new[] { this.Context.ConnectionId }).SendAsync("Receive", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            contador++;
            Console.WriteLine(string.Format("{0} usuários online.", contador));
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Receive", "admin", "podName: " + Environment.MachineName);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            contador--;
            Console.WriteLine(string.Format("{0} usuários online.", contador));
            await base.OnDisconnectedAsync(exception);
        }
    }
}