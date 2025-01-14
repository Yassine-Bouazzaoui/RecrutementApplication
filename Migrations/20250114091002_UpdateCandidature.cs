using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecrutementApplication.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCandidature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Candidatures_OffreId",
                table: "Candidatures",
                column: "OffreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidatures_Offers_OffreId",
                table: "Candidatures",
                column: "OffreId",
                principalTable: "Offers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidatures_Offers_OffreId",
                table: "Candidatures");

            migrationBuilder.DropIndex(
                name: "IX_Candidatures_OffreId",
                table: "Candidatures");
        }
    }
}
