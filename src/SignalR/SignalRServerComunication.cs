using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace SignalRServer.SignalR
{
    public class SignalRServerComunication
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public SignalRServerComunication(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task Send(Int64 contador)
        {
            await _hubContext.Clients.All.SendAsync("Users", contador);
        }
    }
}