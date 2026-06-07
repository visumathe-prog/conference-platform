using System;
using System.Collections.Generic;
using System.Linq;
using Identity.Service.Domain.Entities;
using Identity.Service.Domain.Events;
using Identity.Service.Domain.Exceptions;
using Identity.Service.Domain.ValueObjects;

namespace Conference.Identity.Domain.Entities
{
    /// <summary>
    /// Represents a user in the system - Aggregate Root
    /// </summary>
    public class User : Entity, IAggregateRoot
    {
        private readonly List<Role> _roles = new();
        private readonly List<RefreshToken> _refreshTokens = new();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
        
        private User() { } // EF Core constructor
        
        public User(
            Email email,
            Password password,
            FirstName firstName,
            LastName lastName,
            PhoneNumber phoneNumber)
        {
            Id = Guid.NewGuid();
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Password = password ?? throw new ArgumentNullException(nameof(password));
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            EmailConfirmed = false;
            
            AddDomainEvent(new UserRegisteredEvent(Id, Email.Value, FirstName.Value, LastName.Value));
        }
        
        public Guid Id { get; private set; }
        public Email Email { get; private set; }
        public Password Password { get; private set; }
        public FirstName FirstName { get; private set; }
        public LastName LastName { get; private set; }
        public PhoneNumber PhoneNumber { get; private set; }
        public bool IsActive { get; private set; }
        public bool EmailConfirmed { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        
        public IReadOnlyCollection<Role> Roles => _roles.AsReadOnly();
        public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
        
        /// <summary>
        /// Verifies user password using BCrypt
        /// </summary>
        public bool VerifyPassword(string plainTextPassword)
        {
            return Password.Verify(plainTextPassword);
        }
        
        /// <summary>
        /// Changes user password - requires old password verification
        /// </summary>
        public void ChangePassword(string oldPassword, string newPassword)
        {
            if (!VerifyPassword(oldPassword))
                throw new DomainException("Invalid current password");
            
            Password = Password.Create(newPassword);
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new PasswordChangedEvent(Id));
        }
        
        /// <summary>
        /// Assigns a role to the user
        /// </summary>
        public void AssignRole(Role role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            
            if (_roles.Any(r => r.Id == role.Id))
                throw new DomainException($"User already has role {role.Name}");
            
            _roles.Add(role);
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new UserRoleAssignedEvent(Id, role.Id, role.Name));
        }
        
        /// <summary>
        /// Removes a role from the user
        /// </summary>
        public void RemoveRole(Role role)
        {
            var existingRole = _roles.FirstOrDefault(r => r.Id == role.Id);
            if (existingRole == null)
                throw new DomainException($"User does not have role {role.Name}");
            
            _roles.Remove(existingRole);
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new UserRoleRemovedEvent(Id, role.Id, role.Name));
        }
        
        /// <summary>
        /// Generates a new refresh token for the user
        /// </summary>
        public RefreshToken GenerateRefreshToken(string ipAddress)
        {
            var refreshToken = RefreshToken.Create(Id, ipAddress);
            _refreshTokens.Add(refreshToken);
            
            // Remove old expired tokens
            _refreshTokens.RemoveAll(t => t.IsExpired);
            
            UpdatedAt = DateTime.UtcNow;
            return refreshToken;
        }
        
        /// <summary>
        /// Revokes a specific refresh token
        /// </summary>
        public void RevokeRefreshToken(string token, string ipAddress)
        {
            var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
            if (refreshToken == null)
                throw new DomainException("Refresh token not found");
            
            refreshToken.Revoke(ipAddress);
            UpdatedAt = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Records user login
        /// </summary>
        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Confirms user email
        /// </summary>
        public void ConfirmEmail()
        {
            if (EmailConfirmed)
                throw new DomainException("Email already confirmed");
            
            EmailConfirmed = true;
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new EmailConfirmedEvent(Id, Email.Value));
        }
        
        /// <summary>
        /// Deactivates user (soft delete)
        /// </summary>
        public void Deactivate()
        {
            if (!IsActive)
                throw new DomainException("User is already deactivated");
            
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new UserDeactivatedEvent(Id));
        }
        
        /// <summary>
        /// Reactivates user
        /// </summary>
        public void Reactivate()
        {
            if (IsActive)
                throw new DomainException("User is already active");
            
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
            
            AddDomainEvent(new UserReactivatedEvent(Id));
        }
    }
}
