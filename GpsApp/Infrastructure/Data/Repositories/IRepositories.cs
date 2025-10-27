using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;

namespace GpsApp.Infrastructure.Data.Repositories;

/// <summary>
/// Repository interface for Shipment aggregate
/// </summary>
public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(ShipmentId shipmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Shipment>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default);
    Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default);
    Task DeleteAsync(ShipmentId shipmentId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(ShipmentId shipmentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for Package aggregate
/// </summary>
public interface IPackageRepository
{
    Task<Package?> GetByIdAsync(PackageId packageId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Package>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Package package, CancellationToken cancellationToken = default);
    Task UpdateAsync(Package package, CancellationToken cancellationToken = default);
    Task DeleteAsync(PackageId packageId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(PackageId packageId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Unit of Work interface for managing transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IShipmentRepository Shipments { get; }
    IPackageRepository Packages { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
