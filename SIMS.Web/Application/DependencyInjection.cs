using Microsoft.Extensions.DependencyInjection;
using SIMS.Domain.Factories;

namespace SIMS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddSingleton<UserFactory>();

            services.AddScoped<ICourseAssignmentStrategy, StandardAssignmentStrategy>();
            services.AddScoped<ICourseAssignmentStrategy, HonorsAssignmentStrategy>();

            services.AddScoped<AuthService>();
            services.AddScoped<StudentService>();
            services.AddScoped<CourseService>();

            return services;
        }
    }
}
