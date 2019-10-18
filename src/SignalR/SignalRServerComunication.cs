using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chat.Backend.SignalR
{
    public class SignalRComunication
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public SignalRComunication(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Send(Int64 counter)
        {
            await _hubContext.Clients.All.SendAsync("Users", counter);
        }
    }
}