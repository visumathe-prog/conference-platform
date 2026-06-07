namespace Conference.Identity.Domain.Events
{
    /// <summary>
    /// Event raised when a role is assigned to a user
    /// Used for authorization cache invalidation
    /// </summary>
    public class UserRoleAssignedEvent : DomainEvent
    {
        public UserRoleAssignedEvent(
            Guid userId, 
            Guid roleId, 
            string roleName)
        {
            UserId = userId;
            RoleId = roleId;
            RoleName = roleName;
            OccurredAt = DateTime.UtcNow;
        }
        
        public Guid UserId { get; }
        public Guid RoleId { get; }
        public string RoleName { get; }
        public DateTime OccurredAt { get; }
    }
}
