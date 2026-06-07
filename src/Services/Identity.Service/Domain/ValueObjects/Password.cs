namespace Conference.Identity.Domain.ValueObjects
{
    /// <summary>
    /// Password value object - hashed using BCrypt
    /// </summary>
    public class Password : ValueObject
    {
        private const int WorkFactor = 12; // BCrypt work factor
        
        private Password() { } // EF Core constructor
        
        private Password(string hash)
        {
            Hash = hash;
        }
        
        public string Hash { get; private set; }
        
        /// <summary>
        /// Creates a new password by hashing the plain text
        /// </summary>
        public static Password Create(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
                throw new DomainException("Password cannot be empty");
            
            if (plainTextPassword.Length < 8)
                throw new DomainException("Password must be at least 8 characters");
            
            if (plainTextPassword.Length > 100)
                throw new DomainException("Password must not exceed 100 characters");
            
            // Check password strength
            if (!IsStrongPassword(plainTextPassword))
                throw new DomainException(
                    "Password must contain at least one uppercase letter, " +
                    "one lowercase letter, one digit, and one special character");
            
            var hash = BCrypt.Net.BCrypt.HashPassword(plainTextPassword, WorkFactor);
            return new Password(hash);
        }
        
        /// <summary>
        /// Verifies a plain text password against the stored hash
        /// </summary>
        public bool Verify(string plainTextPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword))
                return false;
            
            return BCrypt.Net.BCrypt.Verify(plainTextPassword, Hash);
        }
        
        /// <summary>
        /// Validates password strength using regex
        /// </summary>
        private static bool IsStrongPassword(string password)
        {
            // At least one uppercase, one lowercase, one digit, one special char
            var hasUpper = password.Any(char.IsUpper);
            var hasLower = password.Any(char.IsLower);
            var hasDigit = password.Any(char.IsDigit);
            var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));
            
            return hasUpper && hasLower && hasDigit && hasSpecial;
        }
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Hash;
        }
    }
}
