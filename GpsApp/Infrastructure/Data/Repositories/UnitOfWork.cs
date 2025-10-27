using Microsoft.EntityFrameworkCore.Storage;
using GpsApp.Infrastructure.Data;

namespace GpsApp.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of Unit of Work pattern
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly GpsAppDbContext _context;
    private IDbContextTransaction? _transaction;
    private IShipmentRepository? _shipments;
    private IPackageRepository? _packages;

    public UnitOfWork(GpsAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IShipmentRepository Shipments => 
        _shipments ??= new ShipmentRepository(_context);

    public IPackageRepository Packages => 
        _packages ??= new PackageRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
