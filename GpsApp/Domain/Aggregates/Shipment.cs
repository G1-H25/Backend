using GpsApp.Domain.ValueObjects;
using GpsApp.Domain.Events;

namespace GpsApp.Domain.Aggregates;

/// <summary>
/// Shipment aggregate root representing a shipment of packages
/// A shipment has some packages (at least one) and travels across some legs of a journey (beginning and end nodes, and routes inbetween)
/// Follows Domain-Driven Design principles with proper encapsulation and business rules
/// </summary>
public class Shipment
{
    private readonly List<DomainEvent> _domainEvents = new();
    private readonly HashSet<Package> _packages = new();
    private readonly List<DeliveryLeg> _deliveryLegs = new();

    public ShipmentId ShipmentId { get; private set; } = null!;
    public DateTime ShipmentDate { get; private set; }
    public ShipmentStatus Status { get; private set; }
    public IReadOnlyCollection<Package> Packages => _packages;
    public IReadOnlyList<DeliveryLeg> DeliveryLegs => _deliveryLegs.AsReadOnly();

    // Domain events
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public enum ShipmentStatus
    {
        Pending,
        Shipped,
        Delivered,
        Cancelled
    }

    // Private constructor for EF Core or other ORM
    private Shipment() { }

    /// <summary>
    /// Creates a shipment from persistence storage (hydration constructor)
    /// This constructor does not raise domain events as it's used for reconstruction
    /// </summary>
    public Shipment(ShipmentId shipmentId, DateTime shipmentDate, ShipmentStatus status, 
        IReadOnlyList<Package> packages, IReadOnlyList<DeliveryLeg> deliveryLegs)
    {
        ShipmentId = shipmentId ?? throw new ArgumentNullException(nameof(shipmentId));
        ShipmentDate = shipmentDate;
        Status = status;

        // Validate packages
        if (packages == null || packages.Count == 0)
            throw new ArgumentException("Shipment must have at least one package", nameof(packages));

        // Validate delivery legs
        if (deliveryLegs == null || deliveryLegs.Count == 0)
            throw new ArgumentException("Shipment must have at least one delivery leg", nameof(deliveryLegs));

        // Add packages and delivery legs
        foreach (var package in packages)
        {
            _packages.Add(package);
        }
        _deliveryLegs.AddRange(deliveryLegs);

        // No domain events raised - this is for hydration from storage
    }

    /// <summary>
    /// Creates a new shipment with packages and delivery legs
    /// </summary>
    public Shipment(ShipmentId shipmentId, DateTime shipmentDate, IReadOnlyList<Package> packages, IReadOnlyList<DeliveryLeg> deliveryLegs)
    {
        ShipmentId = shipmentId ?? throw new ArgumentNullException(nameof(shipmentId));
        ShipmentDate = shipmentDate;
        Status = ShipmentStatus.Pending;

        // Validate packages
        if (packages == null || packages.Count == 0)
            throw new ArgumentException("Shipment must have at least one package", nameof(packages));

        // Validate delivery legs
        if (deliveryLegs == null || deliveryLegs.Count == 0)
            throw new ArgumentException("Shipment must have at least one delivery leg", nameof(deliveryLegs));

        // Validate delivery leg connectivity
        ValidateDeliveryLegConnectivity(deliveryLegs);

        // Add packages and delivery legs
        foreach (var package in packages)
        {
            _packages.Add(package);
        }
        _deliveryLegs.AddRange(deliveryLegs);

        // Raise domain event
        AddDomainEvent(new ShipmentCreatedEvent(ShipmentId, ShipmentDate, 
            packages.Select(p => p.PackageId).ToList(), deliveryLegs.ToList()));
    }

    /// <summary>
    /// Adds a package to the shipment
    /// </summary>
    public void AddPackage(Package package)
    {
        if (package == null)
            throw new ArgumentNullException(nameof(package));

        // HashSet automatically prevents duplicates
        var wasAdded = _packages.Add(package);
        
        if (wasAdded)
        {
            // Raise domain event only if package was actually added
            AddDomainEvent(new PackageAddedToShipmentEvent(ShipmentId, package.PackageId));
        }
    }

    /// <summary>
    /// Adds a delivery leg to the shipment
    /// </summary>
    public void AddDeliveryLeg(DeliveryLeg deliveryLeg)
    {
        if (deliveryLeg == null)
            throw new ArgumentNullException(nameof(deliveryLeg));

        // Check for duplicates first - if it's a duplicate, silently ignore
        if (_deliveryLegs.Contains(deliveryLeg))
            return;

        // Validate connectivity with existing legs
        ValidateNewDeliveryLegConnectivity(deliveryLeg);

        _deliveryLegs.Add(deliveryLeg);

        // Raise domain event
        AddDomainEvent(new DeliveryLegAddedToShipmentEvent(ShipmentId, deliveryLeg));
    }

    /// <summary>
    /// Connects a gateway to a specific delivery leg (typically when workers load the truck)
    /// </summary>
    public void ConnectGatewayToDeliveryLeg(DeliveryLeg originalLeg, GatewayId gatewayId)
    {
        if (originalLeg == null)
            throw new ArgumentNullException(nameof(originalLeg));

        if (gatewayId == null)
            throw new ArgumentNullException(nameof(gatewayId));

        // Find the leg in the shipment
        var legIndex = _deliveryLegs.FindIndex(leg => leg.Equals(originalLeg));
        if (legIndex == -1)
            throw new InvalidOperationException("Delivery leg not found in this shipment");

        // Assign the gateway to the leg
        var updatedLeg = originalLeg.AssignGateway(gatewayId);
        _deliveryLegs[legIndex] = updatedLeg;

        // Raise domain event
        AddDomainEvent(new GatewayConnectedToDeliveryLegEvent(ShipmentId, updatedLeg, gatewayId));
    }

