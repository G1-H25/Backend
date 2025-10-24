using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace GpsApp.Tests.Unit
{
    public class InMemoryTestDatabase : DbContext
    {
        public InMemoryTestDatabase(DbContextOptions<InMemoryTestDatabase> options) : base(options)
        {
        }

        public DbSet<TestGateway> Gateways { get; set; }
        public DbSet<TestSensor> Sensors { get; set; }
        public DbSet<TestAccount> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Gateway table
            modelBuilder.Entity<TestGateway>(entity =>
            {
                entity.ToTable("Gateway", "Secrets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UUID).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
            });

            // Sensor table
            modelBuilder.Entity<TestSensor>(entity =>
            {
                entity.ToTable("Sensor", "Measurements");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.GatewayId).IsRequired();
                entity.Property(e => e.UUID).IsRequired();
                entity.Property(e => e.PolledAt).IsRequired();
            });

            // Account table
            modelBuilder.Entity<TestAccount>(entity =>
            {
                entity.ToTable("Account", "Secrets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CompanyId).IsRequired();
            });
        }
    }

    public class TestGateway
    {
        public int Id { get; set; }
        public Guid UUID { get; set; }
        public int UserId { get; set; }
    }

    public class TestSensor
    {
        public int Id { get; set; }
        public int GatewayId { get; set; }
        public Guid UUID { get; set; }
        public DateTime PolledAt { get; set; }
        public float? TemperatureCel { get; set; }
        public float? HumdityPct { get; set; }
        public int TempTimeOutside { get; set; }
        public int HumidTimeOutside { get; set; }
        public DateTime? TempTimerStart { get; set; }
        public DateTime? HumidTimerStart { get; set; }
        public float? TempMinMeasured { get; set; }
        public float? TempMaxMeasured { get; set; }
        public float? HumidMinMeasured { get; set; }
        public float? HumidMaxMeasured { get; set; }
    }

    public class TestAccount
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
    }
}
