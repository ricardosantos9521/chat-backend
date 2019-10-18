using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Chat.Backend.SignalR
{
    public class ChatHub : Hub
    {
        public static Int64 countUsers = 0;
        public static Int64 countGlobalUsers = 0;

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
            countUsers++;
            await base.OnConnectedAsync();
            await Clients.Caller.SendAsync("Receive", "admin", "Welcome! podName: " + Environment.MachineName);
            await Clients.All.SendAsync("Users", countGlobalUsers + 1);
            await _subscriber.PublishAsync("CountUsers", 1);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            countUsers--;
            await base.OnDisconnectedAsync(exception);
            await Clients.All.SendAsync("Users", countGlobalUsers - 1);
            await _subscriber.PublishAsync("CountUsers", -1);
        }
    }
}