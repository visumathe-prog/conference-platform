namespace Conference.Identity.Controllers
{
    /// <summary>
    /// User management endpoints - requires authentication
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UsersController> _logger;
        
        public UsersController(IMediator mediator, ILogger<UsersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        
        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var query = new GetUserQuery { UserId = Guid.Parse(userId) };
            var result = await _mediator.Send(query);
            
            return Ok(result.Value);
        }
        
        /// <summary>
        /// Update current user profile
        /// </summary>
        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserCommand command)
        {
            var userId = User.FindFirst("userId")?.Value;
            command.UserId = Guid.Parse(userId!);
            
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });
            
            return Ok(result.Value);
        }
        
        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("me/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var userId = User.FindFirst("userId")?.Value;
            command.UserId = Guid.Parse(userId!);
            
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });
            
            return Ok(new { message = "Password changed successfully" });
        }
        
        /// <summary>
        /// Get user by ID (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var query = new GetUserQuery { UserId = id };
            var result = await _mediator.Send(query);
            
            if (!result.IsSuccess)
                return NotFound();
            
            return Ok(result.Value);
        }
        
        /// <summary>
        /// Get all users with pagination (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = new GetUsersListQuery { Page = page, PageSize = pageSize };
            var result = await _mediator.Send(query);
            
            return Ok(result.Value);
        }
        
        /// <summary>
        /// Assign role to user (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{id:guid}/roles")]
        public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleCommand command)
        {
            command.UserId = id;
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });
            
            return Ok(new { message = "Role assigned successfully" });
        }
        
        /// <summary>
        /// Remove role from user (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRole(Guid id, string roleName)
        {
            var command = new RemoveRoleCommand { UserId = id, RoleName = roleName };
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });
            
            return Ok(new { message = "Role removed successfully" });
        }
        
        /// <summary>
        /// Deactivate user (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{id:guid}/deactivate")]
        public async Task<IActionResult> DeactivateUser(Guid id)
        {
            var command = new DeactivateUserCommand { UserId = id };
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });
            
            return Ok(new { message = "User deactivated successfully" });
        }
        
        /// <summary>
        /// Reactivate user (Admin only)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("{id:guid}/reactivate")]
        public async Task<IActionResult> ReactivateUser(Guid id)
        {
            var command = new ReactivateUserCommand { UserId = id };
            var result = await _mediator.Send(command);
            
            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });
            
            return Ok(new { message = "User reactivated successfully" });
        }
    }
}
