using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DatabaseConnection.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropertyAddress",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    City = table.Column<int>(nullable: false),
                    District = table.Column<string>(nullable: true),
                    StreetName = table.Column<string>(nullable: true),
                    DetailedAddress = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyAddress", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyDetails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Area = table.Column<decimal>(nullable: false),
                    NumberOfRooms = table.Column<int>(nullable: false),
                    FloorNumber = table.Column<int>(nullable: true),
                    YearOfConstruction = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyFeatures",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GardenArea = table.Column<decimal>(nullable: true),
                    Balconies = table.Column<int>(nullable: true),
                    BasementArea = table.Column<decimal>(nullable: true),
                    OutdoorParkingPlaces = table.Column<int>(nullable: true),
                    IndoorParkingPlaces = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyPrice",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalGrossPrice = table.Column<decimal>(nullable: false),
                    PricePerMeter = table.Column<decimal>(nullable: false),
                    ResidentalRent = table.Column<decimal>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyPrice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SellerContact",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(nullable: true),
                    Telephone = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SellerContact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OfferDetails",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(nullable: true),
                    CreationDateTime = table.Column<DateTime>(nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(nullable: true),
                    OfferKind = table.Column<int>(nullable: false),
                    SellerContactId = table.Column<int>(nullable: true),
                    IsStillValid = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfferDetails_SellerContact_SellerContactId",
                        column: x => x.SellerContactId,
                        principalTable: "SellerContact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Entries",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfferDetailsId = table.Column<int>(nullable: true),
                    PropertyPriceId = table.Column<int>(nullable: true),
                    PropertyDetailsId = table.Column<int>(nullable: true),
                    PropertyAddressId = table.Column<int>(nullable: true),
                    PropertyFeaturesId = table.Column<int>(nullable: true),
                    RawDescription = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entries_OfferDetails_OfferDetailsId",
                        column: x => x.OfferDetailsId,
                        principalTable: "OfferDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entries_PropertyAddress_PropertyAddressId",
                        column: x => x.PropertyAddressId,
                        principalTable: "PropertyAddress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entries_PropertyDetails_PropertyDetailsId",
                        column: x => x.PropertyDetailsId,
                        principalTable: "PropertyDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entries_PropertyFeatures_PropertyFeaturesId",
                        column: x => x.PropertyFeaturesId,
                        principalTable: "PropertyFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Entries_PropertyPrice_PropertyPriceId",
                        column: x => x.PropertyPriceId,
                        principalTable: "PropertyPrice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entries_OfferDetailsId",
                table: "Entries",
                column: "OfferDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_PropertyAddressId",
                table: "Entries",
                column: "PropertyAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_PropertyDetailsId",
                table: "Entries",
                column: "PropertyDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_PropertyFeaturesId",
                table: "Entries",
                column: "PropertyFeaturesId");

            migrationBuilder.CreateIndex(
                name: "IX_Entries_PropertyPriceId",
                table: "Entries",
                column: "PropertyPriceId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferDetails_SellerContactId",
                table: "OfferDetails",
                column: "SellerContactId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Entries");

            migrationBuilder.DropTable(
                name: "OfferDetails");

            migrationBuilder.DropTable(
                name: "PropertyAddress");

            migrationBuilder.DropTable(
                name: "PropertyDetails");

            migrationBuilder.DropTable(
                name: "PropertyFeatures");

            migrationBuilder.DropTable(
                name: "PropertyPrice");

            migrationBuilder.DropTable(
                name: "SellerContact");
        }
    }
}
