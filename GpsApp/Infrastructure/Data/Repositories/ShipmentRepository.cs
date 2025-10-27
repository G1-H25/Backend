using Microsoft.EntityFrameworkCore;
using GpsApp.Domain.Aggregates;
using GpsApp.Domain.ValueObjects;
using GpsApp.Infrastructure.Data;

namespace GpsApp.Infrastructure.Data.Repositories;

/// <summary>
/// EF Core implementation of Shipment repository
/// </summary>
public class ShipmentRepository : IShipmentRepository
{
    private readonly GpsAppDbContext _context;

    public ShipmentRepository(GpsAppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Shipment?> GetByIdAsync(ShipmentId shipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .Include(s => s.Packages)
            .Include(s => s.DeliveryLegs)
            .FirstOrDefaultAsync(s => s.ShipmentId == shipmentId, cancellationToken);
    }

    public async Task<IEnumerable<Shipment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .Include(s => s.Packages)
            .Include(s => s.DeliveryLegs)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        await _context.Shipments.AddAsync(shipment, cancellationToken);
    }

    public Task UpdateAsync(Shipment shipment, CancellationToken cancellationToken = default)
    {
        _context.Shipments.Update(shipment);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(ShipmentId shipmentId, CancellationToken cancellationToken = default)
    {
        var shipment = await GetByIdAsync(shipmentId, cancellationToken);
        if (shipment != null)
        {
            _context.Shipments.Remove(shipment);
        }
    }

    public async Task<bool> ExistsAsync(ShipmentId shipmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Shipments
            .AnyAsync(s => s.ShipmentId == shipmentId, cancellationToken);
    }
}
