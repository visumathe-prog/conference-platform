using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Conference.Identity.IntegrationTests.Controllers
{
    /// <summary>
    /// Integration tests for AuthController using TestContainers
    /// </summary>
    [Collection("Database collection")]
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly PostgreSqlContainer _postgresContainer;
        private readonly RedisContainer _redisContainer;
        private readonly KafkaContainer _kafkaContainer;
        
        public AuthControllerTests()
        {
            // Initialize test containers
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:15-alpine")
                .WithDatabase("test_identity_db")
                .WithUsername("test")
                .WithPassword("test123")
                .Build();
            
            _redisContainer = new RedisBuilder()
                .WithImage("redis:7-alpine")
                .Build();
            
            _kafkaContainer = new KafkaBuilder()
                .WithImage("confluentinc/cp-kafka:latest")
                .Build();
            
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Replace real services with test containers
                        services.RemoveAll(typeof(DbContextOptions<IdentityDbContext>));
                        services.AddDbContext<IdentityDbContext>(options =>
                        {
                            options.UseNpgsql(_postgresContainer.GetConnectionString());
                        });
                        
                        services.RemoveAll(typeof(IConnectionMultiplexer));
                        services.AddSingleton<IConnectionMultiplexer>(sp =>
                            ConnectionMultiplexer.Connect(_redisContainer.GetConnectionString()));
                    });
                });
            
            _client = _factory.CreateClient();
        }
        
        public async Task InitializeAsync()
        {
            await _postgresContainer.StartAsync();
            await _redisContainer.StartAsync();
            await _kafkaContainer.StartAsync();
        }
        
        public async Task DisposeAsync()
        {
            await _postgresContainer.DisposeAsync();
            await _redisContainer.DisposeAsync();
            await _kafkaContainer.DisposeAsync();
            _client.Dispose();
            await _factory.DisposeAsync();
        }
        
        [Fact]
        public async Task Register_WithValidData_ShouldReturnOk()
        {
            // Arrange
            var command = new RegisterUserCommand
            {
                Email = "test@example.com",
                Password = "Test@123456",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", command);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<UserResponseDto>();
            result.Should().NotBeNull();
            result!.Email.Should().Be("test@example.com");
        }
        
        [Fact]
        public async Task Register_WithExistingEmail_ShouldReturnConflict()
        {
            // Arrange
            var command = new RegisterUserCommand
            {
                Email = "duplicate@example.com",
                Password = "Test@123456",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            };
            
            // Register first time
            await _client.PostAsJsonAsync("/api/auth/register", command);
            
            // Act - register second time with same email
            var response = await _client.PostAsJsonAsync("/api/auth/register", command);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }
        
        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnTokens()
        {
            // Arrange - Register user first
            var registerCommand = new RegisterUserCommand
            {
                Email = "login@example.com",
                Password = "Test@123456",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
            
            // Act - Login
            var loginCommand = new LoginCommand
            {
                Email = "login@example.com",
                Password = "Test@123456"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var result = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            result.Should().NotBeNull();
            result!.AccessToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
            result.TokenType.Should().Be("Bearer");
            result.ExpiresIn.Should().Be(3600);
        }
        
        [Fact]
        public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            var loginCommand = new LoginCommand
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword123"
            };
            
            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        
        [Fact]
        public async Task Refresh_WithValidToken_ShouldReturnNewAccessToken()
        {
            // Arrange - Register and login to get refresh token
            var registerCommand = new RegisterUserCommand
            {
                Email = "refresh@example.com",
                Password = "Test@123456",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
            
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", 
                new LoginCommand { Email = "refresh@example.com", Password = "Test@123456" });
            
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            
            // Set refresh token in cookie
            _client.DefaultRequestHeaders.Add("X-Refresh-Token", loginResult!.RefreshToken);
            
            // Act
            var refreshResponse = await _client.PostAsync("/api/auth/refresh", null);
            
            // Assert
            refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var refreshResult = await refreshResponse.Content.ReadFromJsonAsync<RefreshTokenResponseDto>();
            refreshResult.Should().NotBeNull();
            refreshResult!.AccessToken.Should().NotBeNullOrEmpty();
        }
        
        [Fact]
        public async Task GetCurrentUser_WithValidToken_ShouldReturnUserProfile()
        {
            // Arrange - Register, login, get token
            var user = await RegisterAndLoginUser();
            
            // Set authorization header
            _client.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", user.AccessToken);
            
            // Act
            var response = await _client.GetAsync("/api/users/me");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>();
            profile.Should().NotBeNull();
            profile!.Email.Should().Be("profile@example.com");
        }
        
        [Fact]
        public async Task GetCurrentUser_WithoutToken_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/users/me");
            
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        
        private async Task<AuthResponseDto> RegisterAndLoginUser()
        {
            var registerCommand = new RegisterUserCommand
            {
                Email = "profile@example.com",
                Password = "Test@123456",
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "+1234567890"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerCommand);
            
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
                new LoginCommand { Email = "profile@example.com", Password = "Test@123456" });
            
            return await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>()
                ?? throw new InvalidOperationException();
        }
    }
}
