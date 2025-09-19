Localhost may vary, Azure uses 5193, local enviroemnt uses 5000, local containers use 8080

POST /Gps

Create a new GPS data entry.

  curl -X POST http://localhost:5193/Gps \
    -H "Content-Type: application/json" \
    -d '{
      "DeviceId": "device123",
      "Latitude": 51.509865,
      "Longitude": -0.118092,
      "Timestamp": "2025-09-08T12:00:00Z"
    }'


GET /GpsGet

Retrieve a GPS entry by device and timestamp.

  curl "http://localhost:5193/GpsGet?DeviceId=device123&Timestamp=2025-09-08T12:00:00Z"
