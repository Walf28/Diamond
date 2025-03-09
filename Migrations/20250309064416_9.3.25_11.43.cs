﻿using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCompany.Migrations
{
    /// <inheritdoc />
    public partial class _9325_1143 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DowntimeId",
                table: "Regions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<List<int>>(
                name: "MaterialsId",
                table: "Regions",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<List<int>>(
                name: "RegionsChildrensId",
                table: "Regions",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<List<int>>(
                name: "RegionsParentsId",
                table: "Regions",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<List<int>>(
                name: "RoutesId",
                table: "Regions",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<List<int>>(
                name: "RegionsId",
                table: "Factories",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<List<int>>(
                name: "RequestsId",
                table: "Factories",
                type: "integer[]",
                nullable: true);

            migrationBuilder.AddColumn<List<int>>(
                name: "RoutesId",
                table: "Factories",
                type: "integer[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DowntimeId",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "MaterialsId",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "RegionsChildrensId",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "RegionsParentsId",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "RoutesId",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "RegionsId",
                table: "Factories");

            migrationBuilder.DropColumn(
                name: "RequestsId",
                table: "Factories");

            migrationBuilder.DropColumn(
                name: "RoutesId",
                table: "Factories");
        }
    }
}
