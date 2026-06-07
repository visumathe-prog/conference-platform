namespace Conference.Identity.Domain.Events
{
    /// <summary>
    /// Event raised when a new user registers in the system
    /// Will be published to Kafka for other microservices
    /// </summary>
    public class UserRegisteredEvent : DomainEvent
    {
        public UserRegisteredEvent(
            Guid userId, 
            string email, 
            string firstName, 
            string lastName)
        {
            UserId = userId;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            OccurredAt = DateTime.UtcNow;
            EventType = "UserRegistered";
        }
        
        public Guid UserId { get; }
        public string Email { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public DateTime OccurredAt { get; }
        public string EventType { get; }
        
        /// <summary>
        /// Serializes event to JSON for Kafka message
        /// </summary>
        public string ToJson()
        {
            return JsonSerializer.Serialize(new
            {
                EventId = Guid.NewGuid(),
                EventType = EventType,
                Timestamp = OccurredAt,
                Data = new
                {
                    UserId,
                    Email,
                    FirstName,
                    LastName
                }
            });
        }
    }
}
