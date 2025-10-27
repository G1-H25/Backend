using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GpsApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Packages",
                columns: table => new
                {
                    PackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sender_Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sender_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sender_PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Sender_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Recipient_Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Recipient_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Recipient_PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Recipient_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExpectedTemperatureRange_Minimum = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ExpectedTemperatureRange_Maximum = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ExpectedHumidityRange_Minimum = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ExpectedHumidityRange_Maximum = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    SensorId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SensorAttachedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Packages", x => x.PackageId);
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShipmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.ShipmentId);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryLeg",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartAddress_Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartAddress_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartAddress_PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartAddress_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EndAddress_Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EndAddress_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EndAddress_PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EndAddress_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GatewayId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryLeg", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryLeg_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "ShipmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentPackages",
                columns: table => new
                {
                    ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentPackages", x => new { x.ShipmentId, x.PackageId });
                    table.ForeignKey(
                        name: "FK_ShipmentPackages_Packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "Packages",
                        principalColumn: "PackageId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShipmentPackages_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "ShipmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryLeg_ShipmentId",
                table: "DeliveryLeg",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentPackages_PackageId",
                table: "ShipmentPackages",
                column: "PackageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryLeg");

            migrationBuilder.DropTable(
                name: "ShipmentPackages");

            migrationBuilder.DropTable(
                name: "Packages");

            migrationBuilder.DropTable(
                name: "Shipments");
        }
    }
}
