using Microsoft.AspNetCore.SignalR;

namespace DealtHands.SignalR_Hub
{
    public class RefreshHub : Hub
    {
        /// <summary>
        /// Called when a client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Called by clients to notify when they join a game session
        /// </summary>
        public async Task JoinGameSession(long gameSessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session-{gameSessionId}");
            Console.WriteLine($"Client {Context.ConnectionId} joined session group: session-{gameSessionId}");
        }

        /// <summary>
        /// Called by clients to notify when they leave a game session
        /// </summary>
        public async Task LeaveGameSession(long gameSessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session-{gameSessionId}");
            Console.WriteLine($"Client {Context.ConnectionId} left session group: session-{gameSessionId}");
        }

        /// <summary>
        /// Notify all clients in a session when a player makes a decision
        /// </summary>
        public async Task NotifyPlayerDecision(long gameSessionId, string playerName, string decision)
        {
            await Clients.Group($"session-{gameSessionId}")
                .SendAsync("PlayerDecisionUpdated", playerName, decision);
        }

        /// <summary>
        /// Notify all clients in a session when a round changes
        /// </summary>
        public async Task NotifyRoundChanged(long gameSessionId, long newRoundId)
        {
            await Clients.Group($"session-{gameSessionId}")
                .SendAsync("RoundChanged", newRoundId);
        }

        /// <summary>
        /// Notify all clients in a session when the player list updates
        /// </summary>
        public async Task NotifyPlayersUpdated(long gameSessionId)
        {
            await Clients.Group($"session-{gameSessionId}")
                .SendAsync("PlayersUpdated");
        }

        /// <summary>
        /// Notify all clients when a game is completed
        /// </summary>
        public async Task NotifyGameCompleted(long gameSessionId)
        {
            await Clients.Group($"session-{gameSessionId}")
                .SendAsync("GameCompleted");
        }
    }
}
