using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaUCN.src.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedPendingEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PendingEmail",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingEmail",
                table: "AspNetUsers");
        }
    }
}