    /// <summary>
    /// Starts a delivery leg (changes status from Ready to InProgress)
    /// </summary>
    public void StartDeliveryLeg(DeliveryLeg originalLeg)
    {
        if (originalLeg == null)
            throw new ArgumentNullException(nameof(originalLeg));

        // Find the leg in the shipment
        var legIndex = _deliveryLegs.FindIndex(leg => leg.Equals(originalLeg));
        if (legIndex == -1)
            throw new InvalidOperationException("Delivery leg not found in this shipment");

        // Check if previous legs are completed (except for the first leg)
        if (legIndex > 0)
        {
            var previousLeg = _deliveryLegs[legIndex - 1];
            if (previousLeg.Status != DeliveryLegStatus.Completed)
                throw new InvalidOperationException("Previous delivery leg must be completed before starting the next leg");
        }

        // Ensure all packages have sensors before starting delivery
        var packagesWithoutSensors = _packages.Where(p => !p.HasSensor).ToList();
        if (packagesWithoutSensors.Any())
        {
            var packageIds = string.Join(", ", packagesWithoutSensors.Select(p => p.PackageId));
            throw new InvalidOperationException($"Cannot start delivery leg: packages {packageIds} do not have sensors attached");
        }

        // Start the leg
        var startedLeg = originalLeg.Start();
        _deliveryLegs[legIndex] = startedLeg;

        // Raise domain event
        AddDomainEvent(new DeliveryLegStartedEvent(ShipmentId, startedLeg));
    }

    /// <summary>
    /// Completes a delivery leg (changes status from InProgress to Completed)
    /// </summary>
    public void CompleteDeliveryLeg(DeliveryLeg originalLeg)
    {
        if (originalLeg == null)
            throw new ArgumentNullException(nameof(originalLeg));

        // Find the leg in the shipment
        var legIndex = _deliveryLegs.FindIndex(leg => leg.Equals(originalLeg));
        if (legIndex == -1)
            throw new InvalidOperationException("Delivery leg not found in this shipment");

        // Complete the leg
        var completedLeg = originalLeg.Complete();
        _deliveryLegs[legIndex] = completedLeg;

        // Raise domain event
        AddDomainEvent(new DeliveryLegCompletedEvent(ShipmentId, completedLeg));
    }

    /// <summary>
    /// Changes the shipment status
    /// </summary>
    public void ChangeStatus(ShipmentStatus newStatus)
    {
        if (Status == newStatus)
            return;

        var oldStatus = Status;
        Status = newStatus;

        // Raise domain event
        AddDomainEvent(new ShipmentStatusChangedEvent(ShipmentId, oldStatus, newStatus));
    }

    /// <summary>
    /// Checks if the shipment is valid (has packages and connected delivery legs)
    /// </summary>
    public bool IsValid => HasPackages && HasValidDeliveryRoute;

    /// <summary>
    /// Checks if the shipment has packages
    /// </summary>
    public bool HasPackages => _packages.Count > 0;

    /// <summary>
    /// Checks if the shipment has a valid delivery route (connected legs)
    /// </summary>
    public bool HasValidDeliveryRoute => _deliveryLegs.Count > 0 && AreDeliveryLegsConnected();

    /// <summary>
    /// Gets the starting address of the shipment (first delivery leg's start address)
    /// </summary>
    public Address? StartingAddress => _deliveryLegs.FirstOrDefault()?.StartAddress;

    /// <summary>
    /// Gets the ending address of the shipment (last delivery leg's end address)
    /// </summary>
    public Address? EndingAddress => _deliveryLegs.LastOrDefault()?.EndAddress;

    /// <summary>
    /// Validates that all delivery legs are properly connected
    /// </summary>
    private void ValidateDeliveryLegConnectivity(IReadOnlyList<DeliveryLeg> deliveryLegs)
    {
        for (int i = 0; i < deliveryLegs.Count - 1; i++)
        {
            if (!deliveryLegs[i].ConnectsTo(deliveryLegs[i + 1]))
            {
                throw new ArgumentException($"Delivery leg {i + 1} does not connect to delivery leg {i + 2}. " +
                    $"Leg {i + 1} ends at {deliveryLegs[i].EndAddress} but leg {i + 2} starts at {deliveryLegs[i + 1].StartAddress}");
            }
        }
    }

    /// <summary>
    /// Validates that a new delivery leg connects properly with existing legs
    /// </summary>
    private void ValidateNewDeliveryLegConnectivity(DeliveryLeg newLeg)
    {
        if (_deliveryLegs.Count == 0)
            return; // First leg, no validation needed

        var lastLeg = _deliveryLegs.Last();
        if (!lastLeg.ConnectsTo(newLeg))
        {
            throw new ArgumentException($"New delivery leg does not connect to the last leg. " +
                $"Last leg ends at {lastLeg.EndAddress} but new leg starts at {newLeg.StartAddress}");
        }
    }

    /// <summary>
    /// Checks if all delivery legs are properly connected
    /// </summary>
    private bool AreDeliveryLegsConnected()
    {
        for (int i = 0; i < _deliveryLegs.Count - 1; i++)
        {
            if (!_deliveryLegs[i].ConnectsTo(_deliveryLegs[i + 1]))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Adds a domain event to the aggregate
    /// </summary>
    private void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events (typically called after persistence)
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public override string ToString() => $"Shipment {ShipmentId} with {_packages.Count} packages, {_deliveryLegs.Count} legs, Status: {Status}";
}