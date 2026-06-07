using MediatR;

namespace Conference.Identity.Application.Commands
{
    /// <summary>
    /// Command to register a new user in the system
    /// </summary>
    public class RegisterUserCommand : IRequest<Result<UserResponseDto>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Handler for RegisterUserCommand
    /// </summary>
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly ICacheService _cache;
        private readonly ILogger<RegisterUserCommandHandler> _logger;
        
        public RegisterUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IEventBus eventBus,
            ICacheService cache,
            ILogger<RegisterUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _cache = cache;
            _logger = logger;
        }
        
        public async Task<Result<UserResponseDto>> Handle(
            RegisterUserCommand request, 
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);
                
                // Check if user already exists
                var existingUser = await _userRepository.GetByEmailAsync(
                    Email.Create(request.Email), 
                    cancellationToken);
                    
                if (existingUser != null)
                {
                    _logger.LogWarning("User with email {Email} already exists", request.Email);
                    return Result<UserResponseDto>.Failure("User with this email already exists");
                }
                
                // Create domain objects
                var email = Email.Create(request.Email);
                var password = Password.Create(request.Password);
                var firstName = FirstName.Create(request.FirstName);
                var lastName = LastName.Create(request.LastName);
                var phoneNumber = PhoneNumber.Create(request.PhoneNumber);
                
                // Create user aggregate
                var user = new User(email, password, firstName, lastName, phoneNumber);
                
                // Assign default "Attendee" role
                var attendeeRole = await _userRepository.GetRoleByNameAsync("Attendee", cancellationToken);
                if (attendeeRole != null)
                {
                    user.AssignRole(attendeeRole);
                }
                
                // Save to database
                await _userRepository.AddAsync(user, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                // Cache user in Redis (for 5 minutes)
                var cachedUser = new UserCacheDto
                {
                    Id = user.Id,
                    Email = user.Email.Value,
                    FirstName = user.FirstName.Value,
                    LastName = user.LastName.Value,
                    Roles = user.Roles.Select(r => r.Name).ToList()
                };
                await _cache.SetAsync($"user:{user.Id}", cachedUser, TimeSpan.FromMinutes(5));
                
                // Publish domain events to Kafka
                foreach (var domainEvent in user.DomainEvents)
                {
                    await _eventBus.PublishAsync(domainEvent, cancellationToken);
                }
                
                _logger.LogInformation("User registered successfully with ID: {UserId}", user.Id);
                
                // Return response
                var response = new UserResponseDto
                {
                    Id = user.Id,
                    Email = user.Email.Value,
                    FirstName = user.FirstName.Value,
                    LastName = user.LastName.Value,
                    PhoneNumber = user.PhoneNumber.Value,
                    CreatedAt = user.CreatedAt
                };
                
                return Result<UserResponseDto>.Success(response);
            }
            catch (DomainException ex)
            {
                _logger.LogError(ex, "Domain validation failed during registration for email: {Email}", request.Email);
                return Result<UserResponseDto>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for email: {Email}", request.Email);
                return Result<UserResponseDto>.Failure("An unexpected error occurred during registration");
            }
        }
    }
}
