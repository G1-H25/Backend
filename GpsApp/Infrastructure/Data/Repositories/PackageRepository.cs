using Microsoft.EntityFrameworkCore;
using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;
using GpsApp.Infrastructure.Data;

namespace GpsApp.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of Package repository
/// </summary>
public class PackageRepository : IPackageRepository
{
    private readonly GpsAppDbContext _context;

    public PackageRepository(GpsAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Package?> GetByIdAsync(PackageId packageId, CancellationToken cancellationToken = default)
    {
        return await _context.Packages
            .FirstOrDefaultAsync(p => p.PackageId == packageId, cancellationToken);
    }

    public async Task<IEnumerable<Package>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Packages.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Package package, CancellationToken cancellationToken = default)
    {
        await _context.Packages.AddAsync(package, cancellationToken);
    }

    public Task UpdateAsync(Package package, CancellationToken cancellationToken = default)
    {
        _context.Packages.Update(package);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(PackageId packageId, CancellationToken cancellationToken = default)
    {
        var package = await GetByIdAsync(packageId, cancellationToken);
        if (package != null)
        {
            _context.Packages.Remove(package);
        }
    }

    public async Task<bool> ExistsAsync(PackageId packageId, CancellationToken cancellationToken = default)
    {
        return await _context.Packages
            .AnyAsync(p => p.PackageId == packageId, cancellationToken);
    }
}
