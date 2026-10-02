using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MarketService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MarketAdminId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "MarketAdminId",
                table: "Markets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarketAdminId",
                table: "Markets");
        }
    }
}
