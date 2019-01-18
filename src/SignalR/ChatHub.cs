using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace SignalRServer.SignalR
{
    public class ChatHub : Hub
    {
        public static Int64 contador = 0;

        private ISubscriber _subscriber;

        public ChatHub(ISubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        public async Task Send(string user, string message)
        {
            Console.WriteLine($"{user}: {message}");
            await Clients.AllExcept(new[] { this.Context.ConnectionId }).SendAsync("Receive", user, message);
        }

        public override async Task OnConnectedAsync()
        {
            await _subscriber.PublishAsync("CountUsers", 1);
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Receive", "admin", "Welcome! podName: " + Environment.MachineName);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await _subscriber.PublishAsync("CountUsers", -1);
            await base.OnDisconnectedAsync(exception);
        }
    }
}