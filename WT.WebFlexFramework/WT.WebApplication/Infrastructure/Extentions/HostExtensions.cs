using Microsoft.EntityFrameworkCore;

namespace WT.WebApplication.Infrastructure.Extentions
{
    public static class HostExtensions
    {

        /// <param name="host"></param>
        /// <typeparam name="TContext"></typeparam>
        /// <returns></returns>
        public static IHost RunDatabaseMigrations<TContext>(this IHost host)
            where TContext : DbContext
        {
            using var migrationScope = host.Services.CreateScope();
            using var dbContext = migrationScope.ServiceProvider.GetRequiredService<TContext>();
            var logger = migrationScope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

            logger.LogInformation("Launching database migrations.");
            dbContext.Database.Migrate();
            logger.LogInformation("Migrations completed.");

            return host;
        }
    }
}
