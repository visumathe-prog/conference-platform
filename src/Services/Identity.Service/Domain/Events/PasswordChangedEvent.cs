namespace Conference.Identity.Domain.Events
{
    /// <summary>
    /// Event raised when user changes password
    /// Triggers: Invalidate all refresh tokens, send notification email
    /// </summary>
    public class PasswordChangedEvent : DomainEvent
    {
        public PasswordChangedEvent(Guid userId)
        {
            UserId = userId;
            OccurredAt = DateTime.UtcNow;
            InvalidateAllSessions = true;
        }
        
        public Guid UserId { get; }
        public DateTime OccurredAt { get; }
        public bool InvalidateAllSessions { get; }
    }
}
