using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecrutementApplication.Migrations
{
    /// <inheritdoc />
    public partial class AddCandidatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Candidatures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidatId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OffreId = table.Column<int>(type: "int", nullable: false),
                    LettreMotivation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatePostulation = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Candidatures", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Candidatures");
        }
    }
}
