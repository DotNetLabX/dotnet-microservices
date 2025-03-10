using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Blocks.Core;
using Blocks.Core.Security;

namespace Articles.Security;

public static class ConfigureAuthentication
{
		public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
		{
				var jwtOptions = configuration.GetSectionByTypeName<JwtOptions>();

				services
						.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
						.AddJwtBearer(JwtOptions =>
						{
								JwtOptions.RequireHttpsMetadata = false;
								JwtOptions.SaveToken = true;
								JwtOptions.TokenValidationParameters = new TokenValidationParameters
								{
										ValidateIssuer =  true,
										ValidIssuer = jwtOptions.Issuer,
										ValidateIssuerSigningKey = true,
										IssuerSigningKey = new SymmetricSecurityKey(Encoding.Default.GetBytes(jwtOptions.Secret)),
										ValidateAudience = false,
										RequireExpirationTime = true,
										RoleClaimType = ClaimTypes.Role
								};
						});

				return services;
		}
}
