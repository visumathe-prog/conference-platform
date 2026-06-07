namespace Conference.Identity.Domain.Events
{
    /// <summary>
    /// Event raised when user confirms their email address
    /// </summary>
    public class EmailConfirmedEvent : DomainEvent
    {
        public EmailConfirmedEvent(Guid userId, string email)
        {
            UserId = userId;
            Email = email;
            ConfirmedAt = DateTime.UtcNow;
        }
        
        public Guid UserId { get; }
        public string Email { get; }
        public DateTime ConfirmedAt { get; }
    }
}
