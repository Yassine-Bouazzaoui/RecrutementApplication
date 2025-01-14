using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecrutementApplication.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCandidat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "CandidatId",
                table: "Candidatures",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Candidatures_CandidatId",
                table: "Candidatures",
                column: "CandidatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Candidatures_AspNetUsers_CandidatId",
                table: "Candidatures",
                column: "CandidatId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Candidatures_AspNetUsers_CandidatId",
                table: "Candidatures");

            migrationBuilder.DropIndex(
                name: "IX_Candidatures_CandidatId",
                table: "Candidatures");

            migrationBuilder.AlterColumn<string>(
                name: "CandidatId",
                table: "Candidatures",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
