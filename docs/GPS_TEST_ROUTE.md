# GPS Testing Routes

## Explanation

These routes are used to test and verify the GPS data handling functionality of the system.

## Key notes

* Localhost may vary:  
  * Azure uses port 5193
  * Local environment use port 5000
  * Local containers use port 8080

### POST /Gps

* Create a new GPS data entry.

```bash
  curl -X POST http://localhost:5193/Gps \
    -H "Content-Type: application/json" \
    -d '{
      "DeviceId": "device123",
      "Latitude": 51.509865,
      "Longitude": -0.118092,
      "Timestamp": "2025-09-08T12:00:00Z"
    }'
```

### GET /GpsGet

* Retrieve a GPS entry by device and timestamp.

```bash
  curl "http://localhost:5193/GpsGet?DeviceId=device123&Timestamp=2025-09-08T12:00:00Z"
```
