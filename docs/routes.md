curl -X POST http://localhost:5193/Gps   -H "Content-Type: application/json"   -d '{
        "DeviceId": "device123",
        "Latitude": 51.509865,
        "Longitude": -0.118092,
        "Timestamp": "2025-09-08T12:00:00Z"
      }'




curl "http://localhost:5193/GpsGet?DeviceId=device123&Timestamp=2025-09-08T12:00:00Z"