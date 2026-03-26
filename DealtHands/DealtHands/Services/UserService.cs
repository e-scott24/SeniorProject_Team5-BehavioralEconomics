using DealtHands.Data;
using DealtHands.ModelsV2;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
        /// <returns>The created User object</returns>
        public async Task<User?> RegisterEducatorAsync(string username, string email, string password)
        {
            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
            {
                return null; // User already exists
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Authenticates an educator by email and password.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="password">Plain text password</param>
        /// <returns>User object if authentication successful, null otherwise</returns>
        public async Task<User?> AuthenticateEducatorAsync(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null || user.PasswordHash == null)
                return null;

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash))
                return null;

            return user;
        }

        /// <summary>
        /// Gets an educator by their UserId.
        /// </summary>
        public async Task<User?> GetEducatorByIdAsync(long userId)
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
                .Where(s => s.HostUserId == userId && s.IsActive)
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
            // Generate a unique username if needed
            var baseUsername = SanitizeUsername(username);
            var finalUsername = baseUsername;
            int counter = 1;

            while (await _context.Users.AnyAsync(u => u.Username == finalUsername))
            {
                finalUsername = $"{baseUsername}{counter}";
                counter++;
            }

            // Create a unique email for the student (they won't use it for login)
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
        /// Updates a user's password.
        /// </summary>
        public async Task<bool> UpdatePasswordAsync(long userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.PasswordHash == null)
                return false;

            // Verify old password
            if (!VerifyPassword(oldPassword, user.PasswordHash))
                return false;

            // Update to new password
            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Resets a user's password (for password recovery).
        /// In a production system, you'd want to implement token-based reset.
        /// </summary>
        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false;

            user.PasswordHash = HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Hashes a password using SHA256.
        /// In production, use a more secure method like BCrypt or Argon2.
        /// </summary>
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// Verifies a password against a hash.
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }

        /// <summary>
        /// Sanitizes a username by removing special characters.
        /// </summary>
        private string SanitizeUsername(string username)
        {
            // Remove any characters that aren't letters, numbers, or underscores
            var sanitized = new string(username
                .Where(c => char.IsLetterOrDigit(c) || c == '_')
                .ToArray());

            // Ensure it's not empty and not too long
            if (string.IsNullOrWhiteSpace(sanitized))
                sanitized = "Player";

            if (sanitized.Length > 40)
                sanitized = sanitized.Substring(0, 40);

            return sanitized;
        }

        #endregion
    }
}
