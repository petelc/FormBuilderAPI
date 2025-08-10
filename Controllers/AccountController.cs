using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using FormBuilderAPI.DTO;
using FormBuilderAPI.Models;
using Serilog;

namespace FormBuilderAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private static int _callCount;
        
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApiUser> _userManager;
        private readonly SignInManager<ApiUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;
        
        private readonly IDiagnosticContext _diagnosticContext;

        public AccountController(
            ApplicationDbContext context,
            UserManager<ApiUser> userManager,
            SignInManager<ApiUser> signInManager,
            IConfiguration configuration,
            ILogger<AccountController> logger,
            IDiagnosticContext diagnosticContext)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="input">A DTO containing user data</param>
        /// <returns>A 201 Created response if registration is successful.</returns>
        /// <response code="201">User created successfully.</response>
        /// <response code="400">Invalid input data.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [ResponseCache(CacheProfileName = "NoCache")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register(RegisterDTO input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newUser = new ApiUser();
                    newUser.UserName = input.UserName;
                    newUser.Email = input.Email;
                    var result = await _userManager.CreateAsync(newUser, input.Password);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User {userName} ({email}) has been created.", newUser.UserName, newUser.Email);
                        _diagnosticContext.Set("RegisterCallCount", Interlocked.Increment(ref _callCount));
                        return StatusCode(201, $"User {newUser.UserName} has been created.");
                    }
                    else
                    {
                        throw new Exception(string.Format("Error: {0}", string.Join(" ", result.Errors.Select(e => e.Description))));
                    }
                }
                else
                {
                    var details = new ValidationProblemDetails(ModelState);
                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new BadRequestObjectResult(details);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration.");
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Status = StatusCodes.Status500InternalServerError
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);
            }
        }

        /// <summary>
        /// Performs user login
        /// </summary>
        /// <param name="input">A DTO containing user's credentials</param>
        /// <returns>The Bearer token (in JWT format)</returns>
        /// <response code="200">Login successful, returns JWT token.</response>
        /// <response code="400">Login failed (bad request)</response>
        /// <response code="401">Login failed (Unauthorized)</response>
        [HttpPost]
        [ResponseCache(CacheProfileName = "NoCache")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> Login(LoginDTO input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByNameAsync(input.UserName!);
                    if (user == null || !await _userManager.CheckPasswordAsync(user, input.Password!))
                    {
                        throw new Exception("Invalid username or password.");
                    }
                    else
                    {
                        var signingCredentials = new SigningCredentials(
                            new SymmetricSecurityKey(
                                System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:SigningKey"]!)),
                                SecurityAlgorithms.HmacSha256);

                        var claims = new List<Claim>();
                        claims.Add(new Claim(ClaimTypes.Name, user.UserName!));
                        claims.AddRange(
                            (await _userManager.GetRolesAsync(user))
                            .Select(r => new Claim(ClaimTypes.Role, r)));

                        var jwtObject = new JwtSecurityToken(
                            issuer: _configuration["Jwt:Issuer"],
                            audience: _configuration["Jwt:Audience"],
                            claims: claims,
                            expires: DateTime.Now.AddSeconds(300),
                            signingCredentials: signingCredentials);

                        var jwtString = new JwtSecurityTokenHandler().WriteToken(jwtObject);
                        
                        // log the result
                        _logger.LogInformation("User {userName} ({email}) has logged in.", user.UserName, user.Email);

                        return StatusCode(StatusCodes.Status200OK, jwtString);
                    }
                }
                else
                {
                    var details = new ValidationProblemDetails(ModelState);
                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new BadRequestObjectResult(details);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user login.");
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    Status = StatusCodes.Status401Unauthorized
                };
                return StatusCode(StatusCodes.Status401Unauthorized, exceptionDetails);
            }
        }
    }
}
