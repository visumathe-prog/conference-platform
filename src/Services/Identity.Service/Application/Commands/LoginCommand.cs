namespace Conference.Identity.Application.Commands
{
    /// <summary>
    /// Command to authenticate a user and generate JWT tokens
    /// </summary>
    public class LoginCommand : IRequest<Result<AuthResponseDto>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
    
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ICacheService _cache;
        private readonly ILogger<LoginCommandHandler> _logger;
        
        public LoginCommandHandler(
            IUserRepository userRepository,
            IJwtService jwtService,
            ICacheService cache,
            ILogger<LoginCommandHandler> logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _cache = cache;
            _logger = logger;
        }
        
        public async Task<Result<AuthResponseDto>> Handle(
            LoginCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email} from IP: {IpAddress}", 
                    request.Email, request.IpAddress);
                
                // Get user by email
                var email = Email.Create(request.Email);
                var user = await _userRepository.GetByEmailWithRolesAsync(email, cancellationToken);
                
                if (user == null)
                {
                    _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
                    return Result<AuthResponseDto>.Failure("Invalid email or password");
                }
                
                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed - user is deactivated: {UserId}", user.Id);
                    return Result<AuthResponseDto>.Failure("Account is deactivated. Please contact support.");
                }
                
                // Verify password
                if (!user.VerifyPassword(request.Password))
                {
                    _logger.LogWarning("Login failed - invalid password for user: {UserId}", user.Id);
                    return Result<AuthResponseDto>.Failure("Invalid email or password");
                }
                
                // Check if email is confirmed (optional - can be configured)
                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("Login failed - email not confirmed for user: {UserId}", user.Id);
                    return Result<AuthResponseDto>.Failure("Please confirm your email address before logging in");
                }
                
                // Record login
                user.RecordLogin();
                await _userRepository.UpdateAsync(user, cancellationToken);
                
                // Generate JWT tokens
                var roles = user.Roles.Select(r => r.Name).ToList();
                var accessToken = _jwtService.GenerateAccessToken(user, roles);
                var refreshToken = user.GenerateRefreshToken(request.IpAddress);
                
                await _userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
                await _userRepository.SaveChangesAsync(cancellationToken);
                
                // Cache user session in Redis for fast auth checks
                var sessionKey = $"session:{user.Id}:{refreshToken.Token}";
                var sessionData = new UserSessionDto
                {
                    UserId = user.Id,
                    Email = user.Email.Value,
                    Roles = roles,
                    LoginAt = DateTime.UtcNow,
                    IpAddress = request.IpAddress,
                    UserAgent = request.UserAgent
                };
                await _cache.SetAsync(sessionKey, sessionData, TimeSpan.FromHours(1));
                
                _logger.LogInformation("User logged in successfully: {UserId} from IP: {IpAddress}", 
                    user.Id, request.IpAddress);
                
                var response = new AuthResponseDto
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    ExpiresIn = 3600, // 1 hour in seconds
                    TokenType = "Bearer",
                    User = new UserInfoDto
                    {
                        Id = user.Id,
                        Email = user.Email.Value,
                        FirstName = user.FirstName.Value,
                        LastName = user.LastName.Value,
                        Roles = roles
                    }
                };
                
                return Result<AuthResponseDto>.Success(response);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Domain error during login for email: {Email}", request.Email);
                return Result<AuthResponseDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for email: {Email}", request.Email);
                return Result<AuthResponseDto>.Failure("An unexpected error occurred during login");
            }
        }
    }
}
