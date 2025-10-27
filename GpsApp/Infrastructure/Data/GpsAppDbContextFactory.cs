using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace GpsApp.Infrastructure.Data;

/// <summary>
/// Design-time factory for EF Core migrations
/// </summary>
public class GpsAppDbContextFactory : IDesignTimeDbContextFactory<GpsAppDbContext>
{
    public GpsAppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GpsAppDbContext>();
        
        // Use a default connection string for design-time operations
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=GpsAppDb;Trusted_Connection=true;MultipleActiveResultSets=true";
        
        optionsBuilder.UseSqlServer(connectionString);
        
        return new GpsAppDbContext(optionsBuilder.Options);
    }
}
