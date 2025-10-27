using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;

namespace GpsApp.Infrastructure.Data.Repositories;

/// <summary>
/// Repository interface for PackageMeasurement aggregate
/// </summary>
public interface IPackageMeasurementRepository
{
    Task<PackageMeasurement?> GetByIdAsync(PackageMeasurementId id);
    Task<IEnumerable<PackageMeasurement>> GetAllAsync();
    Task<IEnumerable<PackageMeasurement>> GetByPackageIdAsync(PackageId packageId);
    Task<IEnumerable<PackageMeasurement>> GetByDeliveryLegIdAsync(DeliveryLegId deliveryLegId);
    Task<PackageMeasurement?> GetActiveByPackageAndDeliveryLegAsync(PackageId packageId, DeliveryLegId deliveryLegId);
    Task AddAsync(PackageMeasurement packageMeasurement);
    Task UpdateAsync(PackageMeasurement packageMeasurement);
    Task DeleteAsync(PackageMeasurementId id);
}
