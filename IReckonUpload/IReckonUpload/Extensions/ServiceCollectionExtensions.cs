using IReckonUpload.DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace IReckonUpload.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfigurationSection appConfig)
        {
            var key = Encoding.ASCII.GetBytes(appConfig["JsonWebTokenConfig:Secret"]);

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.IncludeErrorDetails = true;
                    var tokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = appConfig["JsonWebTokenConfig:Issuer"],
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        RequireExpirationTime = true,
                        ValidateLifetime = true
                    };
                    options.TokenValidationParameters = tokenValidationParameters;
                });

            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, string connexionString)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connexionString));
            services.AddScoped<ITransactionService, TransactionService>();

            return services;
        }
    }
}
