using DealtHands.Data;
using DealtHands.ModelsV2;
using Microsoft.EntityFrameworkCore;

namespace DealtHands.Services
{
    /// <summary>
    /// Service for managing users (educators and students) in the V2 schema.
    /// Handles registration, authentication, and user management.
    /// </summary>
    public class UserService
    {
        private readonly DealtHandsDbv2Context _context;

        public UserService(DealtHandsDbv2Context context)
        {
            _context = context;
        }

        #region Educator/Host Management

        /// <summary>
        /// Registers a new educator account.
        /// </summary>
        /// <param name="username">Unique username</param>
        /// <param name="email">Unique email address</param>
        /// <param name="password">Plain text password (will be hashed)</param>
        /// <returns>The created User object, or null if username/email already exists</returns>
        public async Task<User?> RegisterEducatorAsync(string username, string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
                return null;

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsEducator = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Authenticates an educator by email and password.
        /// </summary>
        /// <returns>User object if authentication successful, null otherwise</returns>
        public async Task<User?> AuthenticateEducatorAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null || user.PasswordHash == null)
                return null;

            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }

        /// <summary>
        /// Gets a user by their UserId.
        /// </summary>
        public async Task<User?> GetUserByIdAsync(long userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        /// <summary>
        /// Gets all sessions hosted by an educator.
        /// </summary>
        public async Task<List<GameSession>> GetEducatorSessionsAsync(long userId)
        {
            return await _context.GameSessions
                .Include(s => s.Game)
                .Where(s => s.HostUserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        #endregion

        #region Student/Player Management

        /// <summary>
        /// Creates or retrieves a student user account.
        /// Students are created on-the-fly when they join a session.
        /// They don't have passwords - they use session join codes.
        /// </summary>
        /// <param name="username">Display name for the student</param>
        /// <returns>User object for the student</returns>
        public async Task<User> CreateOrGetStudentAsync(string username)
        {
            var baseUsername = SanitizeUsername(username);
            var finalUsername = baseUsername;
            int counter = 1;

            while (await _context.Users.AnyAsync(u => u.Username == finalUsername))
            {
                finalUsername = $"{baseUsername}{counter}";
                counter++;
            }

            var email = $"{finalUsername.ToLower()}@student.dealthands.local";

            var student = new User
            {
                Username = finalUsername,
                Email = email,
                PasswordHash = null, // Students don't have passwords
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(student);
            await _context.SaveChangesAsync();

            return student;
        }

        /// <summary>
        /// Gets all students who have participated in a session.
        /// </summary>
        public async Task<List<User>> GetStudentsInSessionAsync(long gameSessionId)
        {
            return await _context.Ugcs
                .Where(u => u.GameSessionId == gameSessionId)
                .Select(u => u.User)
                .Distinct()
                .ToListAsync();
        }

        #endregion

        #region Password Management

        /// <summary>
        /// Generates a password reset token for the given email address.
        /// Token expires after 1 hour.
        /// </summary>
        /// <returns>The reset token, or null if email not found</returns>
        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            string token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.PasswordResetExpires = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            return token;
        }

        /// <summary>
        /// Resets a user's password using a valid reset token.
        /// </summary>
        /// <returns>True if the reset succeeded, false if token is invalid or expired</returns>
        public async Task<bool> ResetPasswordWithTokenAsync(string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == token &&
                u.PasswordResetExpires > DateTime.UtcNow);

            if (user == null) return false;

            user.PasswordHash = HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetExpires = null;
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Updates a user's password after verifying the old password.
        /// </summary>
        public async Task<bool> UpdatePasswordAsync(long userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.PasswordHash == null)
                return false;

            if (!VerifyPassword(oldPassword, user.PasswordHash))
                return false;

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Hashes a password using BCrypt.
        /// </summary>
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifies a plain text password against a BCrypt hash.
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        /// <summary>
        /// Sanitizes a username by removing special characters.
        /// </summary>
        private string SanitizeUsername(string username)
        {
            var sanitized = new string(username
                .Where(c => char.IsLetterOrDigit(c) || c == '_')
                .ToArray());

            if (string.IsNullOrWhiteSpace(sanitized))
                sanitized = "Player";

            if (sanitized.Length > 40)
                sanitized = sanitized.Substring(0, 40);

            return sanitized;
        }

        #endregion
    }
}
