using Microsoft.EntityFrameworkCore;       // This clears the red squiggly line!
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HomeServiceProvider.DataAccess.Data;
using HomeServiceProvider.UnitOfWork;
using UnitOfWork = HomeServiceProvider.UnitOfWork;
using Microsoft.EntityFrameworkCore.SqlServer;


namespace HomeServiceProvider.DataAccess
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                        sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                    }));

            // Drill directly into the sub-namespace to resolve the naming conflict
            services.AddScoped<UnitOfWork.IUnitOfWork, HomeServiceProvider.UnitOfWork.UnitOfWork>();

            return services;
        }
    }
}