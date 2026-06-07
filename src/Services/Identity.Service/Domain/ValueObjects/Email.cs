namespace Conference.Identity.Domain.ValueObjects
{
    /// <summary>
    /// Email value object - immutable, with validation
    /// </summary>
    public class Email : ValueObject
    {
        private Email() { } // EF Core constructor
        
        private Email(string value)
        {
            Value = value;
        }
        
        public string Value { get; private set; }
        
        /// <summary>
        /// Creates an Email instance with validation
        /// </summary>
        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new DomainException("Email cannot be empty");
            
            email = email.Trim().ToLowerInvariant();
            
            if (!IsValidEmail(email))
                throw new DomainException($"Invalid email format: {email}");
            
            return new Email(email);
        }
        
        /// <summary>
        /// Validates email format using regex
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
        
        public override string ToString() => Value;
    }
}
