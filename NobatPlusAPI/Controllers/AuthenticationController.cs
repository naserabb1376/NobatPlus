using Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NobatPlusAPI.RequestObjects.Authenticate;
using NobatPlusDATA.DataLayer.Repositories;
using NobatPlusDATA.Domain;
using NobatPlusDATA.ResultObjects;
using NobatPlusDATA.Tools;
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
        private readonly IRegisterRep _registerRep;
        private readonly IPersonRep _personRep;
        private readonly ICustomerRep _customerRep;
        private readonly IStylistRep _stylistRep;
        private readonly IAddressRep _addressRep;

        public AuthenticationController(IConfiguration configuration, ILoginRep loginRep, IRegisterRep registerRep, IPersonRep personRep, ICustomerRep customerRep, IStylistRep stylistRep, IAddressRep addressRep)
        {
            _configuration = configuration;
            _loginRep = loginRep;
            _registerRep = registerRep;
            _personRep = personRep;
            _customerRep = customerRep;
            _stylistRep = stylistRep;
            _addressRep = addressRep;
        }

        [HttpPost("Authenticate")]
        public async Task<ActionResult<RowResultObject<string>>> Authenticate(AuthenticationRequestBody authenticationRequestBody)
        {
            string tokenString = "";
            RowResultObject<string> result = new RowResultObject<string>();
            var authenticateResult = await _loginRep.AuthenticateAsync(authenticationRequestBody.UserName, authenticationRequestBody.Password);

            if (authenticateResult.Status)
            {
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

        [HttpPost("Signup")]
        public async Task<ActionResult<BitResultObject>> Signup(SignupRequestBody signupRequestBody)
        {
            BitResultObject result = new BitResultObject();
            var validUserName = await _loginRep.ExistUserNameAsync(signupRequestBody.UserName);

            if (validUserName.Status && !string.IsNullOrEmpty(validUserName.ErrorMessage))
            {
                result.Status = !validUserName.Status;
                result.ErrorMessage = "نام کاربری تکراری است";
                return BadRequest(result);
            }
            Address address = new Address()
            {
                CityID = signupRequestBody.CityID,
                AddressLocationHorizentalPoint = signupRequestBody.AddressLocationHorizentalPoint,
                AddressLocationVerticalPoint = signupRequestBody.AddressLocationVerticalPoint,
                AddressPostalCode = signupRequestBody.AddressPostalCode,
                AddressStreet = signupRequestBody.AddressStreet,
                State = signupRequestBody.State,
                CreateDate = DateTime.Now,
            };

            result = await _addressRep.AddAddressAsync(address);

            if (result.Status)
            {
                Person person = new Person()
                {
                    FirstName = signupRequestBody.FirstName,
                    LastName = signupRequestBody.LastName,
                    NaCode = signupRequestBody.NaCode,
                    Email = signupRequestBody.Email,
                    PhoneNumber = signupRequestBody.PhoneNumber,
                    DateOfBirth = signupRequestBody.DateOfBirth.StringToDate(),
                    CreateDate = DateTime.Now,
                    AddressID = address.ID,
                };
                result = await _personRep.AddPersonAsync(person);

                if (result.Status)
                {
                    Customer customer = new Customer()
                    {
                        PersonID = person.ID,
                        CreateDate = DateTime.Now,
                    };
                    result = await _customerRep.AddCustomerAsync(customer);

                    if (result.Status)
                    {
                        Register register = new Register()
                        {
                            CreateDate = DateTime.Now,
                            PersonID = person.ID,
                            RegistrationDate = DateTime.Now,
                        };
                        result = await _registerRep.AddRegisterAsync(register);
                        if (result.Status)
                        {
                            Login login = new Login()
                            {
                                Username = signupRequestBody.UserName,
                                PasswordHash = signupRequestBody.Password.ToHash(),
                                PersonID = person.ID,
                                CreateDate = DateTime.Now,
                                LastLoginDate = DateTime.Now,
                            };
                            result = await _loginRep.AddLoginAsync(login);
                            if (result.Status && signupRequestBody.IsStylist)
                            {
                                Stylist stylist = new Stylist()
                                {
                                    JobTypeID = signupRequestBody.JobTypeID,
                                    YearsOfExperience = signupRequestBody.YearsOfExperience,
                                    Specialty = signupRequestBody.Specialty,
                                    StylistParentID = signupRequestBody.StylistParentID,
                                    PersonID = person.ID,
                                    CreateDate = DateTime.Now,
                                };
                                result = await _stylistRep.AddStylistAsync(stylist);
                            }
                            return Ok(result);
                        }
                    }
                }
            }
            return BadRequest(result);
        }
    }
}