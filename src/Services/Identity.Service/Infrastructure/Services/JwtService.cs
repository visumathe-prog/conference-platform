using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Conference.Identity.Infrastructure.Services
{
    /// <summary>
    /// Real JWT token generation service using Microsoft.IdentityModel
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _settings;
        private readonly ILogger<JwtService> _logger;
        
        public JwtService(IOptions<JwtSettings> settings, ILogger<JwtService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            
            // Validate configuration on startup
            if (string.IsNullOrEmpty(_settings.SecretKey) || _settings.SecretKey.Length < 32)
            {
                throw new InvalidOperationException(
                    "JWT Secret Key must be at least 32 characters long. " +
                    "Set JWT_SECRET_KEY in .env file");
            }
        }
        
        /// <summary>
        /// Generates a JWT access token for authenticated user
        /// </summary>
        public string GenerateAccessToken(User user, List<string> roles)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_settings.SecretKey));
                var credentials = new SigningCredentials(
                    securityKey, SecurityAlgorithms.HmacSha256);
                
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email.Value),
                    new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName.Value),
                    new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName.Value),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("userId", user.Id.ToString())
                };
                
                // Add roles as claims
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                
                var token = new JwtSecurityToken(
                    issuer: _settings.Issuer,
                    audience: _settings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
                    signingCredentials: credentials
                );
                
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                
                _logger.LogDebug("Generated access token for user: {UserId}, expires at: {Expiry}", 
                    user.Id, token.ValidTo);
                
                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating access token for user: {UserId}", user.Id);
                throw;
            }
        }
        
        /// <summary>
        /// Validates a JWT token and returns claims principal
        /// </summary>
        public ClaimsPrincipal ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_settings.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = _settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _settings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                _logger.LogWarning("Token has expired");
                throw;
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                _logger.LogWarning("Invalid token signature");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                throw;
            }
        }
    }
}
