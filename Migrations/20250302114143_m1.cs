using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCompany.Migrations
{
    /// <inheritdoc />
    public partial class m1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Regions_Factories_FactoryId",
                table: "Regions");

            migrationBuilder.AddForeignKey(
                name: "FK_Regions_Factories_FactoryId",
                table: "Regions",
                column: "FactoryId",
                principalTable: "Factories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Regions_Factories_FactoryId",
                table: "Regions");

            migrationBuilder.AddForeignKey(
                name: "FK_Regions_Factories_FactoryId",
                table: "Regions",
                column: "FactoryId",
                principalTable: "Factories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
