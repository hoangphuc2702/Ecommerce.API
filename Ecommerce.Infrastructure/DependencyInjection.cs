using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Service.Promotion;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Options;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PayOS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureDI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpContextAccessor();
            services.Configure<JwtOptions>(configuration.GetSection("JwtSettings"));
            var jwtOptions = configuration.GetSection("JwtSettings").Get<JwtOptions>();
            if (jwtOptions == null) throw new Exception("JwtSettings is missing in appsettings.json");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions!.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["ACCESS_TOKEN"];
                        return Task.CompletedTask;
                    }
                };

            });

            services.AddScoped<IApplicationDbContext>(
                provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IPromotionEngine, PromotionEngine>();
            services.AddHttpClient<IShippingService, AhamoveService>(client =>
            {
                client.BaseAddress = new Uri("https://partner-apistg.ahamove.com/");

                //client.Timeout = TimeSpan.FromSeconds(30);
            });
            //services.AddHttpClient<IPaymentService, ZaloPayService>();
            services.Configure<AhamoveSettings>(configuration.GetSection("AhamoveSettings"));
            services.Configure<Options.PayOSOptions>(configuration.GetSection("PayOS"));

            var clientId = configuration["PayOS:ClientId"] ?? throw new Exception("PayOS ClientId is missing in appsettings.json");
            var apiKey = configuration["PayOS:ApiKey"] ?? throw new Exception("PayOS ApiKey is missing in appsettings.json");
            var checksumKey = configuration["PayOS:ChecksumKey"] ?? throw new Exception("PayOS ChecksumKey is missing in appsettings.json");

            services.AddSingleton(new PayOSClient(clientId, apiKey, checksumKey));

            services.AddScoped<IPaymentService, PayOSService>();

            return services;
        }
    }
}
