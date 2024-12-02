using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShiftLogger.Migrations
{
    /// <inheritdoc />
    public partial class NickToMarker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Nick",
                table: "Cars",
                newName: "Marker");

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.RenameColumn(
                name: "Marker",
                table: "Cars",
                newName: "Nick");
        }
    }
}
