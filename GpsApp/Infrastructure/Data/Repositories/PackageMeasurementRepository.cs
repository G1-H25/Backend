using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GpsApp.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of PackageMeasurement repository
/// </summary>
public class PackageMeasurementRepository : IPackageMeasurementRepository
{
    private readonly GpsAppDbContext _context;

    public PackageMeasurementRepository(GpsAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PackageMeasurement?> GetByIdAsync(PackageMeasurementId id)
    {
        return await _context.PackageMeasurements
            .Include(pm => pm.DeliveryLeg)
            .FirstOrDefaultAsync(pm => pm.Id.Value == id.Value);
    }

    public async Task<IEnumerable<PackageMeasurement>> GetAllAsync()
    {
        return await _context.PackageMeasurements
            .Include(pm => pm.DeliveryLeg)
            .ToListAsync();
    }

    public async Task<IEnumerable<PackageMeasurement>> GetByPackageIdAsync(PackageId packageId)
    {
        return await _context.PackageMeasurements
            .Include(pm => pm.DeliveryLeg)
            .Where(pm => pm.PackageId.Value == packageId.Value)
            .ToListAsync();
    }

    public async Task<IEnumerable<PackageMeasurement>> GetByDeliveryLegIdAsync(DeliveryLegId deliveryLegId)
    {
        return await _context.PackageMeasurements
            .Include(pm => pm.DeliveryLeg)
            .Where(pm => pm.DeliveryLeg.DeliveryLegId.Value == deliveryLegId.Value)
            .ToListAsync();
    }

    public async Task<PackageMeasurement?> GetActiveByPackageAndDeliveryLegAsync(PackageId packageId, DeliveryLegId deliveryLegId)
    {
        return await _context.PackageMeasurements
            .Include(pm => pm.DeliveryLeg)
            .Where(pm => pm.PackageId.Value == packageId.Value && 
                        pm.DeliveryLeg.DeliveryLegId.Value == deliveryLegId.Value &&
                        pm.SessionEndTime == null) // Active session
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(PackageMeasurement packageMeasurement)
    {
        await _context.PackageMeasurements.AddAsync(packageMeasurement);
    }

    public Task UpdateAsync(PackageMeasurement packageMeasurement)
    {
        _context.PackageMeasurements.Update(packageMeasurement);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(PackageMeasurementId id)
    {
        var packageMeasurement = await GetByIdAsync(id);
        if (packageMeasurement != null)
        {
            _context.PackageMeasurements.Remove(packageMeasurement);
        }
    }
}
