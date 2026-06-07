using Xunit;
using FluentAssertions;

namespace Conference.Identity.UnitTests.Domain
{
    /// <summary>
    /// Unit tests for User domain entity - DDD validation
    /// </summary>
    public class UserTests
    {
        [Fact]
        public void CreateUser_WithValidData_ShouldSucceed()
        {
            // Arrange
            var email = Email.Create("test@example.com");
            var password = Password.Create("Test@123456");
            var firstName = FirstName.Create("John");
            var lastName = LastName.Create("Doe");
            var phoneNumber = PhoneNumber.Create("+1234567890");
            
            // Act
            var user = new User(email, password, firstName, lastName, phoneNumber);
            
            // Assert
            user.Should().NotBeNull();
            user.Id.Should().NotBeEmpty();
            user.Email.Value.Should().Be("test@example.com");
            user.IsActive.Should().BeTrue();
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            user.DomainEvents.Should().ContainSingle(e => e is UserRegisteredEvent);
        }
        
        [Fact]
        public void CreateUser_WithInvalidEmail_ShouldThrowDomainException()
        {
            // Act
            Action act = () => Email.Create("invalid-email");
            
            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("Invalid email format: invalid-email");
        }
        
        [Fact]
        public void CreateUser_WithWeakPassword_ShouldThrowDomainException()
        {
            // Act
            Action act = () => Password.Create("weak");
            
            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("Password must be at least 8 characters");
        }
        
        [Fact]
        public void User_VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
        {
            // Arrange
            var plainPassword = "Test@123456";
            var password = Password.Create(plainPassword);
            var user = new User(
                Email.Create("test@example.com"),
                password,
                FirstName.Create("John"),
                LastName.Create("Doe"),
                PhoneNumber.Create("+1234567890"));
            
            // Act
            var result = user.VerifyPassword(plainPassword);
            
            // Assert
            result.Should().BeTrue();
        }
        
        [Fact]
        public void User_VerifyPassword_WithWrongPassword_ShouldReturnFalse()
        {
            // Arrange
            var user = new User(
                Email.Create("test@example.com"),
                Password.Create("Test@123456"),
                FirstName.Create("John"),
                LastName.Create("Doe"),
                PhoneNumber.Create("+1234567890"));
            
            // Act
            var result = user.VerifyPassword("Wrong@123");
            
            // Assert
            result.Should().BeFalse();
        }
        
        [Fact]
        public void User_ChangePassword_WithValidOldPassword_ShouldSucceed()
        {
            // Arrange
            var oldPassword = "Test@123456";
            var newPassword = "NewTest@789";
            var user = new User(
                Email.Create("test@example.com"),
                Password.Create(oldPassword),
                FirstName.Create("John"),
                LastName.Create("Doe"),
                PhoneNumber.Create("+1234567890"));
            
            // Act
            user.ChangePassword(oldPassword, newPassword);
            
            // Assert
            user.VerifyPassword(newPassword).Should().BeTrue();
            user.VerifyPassword(oldPassword).Should().BeFalse();
            user.DomainEvents.Should().ContainSingle(e => e is PasswordChangedEvent);
        }
        
        [Fact]
        public void User_AssignRole_ShouldAddRole()
        {
            // Arrange
            var user = CreateTestUser();
            var role = new Role("Attendee", "Can attend conferences");
            
            // Act
            user.AssignRole(role);
            
            // Assert
            user.Roles.Should().Contain(role);
            user.DomainEvents.Should().ContainSingle(e => e is UserRoleAssignedEvent);
        }
        
        [Fact]
        public void User_AssignSameRoleTwice_ShouldThrowDomainException()
        {
            // Arrange
            var user = CreateTestUser();
            var role = new Role("Attendee", "Can attend conferences");
            user.AssignRole(role);
            
            // Act
            Action act = () => user.AssignRole(role);
            
            // Assert
            act.Should().Throw<DomainException>()
                .WithMessage("User already has role Attendee");
        }
        
        [Fact]
        public void User_GenerateRefreshToken_ShouldCreateValidToken()
        {
            // Arrange
            var user = CreateTestUser();
            var ipAddress = "192.168.1.1";
            
            // Act
            var refreshToken = user.GenerateRefreshToken(ipAddress);
            
            // Assert
            refreshToken.Should().NotBeNull();
            refreshToken.Token.Should().NotBeNullOrEmpty();
            refreshToken.UserId.Should().Be(user.Id);
            refreshToken.CreatedByIp.Should().Be(ipAddress);
            refreshToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            user.RefreshTokens.Should().Contain(refreshToken);
        }
        
        [Fact]
        public void User_Deactivate_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var user = CreateTestUser();
            
            // Act
            user.Deactivate();
            
            // Assert
            user.IsActive.Should().BeFalse();
            user.DomainEvents.Should().ContainSingle(e => e is UserDeactivatedEvent);
        }
        
        private User CreateTestUser()
        {
            return new User(
                Email.Create("test@example.com"),
                Password.Create("Test@123456"),
                FirstName.Create("John"),
                LastName.Create("Doe"),
                PhoneNumber.Create("+1234567890"));
        }
    }
}
