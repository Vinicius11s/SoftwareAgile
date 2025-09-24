using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class MachineLearningComAsOfertas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CorrecoesAprendidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TextoOriginal = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TextoCorrigido = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoCorrecao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FrequenciaUso = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UltimaUtilizacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UsuarioId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmpresaId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioAtualizacao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorrecoesAprendidas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HistoricoCorrecoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TextoOriginal = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TextoCorrigido = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoCorrecao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataCorrecao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UsuarioId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EmpresaId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SessaoId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricoCorrecoes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorrecoesAprendidas_Ativo",
                table: "CorrecoesAprendidas",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_CorrecoesAprendidas_DataCriacao",
                table: "CorrecoesAprendidas",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_CorrecoesAprendidas_TextoOriginal",
                table: "CorrecoesAprendidas",
                column: "TextoOriginal");

            migrationBuilder.CreateIndex(
                name: "IX_CorrecoesAprendidas_UsuarioId_EmpresaId_TipoCorrecao",
                table: "CorrecoesAprendidas",
                columns: new[] { "UsuarioId", "EmpresaId", "TipoCorrecao" });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoCorrecoes_DataCorrecao",
                table: "HistoricoCorrecoes",
                column: "DataCorrecao");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoCorrecoes_SessaoId",
                table: "HistoricoCorrecoes",
                column: "SessaoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoCorrecoes_TipoCorrecao",
                table: "HistoricoCorrecoes",
                column: "TipoCorrecao");

            migrationBuilder.CreateIndex(
                name: "IX_HistoricoCorrecoes_UsuarioId_EmpresaId",
                table: "HistoricoCorrecoes",
                columns: new[] { "UsuarioId", "EmpresaId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorrecoesAprendidas");

            migrationBuilder.DropTable(
                name: "HistoricoCorrecoes");
        }
    }
}
