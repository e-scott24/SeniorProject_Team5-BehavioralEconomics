using Microsoft.AspNetCore.Http;

namespace DealtHands.Services
{
    /// <summary>
    /// Interface for managing user authentication and session state.
    /// This abstraction layer provides a secure way to handle user sessions
    /// without directly manipulating HttpContext.Session throughout the application.
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Returns true if the current user is logged in as an Educator.
        /// </summary>
        bool IsEducator { get; }

        /// <summary>
        /// Returns true if the current user is logged in as a Student.
        /// </summary>
        bool IsStudent { get; }

        /// <summary>
        /// Returns the current user's UserId, or null if not authenticated.
        /// </summary>
        long? UserId { get; }

        /// <summary>
        /// Returns the current user's Username, or null if not authenticated.
        /// </summary>
        string? Username { get; }

        /// <summary>
        /// Returns the current student's GameSessionId, or null if not a student or not in a session.
        /// </summary>
        long? GameSessionId { get; }

        /// <summary>
        /// Returns the current session code (for educators managing a session).
        /// </summary>
        string? SessionCode { get; }

        /// <summary>
        /// Sets up an educator session after successful login.
        /// </summary>
        void SetEducatorSession(long userId, string username);

        /// <summary>
        /// Sets up a student session after joining a game.
        /// </summary>
        void SetStudentSession(long userId, string username, long gameSessionId);

        /// <summary>
        /// Sets the session code for an educator who is managing a game session.
        /// </summary>
        void SetSessionCode(string sessionCode);

        /// <summary>
        /// Clears all session data (logout).
        /// </summary>
        void ClearSession();
    }

    /// <summary>
    /// Implementation of authentication service that uses ASP.NET Core Session for state management.
    /// </summary>
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpContextAccessor _accessor;

        public AuthenticationService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public bool IsEducator => _accessor.HttpContext?.Session.GetString("Role") == "Educator";

        public bool IsStudent => _accessor.HttpContext?.Session.GetString("Role") == "Student";

        public long? UserId
        {
            get
            {
                var userIdStr = _accessor.HttpContext?.Session.GetString("UserId");
                return long.TryParse(userIdStr, out long id) ? id : null;
            }
        }

        public string? Username => _accessor.HttpContext?.Session.GetString("Username");

        public long? GameSessionId
        {
            get
            {
                var sessionIdStr = _accessor.HttpContext?.Session.GetString("GameSessionId");
                return long.TryParse(sessionIdStr, out long id) ? id : null;
            }
        }

        public string? SessionCode => _accessor.HttpContext?.Session.GetString("SessionCode");

        public void SetEducatorSession(long userId, string username)
        {
            var session = _accessor.HttpContext?.Session;
            if (session == null) return;

            session.Clear(); // Clear any existing session data
            session.SetString("UserId", userId.ToString());
            session.SetString("Username", username);
            session.SetString("Role", "Educator");
        }

        public void SetStudentSession(long userId, string username, long gameSessionId)
        {
            var session = _accessor.HttpContext?.Session;
            if (session == null) return;

            session.Clear(); // Clear any existing session data
            session.SetString("UserId", userId.ToString());
            session.SetString("Username", username);
            session.SetString("Role", "Student");
            session.SetString("GameSessionId", gameSessionId.ToString());
        }

        public void SetSessionCode(string sessionCode)
        {
            var session = _accessor.HttpContext?.Session;
            if (session == null) return;

            session.SetString("SessionCode", sessionCode);
        }

        public void ClearSession()
        {
            _accessor.HttpContext?.Session.Clear();
        }
    }
}