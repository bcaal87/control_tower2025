using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CongresoUMG.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCorreoYCiclo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CicloOSemestre",
                table: "Participantes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CorreoElectronico",
                table: "Participantes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CicloOSemestre",
                table: "Participantes");

            migrationBuilder.DropColumn(
                name: "CorreoElectronico",
                table: "Participantes");
        }
    }
}
