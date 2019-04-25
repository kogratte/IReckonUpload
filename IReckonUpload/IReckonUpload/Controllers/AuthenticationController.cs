using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using IReckonUpload.Models.Configuration;
using IReckonUpload.Models.Authentication;
using IReckonUpload.Tools;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using IReckonUpload.DAL;
using IReckonUpload.Models.Consumers;

namespace IReckonUpload.Controllers
{
    /// <summary>
    /// Allow a user to get a JWT Token
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    
    public class AuthenticationController: ControllerBase
    {
        private IRepository<Consumer> _consumerRepository;
        private readonly AppConfigurationOptions _config;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerRepository"></param>
        /// <param name="configuration"></param>
        public AuthenticationController(IRepository<Consumer> consumerRepository, IOptions<AppConfigurationOptions> configuration)
        {
            _consumerRepository = consumerRepository;
            _config = configuration?.Value;

            if (_config == null)
            {
                throw new NullReferenceException(nameof(_config));
            }
        }

        /// <summary>
        /// Authenticate an user with the provided credentials.
        /// 
        /// If environment is development, you can use demo / demo to get a valid JWT.
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns>null if no user match the provided login / password</returns>
        [HttpPost]
        public LoginSuccessResponse Post(LoginRequest loginRequest)
        {
            Consumer user = _consumerRepository.FindOne(x => x.Username == loginRequest.Username && x.Password == Sha256Builder.Compute(loginRequest.Password));

            if (null == user)
            {
                return null;
            }

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.JsonWebTokenConfig.Secret);
            var signingKey = new SymmetricSecurityKey(key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddDays(_config.JsonWebTokenConfig.Validity),
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                Issuer = _config.JsonWebTokenConfig.Issuer
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new LoginSuccessResponse
            {
                JsonWebToken = tokenHandler.WriteToken(token)
            };
        }
    }
}
