using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.RequestObjects;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.ResultObjects;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NobatPlusAPI.Controllers
{
    [Route("authentication")]
    [ApiController]
    [Produces("application/json")]

    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILoginRep _loginRep;

        public AuthenticationController(IConfiguration configuration,ILoginRep loginRep)
        {
            _configuration = configuration;
            _loginRep = loginRep;
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<RowResultObject<string>>> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            string tokenString = "";
            RowResultObject<string> result = new RowResultObject<string>();
            var authenticateResult = await _loginRep.AuthenticateAsync(authenticationRequestBody.UserName,authenticationRequestBody.Password);

            if (authenticateResult.Status) {
                var key = _configuration["Jwt:Key"];
                var issuer = _configuration["Jwt:Issuer"];
                var audience = _configuration["Jwt:Audience"];

                var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, authenticateResult.Result.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", authenticateResult.Result.PersonID.ToString()),
            new Claim("firstName", authenticateResult.Result.Person.FirstName),
            new Claim("lastName", authenticateResult.Result.Person.LastName)
        };

                var token = new JwtSecurityToken(
                    issuer,
                    audience,
                    claims,
                    expires: DateTime.UtcNow.AddHours(1),
                    signingCredentials: signingCredentials);

                 tokenString = new JwtSecurityTokenHandler().WriteToken(token);             
            }

            result.Status = authenticateResult.Status;
            result.ErrorMessage = authenticateResult.ErrorMessage;
            result.Result = tokenString;
            if (result.Status)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

    }
}
