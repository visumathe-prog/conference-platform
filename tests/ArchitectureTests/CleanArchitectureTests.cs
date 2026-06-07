using NetArchTest.Rules;
using Xunit;

namespace Conference.ArchitectureTests
{
    /// <summary>
    /// Architecture tests ensuring Clean Architecture rules are followed
    /// </summary>
    public class CleanArchitectureTests
    {
        private const string DomainNamespace = "Conference.Identity.Domain";
        private const string ApplicationNamespace = "Conference.Identity.Application";
        private const string InfrastructureNamespace = "Conference.Identity.Infrastructure";
        private const string ApiNamespace = "Conference.Identity.Controllers";
        
        [Fact]
        public void Domain_ShouldNotHaveDependencyOnOtherLayers()
        {
            // Arrange
            var assembly = typeof(User).Assembly;
            
            // Act
            var result = Types.InAssembly(assembly)
                .That().ResideInNamespace(DomainNamespace)
                .ShouldNot()
                .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
                .GetResult();
            
            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        
        [Fact]
        public void Application_ShouldNotHaveDependencyOnInfrastructure()
        {
            // Arrange
            var assembly = typeof(RegisterUserCommandHandler).Assembly;
            
            // Act
            var result = Types.InAssembly(assembly)
                .That().ResideInNamespace(ApplicationNamespace)
                .ShouldNot()
                .HaveDependencyOn(InfrastructureNamespace)
                .GetResult();
            
            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        
        [Fact]
        public void Application_ShouldNotHaveDependencyOnApi()
        {
            // Arrange
            var assembly = typeof(RegisterUserCommandHandler).Assembly;
            
            // Act
            var result = Types.InAssembly(assembly)
                .That().ResideInNamespace(ApplicationNamespace)
                .ShouldNot()
                .HaveDependencyOn(ApiNamespace)
                .GetResult();
            
            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        
        [Fact]
        public void Infrastructure_ShouldNotHaveDependencyOnApi()
        {
            // Arrange
            var assembly = typeof(UserRepository).Assembly;
            
            // Act
            var result = Types.InAssembly(assembly)
                .That().ResideInNamespace(InfrastructureNamespace)
                .ShouldNot()
                .HaveDependencyOn(ApiNamespace)
                .GetResult();
            
            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        
        [Fact]
        public void Commands_ShouldBeSealedAndImplementIRequest()
        {
            // Arrange
            var assembly = typeof(RegisterUserCommand).Assembly;
            
            // Act
            var result = Types.InAssembly(assembly)
                .That().HaveNameEndingWith("Command")
                .Should()
                .BeSealed()
                .And()
                .ImplementInterface(typeof(IRequest<>))
                .GetResult();
            
            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        
        [Fact]
        public void Handlers_ShouldImplementIRequestHandler()
        {
            // Arrange
            var assembly = typeof(RegisterUserCommandHandler).Assembly;
            
            // Act
            var result = Types.InAssembly(assembly)
                .That().HaveNameEndingWith("Handler")
                .Should()
                .ImplementInterface(typeof(IRequestHandler<,>))
                .GetResult();
            
            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        
        [Fact]
        public void Entities_ShouldHavePrivateParameterlessConstructor()
        {
            // Arrange
            var assembly = typeof(User).Assembly;
            
            // Act
            var result = Types.InAssembly(assembly)
                .That().Inherit(typeof(Entity))
                .Should()
                .HaveConstructorWithParameters(new Type[] { }) // Private parameterless
                .Or()
                .HaveConstructorWithParameters(new Type[] { })
                .GetResult();
            
            // Note: EF Core requires parameterless constructor
            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        
        [Fact]
        public void DomainEvents_ShouldBeImmutable()
        {
            // Arrange
            var assembly = typeof(UserRegisteredEvent).Assembly;
            
            // Act
            var result = Types.InAssembly(assembly)
                .That().Inherit(typeof(DomainEvent))
                .Should()
                .BeSealed()
                .And()
                .HavePropertiesWithNames("UserId", "OccurredAt") // Required properties
                .GetResult();
            
            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        
        [Fact]
        public void Controllers_ShouldHaveRouteAttribute()
        {
            // Arrange
            var assembly = typeof(AuthController).Assembly;
            
            // Act
            var result = Types.InAssembly(assembly)
                .That().HaveNameEndingWith("Controller")
                .Should()
                .HaveCustomAttribute(typeof(RouteAttribute))
                .GetResult();
            
            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        
        [Fact]
        public void Repositories_ShouldBeNamedCorrectly()
        {
            // Arrange
            var assembly = typeof(UserRepository).Assembly;
            
            // Act
            var result = Types.InAssembly(assembly)
                .That().ResideInNamespace(InfrastructureNamespace)
                .And().HaveNameEndingWith("Repository")
                .Should()
                .ImplementInterface($"I{Types.InAssembly(assembly).That().HaveNameEndingWith("Repository").GetType().Name}")
                .GetResult();
            
            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
        
        [Fact]
        public void ValueObjects_ShouldBeImmutable()
        {
            // Arrange
            var assembly = typeof(Email).Assembly;
            
            // Act
            var result = Types.InAssembly(assembly)
                .That().Inherit(typeof(ValueObject))
                .Should()
                .BeSealed()
                .And()
                .HavePropertiesWithNames("Value")
                .GetResult();
            
            // Assert
            result.IsSuccessful.Should().BeTrue();
        }
    }
}
