using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;

namespace SequenceAlignment.Hubs
{
    [HubName("GridHub")]
    public class GridHub : Hub
    {
        public void Alignment(string Model)
        {
            Clients.All.Result(Model);
        }
    }
}
