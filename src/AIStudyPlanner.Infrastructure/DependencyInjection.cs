using AIStudyPlanner.Application.Common.Interfaces;
using AIStudyPlanner.Domain.Entities;
using AIStudyPlanner.Infrastructure.Data;
using AIStudyPlanner.Infrastructure.Repositories;
using AIStudyPlanner.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AIStudyPlanner.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.AddHttpClient<GeminiStudyPlannerService>(client => 
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddHttpClient<GroqStudyPlannerService>(client => 
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddScoped<IAIService, FallbackAIService>();
            services.AddScoped<IJwtService, JwtService>();

            return services;
        }
    }
}
