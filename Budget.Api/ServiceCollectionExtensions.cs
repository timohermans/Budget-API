using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Budget.Api;

public static class ServiceCollectionExtensions
{
    public static void AddBudgetApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddOpenApi(); // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddBudgetAuthentication(configuration);
    }

    private static void AddBudgetAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = configuration["Authentication:Authority"];
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = configuration["Authentication:Issuer"],
                    ValidAudiences = configuration.GetSection("Authentication:Audiences").Get<string[]>(),
                };
            });
    }
}