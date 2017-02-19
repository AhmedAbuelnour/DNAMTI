using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Hubs;

namespace SequenceAlignment.Hubs
{
    [HubName("GridHub")]
    public class GridHub : Hub
    {
        // Alignment Method for sender (Invoke)
        public void SendToGrid(string JobJson)
        {
            // Result Method for listner (ON)
            Clients.All.ListenToGrid(JobJson);
        }
    }
}
