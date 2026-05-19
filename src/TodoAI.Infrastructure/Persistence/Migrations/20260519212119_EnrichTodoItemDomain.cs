using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoAI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnrichTodoItemDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "todo_items");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "todo_items",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CompletedAtUtc",
                table: "todo_items",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "todo_items",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "todo_items",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "todo_items");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "todo_items");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "todo_items",
                newName: "CompletedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "todo_items",
                newName: "CreatedAtUtc");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "todo_items",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
