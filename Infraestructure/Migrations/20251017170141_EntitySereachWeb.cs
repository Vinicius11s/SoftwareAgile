using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class EntitySereachWeb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImagensProduto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoBarras = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CaminhoImagem = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NomeArquivo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UrlOrigem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FonteImagem = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataBusca = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataUpload = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TamanhoArquivo = table.Column<long>(type: "bigint", nullable: false),
                    TipoArquivo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagensProduto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImagensProduto_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImagensProduto_CodigoBarras",
                table: "ImagensProduto",
                column: "CodigoBarras");

            migrationBuilder.CreateIndex(
                name: "IX_ImagensProduto_CodigoBarras_UsuarioId_Ativo",
                table: "ImagensProduto",
                columns: new[] { "CodigoBarras", "UsuarioId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_ImagensProduto_DataBusca",
                table: "ImagensProduto",
                column: "DataBusca");

            migrationBuilder.CreateIndex(
                name: "IX_ImagensProduto_UsuarioId",
                table: "ImagensProduto",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ImagensProduto_UsuarioId_Ativo",
                table: "ImagensProduto",
                columns: new[] { "UsuarioId", "Ativo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImagensProduto");
        }
    }
}
