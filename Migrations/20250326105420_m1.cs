using System;
using System.Collections.Generic;
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
                    ProductsCommonId = table.Column<List<int>>(type: "integer[]", nullable: false),
                    ProductsCommonSize = table.Column<List<int>>(type: "integer[]", nullable: false),
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
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RegionsRoute = table.Column<List<int>>(type: "integer[]", nullable: false),
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
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FactoryId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Warehouses_Factories_FactoryId",
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
                    Name = table.Column<string>(type: "text", nullable: false),
                    TechnologyProcessing = table.Column<int[]>(type: "integer[]", nullable: false),
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
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Workload = table.Column<int>(type: "integer", nullable: false),
                    TransitTime = table.Column<int>(type: "integer", nullable: false),
                    ReadjustmentTime = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FactoryId = table.Column<int>(type: "integer", nullable: false),
                    MaterialOptionNowId = table.Column<int>(type: "integer", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_Regions_Materials_MaterialOptionNowId",
                        column: x => x.MaterialOptionNowId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialsWarehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialsWarehouse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialsWarehouse_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialsWarehouse_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
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
                name: "Downtimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DowntimeStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DowntimeDuration = table.Column<int>(type: "integer", nullable: true),
                    DowntimeReason = table.Column<string>(type: "text", nullable: true),
                    DowntimeFinish = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RegionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Downtimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Downtimes_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Size = table.Column<int>(type: "integer", nullable: false),
                    ComingSoon = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsFabricating = table.Column<bool>(type: "boolean", nullable: false),
                    FactoryId = table.Column<int>(type: "integer", nullable: false),
                    RouteId = table.Column<int>(type: "integer", nullable: false),
                    RegionId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plans_Factories_FactoryId",
                        column: x => x.FactoryId,
                        principalTable: "Factories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plans_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plans_ProductsSpecific_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ProductsSpecific",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plans_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Plans_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductsSpecificWarehouse",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    WarehouseId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsSpecificWarehouse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductsSpecificWarehouse_ProductsSpecific_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ProductsSpecific",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductsSpecificWarehouse_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
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
                    CountComplete = table.Column<int>(type: "integer", nullable: false),
                    DateOfReceipt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateOfDesiredComplete = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateOfAcceptance = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateOfComplete = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FactoryId = table.Column<int>(type: "integer", nullable: true),
                    ProductId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Requests_Factories_FactoryId",
                        column: x => x.FactoryId,
                        principalTable: "Factories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Requests_ProductsSpecific_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ProductsSpecific",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Downtimes_RegionId",
                table: "Downtimes",
                column: "RegionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Factories_Name",
                table: "Factories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialsWarehouse_MaterialId",
                table: "MaterialsWarehouse",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialsWarehouse_WarehouseId",
                table: "MaterialsWarehouse",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_FactoryId",
                table: "Plans",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_MaterialId",
                table: "Plans",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_ProductId",
                table: "Plans",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_RegionId",
                table: "Plans",
                column: "RegionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_RouteId",
                table: "Plans",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsGroup_MaterialId",
                table: "ProductsGroup",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsSpecific_ProductGroupId",
                table: "ProductsSpecific",
                column: "ProductGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsSpecificWarehouse_ProductId",
                table: "ProductsSpecificWarehouse",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsSpecificWarehouse_WarehouseId",
                table: "ProductsSpecificWarehouse",
                column: "WarehouseId");

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
                name: "IX_Regions_MaterialOptionNowId",
                table: "Regions",
                column: "MaterialOptionNowId");

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
                name: "IX_Routes_FactoryId",
                table: "Routes",
                column: "FactoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_FactoryId",
                table: "Warehouses",
                column: "FactoryId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Downtimes");

            migrationBuilder.DropTable(
                name: "MaterialsWarehouse");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "ProductsSpecificWarehouse");

            migrationBuilder.DropTable(
                name: "RegionRegion");

            migrationBuilder.DropTable(
                name: "RegionRoute");

            migrationBuilder.DropTable(
                name: "RegionsMaterials");

            migrationBuilder.DropTable(
                name: "Requests");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "ProductsSpecific");

            migrationBuilder.DropTable(
                name: "Factories");

            migrationBuilder.DropTable(
                name: "ProductsGroup");

            migrationBuilder.DropTable(
                name: "Materials");
        }
    }
}
