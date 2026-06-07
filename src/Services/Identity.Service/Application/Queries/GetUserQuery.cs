namespace Conference.Identity.Application.Queries
{
    /// <summary>
    /// Query to get user details by ID
    /// </summary>
    public class GetUserQuery : IRequest<Result<UserResponseDto>>
    {
        public Guid UserId { get; set; }
    }
    
    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cache;
        private readonly ILogger<GetUserQueryHandler> _logger;
        
        public GetUserQueryHandler(
            IUserRepository userRepository,
            ICacheService cache,
            ILogger<GetUserQueryHandler> logger)
        {
            _userRepository = userRepository;
            _cache = cache;
            _logger = logger;
        }
        
        public async Task<Result<UserResponseDto>> Handle(
            GetUserQuery request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Fetching user with ID: {UserId}", request.UserId);
                
                // Try to get from cache first
                var cacheKey = $"user:{request.UserId}";
                var cachedUser = await _cache.GetAsync<UserCacheDto>(cacheKey);
                
                if (cachedUser != null)
                {
                    _logger.LogDebug("User {UserId} found in cache", request.UserId);
                    
                    return Result<UserResponseDto>.Success(new UserResponseDto
                    {
                        Id = cachedUser.Id,
                        Email = cachedUser.Email,
                        FirstName = cachedUser.FirstName,
                        LastName = cachedUser.LastName,
                        Roles = cachedUser.Roles
                    });
                }
                
                // Get from database
                var user = await _userRepository.GetByIdWithRolesAsync(request.UserId, cancellationToken);
                
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    return Result<UserResponseDto>.Failure("User not found");
                }
                
                var response = new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email.Value,
                    FirstName = user.FirstName.Value,
                    LastName = user.LastName.Value,
                    PhoneNumber = user.PhoneNumber.Value,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt,
                    Roles = user.Roles.Select(r => r.Name).ToList()
                };
                
                // Cache for future requests
                await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
                
                _logger.LogInformation("User {UserId} fetched successfully", request.UserId);
                
                return Result<UserResponseDto>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user {UserId}", request.UserId);
                return Result<UserResponseDto>.Failure("An error occurred while fetching user");
            }
        }
    }
}
