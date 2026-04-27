namespace DealtHands.Services
{
    /// <summary>
    /// In-memory tracker for players connected to each session.
    /// Registered as a singleton so state persists across requests.
    /// </summary>
    public class SessionTracker
    {
        private readonly Dictionary<long, HashSet<long>> _sessions = new();
        private readonly object _lock = new();

        /// <summary>
        /// Registers a player as connected to a session.
        /// </summary>
        public void AddPlayer(long gameSessionId, long userId)
        {
            lock (_lock)
            {
                if (!_sessions.ContainsKey(gameSessionId))
                    _sessions[gameSessionId] = new HashSet<long>();

                _sessions[gameSessionId].Add(userId);
            }
        }

        /// <summary>
        /// Returns all user IDs currently connected to a session.
        /// </summary>
        public List<long> GetPlayers(long gameSessionId)
        {
            lock (_lock)
            {
                return _sessions.TryGetValue(gameSessionId, out var players)
                    ? players.ToList()
                    : new List<long>();
            }
        }

        /// <summary>
        /// Removes a player from a session (e.g., on disconnect).
        /// </summary>
        public void RemovePlayer(long gameSessionId, long userId)
        {
            lock (_lock)
            {
                if (_sessions.ContainsKey(gameSessionId))
                    _sessions[gameSessionId].Remove(userId);
            }
        }

        /// <summary>
        /// Returns how many players are currently connected to a session.
        /// </summary>
        public int GetPlayerCount(long gameSessionId)
        {
            lock (_lock)
            {
                return _sessions.TryGetValue(gameSessionId, out var players)
                    ? players.Count
                    : 0;
            }
        }
    }
}
