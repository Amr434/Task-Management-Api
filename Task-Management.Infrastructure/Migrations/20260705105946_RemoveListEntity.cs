using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveListEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Lists_ListId",
                table: "TaskItems");

            migrationBuilder.DropTable(
                name: "Lists");

            migrationBuilder.RenameColumn(
                name: "ListId",
                table: "TaskItems",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskItems_ListId",
                table: "TaskItems",
                newName: "IX_TaskItems_ProjectId");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TaskItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Projects_ProjectId",
                table: "TaskItems",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Projects_ProjectId",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TaskItems");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "TaskItems",
                newName: "ListId");

            migrationBuilder.RenameIndex(
                name: "IX_TaskItems_ProjectId",
                table: "TaskItems",
                newName: "IX_TaskItems_ListId");

            migrationBuilder.CreateTable(
                name: "Lists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lists_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lists_ProjectId",
                table: "Lists",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Lists_ListId",
                table: "TaskItems",
                column: "ListId",
                principalTable: "Lists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
