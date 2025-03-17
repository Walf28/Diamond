using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Diamond.Migrations
{
    /// <inheritdoc />
    public partial class m1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Factories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Factories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Workload = table.Column<int>(type: "integer", nullable: false),
                    MaxVolume = table.Column<int>(type: "integer", nullable: false),
                    TransitTime = table.Column<int>(type: "integer", nullable: false),
                    FactoryId = table.Column<int>(type: "integer", nullable: false),
                    DowntimeId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regions_Factories_FactoryId",
                        column: x => x.FactoryId,
                        principalTable: "Factories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FactoryId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Routes_Factories_FactoryId",
                        column: x => x.FactoryId,
                        principalTable: "Factories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductsGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    MaterialId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductsGroup_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Downtimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    DowntimeStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DowntimeDuration = table.Column<int>(type: "integer", nullable: true),
                    DowntimeReason = table.Column<string>(type: "text", nullable: true),
                    DowntimeFinish = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Downtimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Downtimes_Regions_Id",
                        column: x => x.Id,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RegionRegion",
                columns: table => new
                {
                    RegionsChildrensId = table.Column<int>(type: "integer", nullable: false),
                    RegionsParentsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionRegion", x => new { x.RegionsChildrensId, x.RegionsParentsId });
                    table.ForeignKey(
                        name: "FK_RegionRegion_Regions_RegionsChildrensId",
                        column: x => x.RegionsChildrensId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegionRegion_Regions_RegionsParentsId",
                        column: x => x.RegionsParentsId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegionsMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Power = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    RegionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionsMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegionsMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegionsMaterials_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegionRoute",
                columns: table => new
                {
                    RegionsId = table.Column<int>(type: "integer", nullable: false),
                    RoutesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionRoute", x => new { x.RegionsId, x.RoutesId });
                    table.ForeignKey(
                        name: "FK_RegionRoute_Regions_RegionsId",
                        column: x => x.RegionsId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegionRoute_Routes_RoutesId",
                        column: x => x.RoutesId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductsSpecific",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    ProductGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsSpecific", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductsSpecific_ProductsGroup_ProductGroupId",
                        column: x => x.ProductGroupId,
                        principalTable: "ProductsGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Requests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    DateOfReceipt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateOfDesiredComplete = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateOfAcceptance = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateOfComplete = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FactoryId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    RouteId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Factories_FactoryId",
                        column: x => x.FactoryId,
                        principalTable: "Factories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Requests_ProductsSpecific_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ProductsSpecific",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Requests_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Factories_Name",
                table: "Factories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductsGroup_MaterialId",
                table: "ProductsGroup",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsSpecific_ProductGroupId",
                table: "ProductsSpecific",
                column: "ProductGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_RegionRegion_RegionsParentsId",
                table: "RegionRegion",
                column: "RegionsParentsId");

            migrationBuilder.CreateIndex(
                name: "IX_RegionRoute_RoutesId",
                table: "RegionRoute",
                column: "RoutesId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_FactoryId",
                table: "Regions",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_RegionsMaterials_MaterialId",
                table: "RegionsMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_RegionsMaterials_RegionId",
                table: "RegionsMaterials",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_FactoryId",
                table: "Requests",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ProductId",
                table: "Requests",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_RouteId",
                table: "Requests",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Routes_FactoryId",
                table: "Routes",
                column: "FactoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Downtimes");

            migrationBuilder.DropTable(
                name: "RegionRegion");

            migrationBuilder.DropTable(
                name: "RegionRoute");

            migrationBuilder.DropTable(
                name: "RegionsMaterials");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "ProductsSpecific");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "ProductsGroup");

            migrationBuilder.DropTable(
                name: "Factories");

            migrationBuilder.DropTable(
                name: "Materials");
        }
    }
}
