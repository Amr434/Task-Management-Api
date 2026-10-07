using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedToId",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedAt",
                table: "Comments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResolvedById",
                table: "Comments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AssignedToId",
                table: "Comments",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ResolvedById",
                table: "Comments",
                column: "ResolvedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_AssignedToId",
                table: "Comments",
                column: "AssignedToId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_ResolvedById",
                table: "Comments",
                column: "ResolvedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_AssignedToId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_ResolvedById",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_AssignedToId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ResolvedById",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "AssignedToId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ResolvedAt",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ResolvedById",
                table: "Comments");
        }
    }
}
