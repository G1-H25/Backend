namespace GpsApp.Services
{
    class IntegerHolder
    {
        public static int integerholder = 0;
    }

    class LivedataConstruction
    {
        public Guid GatewayUUID { get; set; }
        public Guid UUID { get; set; }
        public DateTime PolledAt { get; set; }
        public float? TemperatureCel { get; set; }
        public float? HumdityPct { get; set; }

        public TimeSpan? TempTimeOutside { get; set; }
        public TimeSpan? HumidTimeOutside { get; set; }
        public DateTime? TempTimerStart { get; set; }
        public DateTime? HumidTimerStart { get; set; }
        public float? TempMinMeasured { get; set; }
        public float? TempMaxMeasured { get; set; }
        public float? HumidMinMeasured { get; set; }
        public float? HumidMaxMeasured { get; set; }
    }


    class MockLiveData
    {
        private readonly ISqlInsert _insertService;
        private readonly ISqlUpdate _sqlUpdate;
        private readonly ISqlGet _sqlGet;

        public MockLiveData(ISqlInsert insertService, ISqlUpdate sqlUpdate, ISqlGet sqlGet)
        {
            _insertService = insertService;
            _sqlUpdate = sqlUpdate;
            _sqlGet = sqlGet;
        }

        public async Task<int> CreateMockDeliveryAsync(int existingSensorId)
        {
            int postAddressId = await InsertMockPostAddressAsync();
            int mockCompanyId = await InsertMockCompanyAsync(postAddressId);
            int registrationId = await InsertMockRegistrationAsync();

            int gatewayId = await FetchGatewayIdForSensor(existingSensorId);
            Console.WriteLine($"Fetched GatewayId: {gatewayId}");
            int mockVehicleId = await InsertMockVehicleAsync(gatewayId, registrationId);

            int senderId = await InsertMockSenderAsync(mockCompanyId);
            int recipientId = await InsertMockRecipientAsync(mockCompanyId);
            int carrierId = await InsertMockCarrierAsync(mockCompanyId, mockVehicleId);
            int routeId = await InsertMockRouteAsync();

            await InsertMockExpectedValuesForSensorAsync(existingSensorId);

            int deliveryId = await InsertMockDeliveryAsync(routeId, carrierId, senderId, recipientId, existingSensorId);

            var stateData = new Dictionary<string, object>
            {
                { "DeliveryId", deliveryId },
                { "CurrentState", "Pending" },
                { "UpdatedAt", DateTime.UtcNow }
            };
            await _insertService.InsertAsync("Orders.DeliveryState", stateData);

            Console.WriteLine($"Created mock Delivery {deliveryId} linked to Sensor {existingSensorId}");
            return deliveryId;
        }

        public async Task<int> InsertMockSenderAsync(int companyId)
        {
            var data = new Dictionary<string, object> { { "CompanyId", companyId } };
            return await _insertService.InsertAndReturnIdAsync("Logistics.Sender", data);
        }

        public async Task<int> InsertMockRecipientAsync(int companyId)
        {
            var data = new Dictionary<string, object> { { "CompanyId", companyId } };
            return await _insertService.InsertAndReturnIdAsync("Logistics.Recipient", data);
        }


        public async Task<int> InsertMockCarrierAsync(int companyId, int vehicleId)
        {
            var data = new Dictionary<string, object>
        {
            { "CompanyId", companyId },
            { "VehicleId", vehicleId }
        };
            return await _insertService.InsertAndReturnIdAsync("Logistics.Carrier", data);
        }

        public async Task<int> InsertMockRouteAsync()
        {
            var random = new Random();
            var data = new Dictionary<string, object>
        {
            { "Code", $"R{random.Next(1000, 9999)}" },
            { "Area", "MockArea" }
        };
            return await _insertService.InsertAndReturnIdAsync("Logistics.TransportRoute", data);
        }

        public async Task<int> InsertMockPostAddressAsync()
        {
            var data = new Dictionary<string, object>
        {
            { "Zipcode", "12345" },
            { "Locality", "MockTown" },
            { "Country", "MockCountry" },
            { "Street", "MockStreet" },
            { "StreetNumber", 1 }
        };
            return await _insertService.InsertAndReturnIdAsync("Customers.PostAddress", data);
        }

        public async Task<int> InsertMockCompanyAsync(int postAddressId)
        {
            var data = new Dictionary<string, object>
        {
            { "CompanyName", "MockCompany" },
            { "Email", "mock@company.com" },
            { "PostAddressId", postAddressId }
        };
            return await _insertService.InsertAndReturnIdAsync("Customers.Company", data);
        }

        public async Task<int> InsertMockRegistrationAsync()
        {
            var data = new Dictionary<string, object>
        {
            { "Plate", "12345" },
            { "Brand", "MockBrand" },
            { "Model", "MockModel" }
        };
            return await _insertService.InsertAndReturnIdAsync("Secrets.Registration", data);
        }

        public async Task<int> InsertMockVehicleAsync(int gatewayId, int registrationId)
        {
            var data = new Dictionary<string, object>
        {
            { "GatewayId", gatewayId },
            { "RegistrationId", registrationId }
        };
            return await _insertService.InsertAndReturnIdAsync("Secrets.Vehicle", data);
        }

        public async Task<int> InsertMockDeliveryAsync(int routeId, int carrierId, int senderId, int recipientId, int sensorId)
        {
            var data = new Dictionary<string, object>
        {
            { "RouteId", routeId },
            { "CarrierId", carrierId },
            { "SenderId", senderId },
            { "RecipientId", recipientId },
            { "SensorId", sensorId },
            { "OrderPlaced", DateTime.UtcNow } // corrected
        };
            return await _insertService.InsertAndReturnIdAsync("Orders.Delivery", data);
        }

        private async Task<int> FetchGatewayIdForSensor(int sensorId)
        {
            var sensorRecord = await _sqlGet.FetchAsync("Measurements.Sensor", new Dictionary<string, object>
        {
            { "Id", sensorId }
        });

            if (sensorRecord == null || !sensorRecord.Any())
                throw new Exception($"Sensor {sensorId} not found.");

            return Convert.ToInt32(sensorRecord["GatewayId"]);
        }

        public async Task InsertMockExpectedValuesForSensorAsync(int sensorId)
        {
            // Insert Expected Humidity
            var expectedHumidData = new Dictionary<string, object>
            {
                { "Note", "Mock Humidity" },
                { "Min", 30f },
                { "Max", 60f }
            };
            int expectedHumidId = await _insertService.InsertAndReturnIdAsync("Measurements.ExpectedHumid", expectedHumidData);

            // Update Sensor with ExpectedHumidId
            await _sqlUpdate.UpdateAsync("Measurements.Sensor",
                new Dictionary<string, object> { { "ExpectedHumidId", expectedHumidId } },
                new Dictionary<string, object> { { "Id", sensorId } });

            // Insert Expected Temperature
            var expectedTempData = new Dictionary<string, object>
            {
                { "Note", "Mock Temp" },
                { "Min", 2f },
                { "Max", 8f }
            };
            int expectedTempId = await _insertService.InsertAndReturnIdAsync("Measurements.ExpectedTemp", expectedTempData);

            // Update Sensor with ExpectedTempId
            await _sqlUpdate.UpdateAsync("Measurements.Sensor",
                new Dictionary<string, object> { { "ExpectedTempId", expectedTempId } },
                new Dictionary<string, object> { { "Id", sensorId } });

            Console.WriteLine($"Inserted expected values for Sensor {sensorId} (TempId={expectedTempId}, HumidId={expectedHumidId})");
        }
        public async Task<bool> DoesDeliveryExistForSensorAsync(int sensorId)
        {
            var deliveryRecord = await _sqlGet.FetchAsync("Orders.Delivery", new Dictionary<string, object>
            {
                { "SensorId", sensorId }
            });

            return deliveryRecord != null && deliveryRecord.Any();
        }
    }
}