# Example Shipments for GPS Application

Using the Domain Driven type system:

## Example 1: Simple Door-to-Door Shipment (In Progress)

**Scenario**: Single leg shipment from warehouse to customer, currently in progress with sensor attached.

### Shipment Details
- **Shipment ID**: `550e8400-e29b-41d4-a716-446655440001`
- **Status**: `InProgress`
- **Shipment Date**: `2024-01-15T08:00:00Z`

### Delivery Leg
- **Start Address**: 
  - Street: `Lagerhusgatan 12`
  - City: `Stockholm`
  - Postal Code: `111 22`
  - Country: `Sweden`
- **End Address**:
  - Street: `Kundgatan 4B`
  - City: `Stockholm`
  - Postal Code: `112 23`
  - Country: `Sweden`
- **Gateway ID**: `550e8400-e29b-41d4-a716-446655440010`
- **Status**: `InProgress`

### Package
- **Package ID**: `550e8400-e29b-41d4-a716-446655440002`
- **Sender Address**:
  - Street: `Lagerhusgatan 12`
  - City: `Stockholm`
  - Postal Code: `111 22`
  - Country: `Sweden`
- **Recipient Address**:
  - Street: `Kundgatan 4B`
  - City: `Stockholm`
  - Postal Code: `112 23`
  - Country: `Sweden`
- **Sensor ID**: `SENSOR_001`
- **Expected Temperature Range**: `2°C - 8°C`
- **Expected Humidity Range**: `40% - 60%`
- **Created At**: `2024-01-15T07:30:00Z`
- **Sensor Attached At**: `2024-01-15T07:45:00Z`

---

## Example 2: Multi-Leg Shipment (Ready)

**Scenario**: Two-leg shipment from warehouse to distribution center, then to customer. Currently ready to start.

### Shipment Details
- **Shipment ID**: `550e8400-e29b-41d4-a716-446655440003`
- **Status**: `Pending`
- **Shipment Date**: `2024-01-16T09:00:00Z`

### Delivery Legs

#### Leg 1: Warehouse to Distribution Center
- **Start Address**:
  - Street: `Huvudlagerstigen 8`
  - City: `Göteborg`
  - Postal Code: `411 38`
  - Country: `Sweden`
- **End Address**:
  - Street: `Distributionsgatan 15A`
  - City: `Malmö`
  - Postal Code: `211 15`
  - Country: `Sweden`
- **Gateway ID**: `550e8400-e29b-41d4-a716-446655440011`
- **Status**: `Ready`

#### Leg 2: Distribution Center to Customer
- **Start Address**:
  - Street: `Distributionsgatan 15A`
  - City: `Malmö`
  - Postal Code: `211 15`
  - Country: `Sweden`
- **End Address**:
  - Street: `Bostadsgatan 22C`
  - City: `Lund`
  - Postal Code: `222 22`
  - Country: `Sweden`
- **Gateway ID**: `550e8400-e29b-41d4-a716-446655440012`
- **Status**: `Planned`

### Package
- **Package ID**: `550e8400-e29b-41d4-a716-446655440004`
- **Sender Address**:
  - Street: `Huvudlagerstigen 8`
  - City: `Göteborg`
  - Postal Code: `411 38`
  - Country: `Sweden`
- **Recipient Address**:
  - Street: `Bostadsgatan 22C`
  - City: `Lund`
  - Postal Code: `222 22`
  - Country: `Sweden`
- **Sensor ID**: `SENSOR_002`
- **Expected Temperature Range**: `-18°C - -15°C` (Frozen goods)
- **Expected Humidity Range**: `30% - 50%`
- **Created At**: `2024-01-16T08:30:00Z`
- **Sensor Attached At**: `2024-01-16T08:45:00Z`

---

## Example 3: Multi-Package Shipment (Pending - Missing Sensor)

**Scenario**: Three packages in one shipment, but one package is missing a sensor, so shipment remains pending.

### Shipment Details
- **Shipment ID**: `550e8400-e29b-41d4-a716-446655440005`
- **Status**: `Pending`
- **Shipment Date**: `2024-01-17T10:00:00Z`

