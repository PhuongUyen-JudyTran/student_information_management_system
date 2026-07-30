using Microsoft.Extensions.DependencyInjection;
using SIMS.Domain.Interfaces;
using SIMS.Infrastructure.Data;
using SIMS.Infrastructure.Locking;
using SIMS.Infrastructure.Repositories;

namespace SIMS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddSingleton<ICsvFileLockManager, CsvFileLockManager>();
            
            services.AddScoped<CsvContext>();

            services.AddScoped<IUserRepository, CsvUserRepository>();
            services.AddScoped<IStudentRepository, CsvStudentRepository>();
            services.AddScoped<ICourseRepository, CsvCourseRepository>();
            services.AddScoped<IEnrollmentRepository, CsvEnrollmentRepository>();

            return services;
        }
    }
}
