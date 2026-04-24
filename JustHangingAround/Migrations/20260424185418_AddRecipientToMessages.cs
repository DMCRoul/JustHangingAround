using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JustHangingAround.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipientToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Recipient",
                table: "Messages",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Recipient",
                table: "Messages");
        }
    }
}
