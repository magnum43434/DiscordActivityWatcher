// Build the configuration from appsettings.json without hard coding the connection string.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

string connectionString = configuration.GetConnectionString("Storage");

// Configure the DbContextOptions for StudentContext.
var optionsBuilder = new DbContextOptionsBuilder<StudentContext>();
optionsBuilder.UseSqlite(connectionString);

// Create the StudentContext using dependency injection–like pattern.
using (var context = new StudentContext(optionsBuilder.Options))
{
    Console.WriteLine("Applying migrations...");
    // This will automatically apply any pending migrations.
    context.Database.Migrate();
    Console.WriteLine("Migrations applied successfully.");
}