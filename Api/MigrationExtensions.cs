using Api.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;

namespace Api;

public static class MigrationExtensions
{
    /// <summary>
    /// Checks what migrations are pending and applies them if any.
    /// Returns true if migrations were applied.
    /// </summary>
    public static bool EnsureDatabaseMigrated(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        // category name can be anything you like
        var logger = loggerFactory.CreateLogger("MigrationExtensions");

        // which migrations are already in the DB?
        var applied = context.Database.GetAppliedMigrations().ToList();
        // which migrations do we ship in code?
        var all     = context.Database.GetMigrations().ToList();
        // the ones not yet in the DB
        var pending = context.Database.GetPendingMigrations().ToList();

        var conn = context.Database.GetDbConnection();
        logger.LogInformation("Current database: {database}", conn.Database);
        logger.LogDebug($"Applied: {string.Join(", ", applied)}");
        logger.LogDebug($"Pending: {string.Join(", ", pending)}");

        if (!pending.Any())
        {
            logger.LogInformation("Database is up-to-date. No migrations to apply.");
            return false;
        }

        // apply everything that’s pending
        logger.LogInformation("Applying {Count} pending migrations...", pending.Count);
        context.Database.Migrate();
        logger.LogInformation("Successfully applied migrations: {Pending}", pending);
        return true;
    }
}