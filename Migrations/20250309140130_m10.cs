using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCompany.Migrations
{
    /// <inheritdoc />
    public partial class m10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegionsMaterials_Materials_MaterialId",
                table: "RegionsMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_RegionsMaterials_Regions_RegionId",
                table: "RegionsMaterials");

            migrationBuilder.AddForeignKey(
                name: "FK_RegionsMaterials_Materials_MaterialId",
                table: "RegionsMaterials",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegionsMaterials_Regions_RegionId",
                table: "RegionsMaterials",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegionsMaterials_Materials_MaterialId",
                table: "RegionsMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_RegionsMaterials_Regions_RegionId",
                table: "RegionsMaterials");

            migrationBuilder.AddForeignKey(
                name: "FK_RegionsMaterials_Materials_MaterialId",
                table: "RegionsMaterials",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RegionsMaterials_Regions_RegionId",
                table: "RegionsMaterials",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
