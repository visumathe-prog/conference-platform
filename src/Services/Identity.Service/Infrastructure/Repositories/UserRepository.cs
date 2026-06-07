namespace Conference.Identity.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for User aggregate using EF Core
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDbContext _context;
        private readonly ILogger<UserRepository> _logger;
        
        public UserRepository(IdentityDbContext context, ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
                throw;
            }
        }
        
        public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Roles)
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with roles by ID: {UserId}", id);
                throw;
            }
        }
        
        public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email.Value);
                throw;
            }
        }
        
        public async Task<User?> GetByEmailWithRolesAsync(Email email, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Roles)
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user with roles by email: {Email}", email.Value);
                throw;
            }
        }
        
        public async Task<IEnumerable<User>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Users
                    .Include(u => u.Roles)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users with pagination");
                throw;
            }
        }
        
        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.Users.AddAsync(user, cancellationToken);
                _logger.LogInformation("Added new user: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user: {UserId}", user.Id);
                throw;
            }
        }
        
        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Entry(user).State = EntityState.Modified;
                await Task.CompletedTask;
                _logger.LogDebug("Updated user: {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                throw;
            }
        }
        
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await GetByIdAsync(id, cancellationToken);
                if (user != null)
                {
                    _context.Users.Remove(user);
                    _logger.LogInformation("Deleted user: {UserId}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", id);
                throw;
            }
        }
        
        public async Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Roles
                    .FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role by name: {RoleName}", roleName);
                throw;
            }
        }
        
        public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
                _logger.LogDebug("Added refresh token for user: {UserId}", refreshToken.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding refresh token for user: {UserId}", refreshToken.UserId);
                throw;
            }
        }
        
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while saving changes");
                throw;
            }
        }
    }
}