### Delivery Leg
- **Start Address**:
  - Street: `Centrallagerstigen 5`
  - City: `Uppsala`
  - Postal Code: `751 05`
  - Country: `Sweden`
- **End Address**:
  - Street: `Affärsgatan 18B`
  - City: `Uppsala`
  - Postal Code: `752 21`
  - Country: `Sweden`
- **Gateway ID**: `null` (Not yet assigned)
- **Status**: `Planned`

### Packages

#### Package 1: Electronics (Has Sensor)
- **Package ID**: `550e8400-e29b-41d4-a716-446655440006`
- **Sender Address**:
  - Street: `Centrallagerstigen 5`
  - City: `Uppsala`
  - Postal Code: `751 05`
  - Country: `Sweden`
- **Recipient Address**:
  - Street: `Affärsgatan 18B`
  - City: `Uppsala`
  - Postal Code: `752 21`
  - Country: `Sweden`
- **Sensor ID**: `SENSOR_003`
- **Expected Temperature Range**: `15°C - 25°C`
- **Expected Humidity Range**: `45% - 65%`
- **Created At**: `2024-01-17T09:30:00Z`
- **Sensor Attached At**: `2024-01-17T09:45:00Z`

#### Package 2: Pharmaceuticals (Has Sensor)
- **Package ID**: `550e8400-e29b-41d4-a716-446655440007`
- **Sender Address**:
  - Street: `Centrallagerstigen 5`
  - City: `Uppsala`
  - Postal Code: `751 05`
  - Country: `Sweden`
- **Recipient Address**:
  - Street: `Affärsgatan 18B`
  - City: `Uppsala`
  - Postal Code: `752 21`
  - Country: `Sweden`
- **Sensor ID**: `SENSOR_004`
- **Expected Temperature Range**: `2°C - 8°C`
- **Expected Humidity Range**: `35% - 55%`
- **Created At**: `2024-01-17T09:35:00Z`
- **Sensor Attached At**: `2024-01-17T09:50:00Z`

#### Package 3: Documents (Missing Sensor)
- **Package ID**: `550e8400-e29b-41d4-a716-446655440008`
- **Sender Address**:
  - Street: `Centrallagerstigen 5`
  - City: `Uppsala`
  - Postal Code: `751 05`
  - Country: `Sweden`
- **Recipient Address**:
  - Street: `Affärsgatan 18B`
  - City: `Uppsala`
  - Postal Code: `752 21`
  - Country: `Sweden`
- **Sensor ID**: `null` (Missing sensor - shipment cannot start)
- **Expected Temperature Range**: `18°C - 22°C`
- **Expected Humidity Range**: `40% - 60%`
- **Created At**: `2024-01-17T09:40:00Z`
- **Sensor Attached At**: `null`

---

## API Usage Examples

### Creating Example 1 Shipment
```http
POST /api/Shipment
Content-Type: application/json

{
  "shipmentDate": "2024-01-15T08:00:00Z",
  "packages": [
    {
      "sender": {
        "street": "Lagerhusgatan 12",
        "city": "Stockholm",
        "postalCode": "111 22",
        "country": "Sweden"
      },
      "recipient": {
        "street": "Kundgatan 4B",
        "city": "Stockholm",
        "postalCode": "112 23",
        "country": "Sweden"
      }
    }
  ],
  "deliveryLegs": [
    {
      "startAddress": {
        "street": "Lagerhusgatan 12",
        "city": "Stockholm",
        "postalCode": "111 22",
        "country": "Sweden"
      },
      "endAddress": {
        "street": "Kundgatan 4B",
        "city": "Stockholm",
        "postalCode": "112 23",
        "country": "Sweden"
      }
    }
  ]
}
```

### Attaching Sensor to Package
```http
POST /api/Package/{packageId}/attach-sensor
Content-Type: application/json

{
  "sensorId": "SENSOR_001"
}
```

### Starting Delivery Leg
```http
POST /api/Shipment/{shipmentId}/start-leg
Content-Type: application/json

{
  "deliveryLeg": {
    "startAddress": { /* address object */ },
    "endAddress": { /* address object */ }
  }
}
```
