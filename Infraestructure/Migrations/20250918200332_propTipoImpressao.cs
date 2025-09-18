using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class propTipoImpressao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TipoImpressao",
                table: "FundosPersonalizados",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_FundosPersonalizados_UsuarioId_TipoImpressao_Ativo",
                table: "FundosPersonalizados",
                columns: new[] { "UsuarioId", "TipoImpressao", "Ativo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FundosPersonalizados_UsuarioId_TipoImpressao_Ativo",
                table: "FundosPersonalizados");

            migrationBuilder.DropColumn(
                name: "TipoImpressao",
                table: "FundosPersonalizados");
        }
    }
}
