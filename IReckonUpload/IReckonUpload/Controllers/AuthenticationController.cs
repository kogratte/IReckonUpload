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
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController: ControllerBase
    {
        private IRepository<Consumer> _consumerRepository;
        private readonly AppConfigurationOptions _config;

        public AuthenticationController(IRepository<Consumer> consumerRepository, IOptions<AppConfigurationOptions> configuration)
        {
            _consumerRepository = consumerRepository;
            _config = configuration?.Value;

            if (_config == null)
            {
                throw new NullReferenceException(nameof(_config));
            }
        }

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
