using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class Removed_Channel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "TimeSpent");

            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ChannelName",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "ChannelType",
                table: "Activities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "ChannelId",
                table: "TimeSpent",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "ChannelId",
                table: "Activities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "ChannelName",
                table: "Activities",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChannelType",
                table: "Activities",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
