using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Conference.Identity.Controllers
{
    /// <summary>
    /// Authentication endpoints - Login, Register, Refresh Token, Logout
    /// No authentication required for most endpoints except logout
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;
        
        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        
        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <param name="command">Registration details</param>
        /// <returns>User information and confirmation instructions</returns>
        /// <response code="200">User registered successfully</response>
        /// <response code="400">Invalid registration data</response>
        /// <response code="409">User already exists</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
        {
            _logger.LogInformation("Registration attempt for email: {Email}", command.Email);
            
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
            {
                if (result.Error.Contains("already exists"))
                    return Conflict(new ProblemDetails 
                    { 
                        Title = "User already exists", 
                        Detail = result.Error 
                    });
                    
                return BadRequest(new ValidationProblemDetails(
                    new Dictionary<string, string[]> 
                    { 
                        { "Registration", new[] { result.Error } } 
                    }));
            }
            
            return Ok(result.Value);
        }
        
        /// <summary>
        /// Authenticate user and receive JWT tokens
        /// </summary>
        /// <param name="command">Login credentials</param>
        /// <returns>Access token and refresh token</returns>
        /// <response code="200">Authentication successful</response>
        /// <response code="401">Invalid credentials</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            // Capture client information for security audit
            command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            command.UserAgent = Request.Headers["User-Agent"].ToString();
            
            _logger.LogInformation("Login attempt for email: {Email} from IP: {IpAddress}", 
                command.Email, command.IpAddress);
            
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
                return Unauthorized(new { message = result.Error });
            
            // Set refresh token as HTTP-only cookie for additional security
            SetRefreshTokenCookie(result.Value.RefreshToken);
            
            return Ok(new 
            { 
                accessToken = result.Value.AccessToken,
                expiresIn = result.Value.ExpiresIn,
                tokenType = result.Value.TokenType,
                user = result.Value.User
            });
        }
        
        /// <summary>
        /// Refresh expired access token using refresh token
        /// </summary>
        /// <returns>New access token</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(RefreshTokenResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh()
        {
            // Get refresh token from cookie or header
            var refreshToken = Request.Cookies["refreshToken"] 
                ?? Request.Headers["X-Refresh-Token"].ToString();
            
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Refresh token is required" });
            
            var command = new RefreshTokenCommand { RefreshToken = refreshToken };
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
                return Unauthorized(new { message = result.Error });
            
            SetRefreshTokenCookie(result.Value.RefreshToken);
            
            return Ok(new { accessToken = result.Value.AccessToken });
        }
        
        /// <summary>
        /// Logout user and invalidate refresh token
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst("userId")?.Value;
            var refreshToken = Request.Cookies["refreshToken"];
            
            if (!string.IsNullOrEmpty(refreshToken) && !string.IsNullOrEmpty(userId))
            {
                var command = new LogoutCommand 
                { 
                    UserId = Guid.Parse(userId), 
                    RefreshToken = refreshToken 
                };
                await _mediator.Send(command);
            }
            
            // Clear cookie
            Response.Cookies.Delete("refreshToken");
            
            return Ok(new { message = "Logged out successfully" });
        }
        
        /// <summary>
        /// Confirm user email address
        /// </summary>
        [HttpGet("confirm-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmEmail([FromQuery] Guid userId, [FromQuery] string token)
        {
            var command = new ConfirmEmailCommand { UserId = userId, Token = token };
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });
            
            return Ok(new { message = "Email confirmed successfully" });
        }
        
        /// <summary>
        /// Request password reset email
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            // Always return OK to prevent email enumeration
            await _mediator.Send(command);
            return Ok(new { message = "If your email is registered, you will receive a password reset link" });
        }
        
        /// <summary>
        /// Reset password using token
        /// </summary>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });
            
            return Ok(new { message = "Password reset successfully" });
        }
        
        private void SetRefreshTokenCookie(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            };
            
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
