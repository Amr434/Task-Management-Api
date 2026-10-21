using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Task_Management.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "TaskItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_CreatedById",
                table: "TaskItems",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_Users_CreatedById",
                table: "TaskItems",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Existing tasks: the creator is whoever logged the "Created"
            // history entry (TaskActivityType.Created = 0). Tasks with no such
            // entry keep a null creator, so nobody can delete them.
            migrationBuilder.Sql(@"
                UPDATE t SET CreatedById = (
                    SELECT TOP 1 a.UserId FROM TaskActivities a
                    WHERE a.TaskItemId = t.Id AND a.Type = 0
                    ORDER BY a.Id)
                FROM TaskItems t
                WHERE t.CreatedById IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_Users_CreatedById",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_CreatedById",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "TaskItems");
        }
    }
}
