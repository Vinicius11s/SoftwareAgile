using Domain.DTOs;
using Domain.Entities;
using Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Globalization;
using QDocument = QuestPDF.Fluent.Document;

namespace Services
{
    public class PdfServices : IPdfService
    {
        public byte[] GerarCartazes(List<OfertaDTO> ofertas, byte[]? fundo = null)
        {
            // Define a licença do QuestPDF como "Community" (gratuita).
            QuestPDF.Settings.License = LicenseType.Community;

            var doc = QDocument.Create(container =>
            {
                // Vamos trabalhar sempre em pares de ofertas (2 por página).
                for (int i = 0; i < ofertas.Count; i += 2)
                {
                    var oferta1 = ofertas[i];
                    var oferta2 = i + 1 < ofertas.Count ? ofertas[i + 1] : null;

                    container.Page(page =>
                    {
                        // Configuração da página em A4 horizontal.
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(0);

                        // Se existir imagem de fundo, aplica.
                        if (fundo != null)
                            page.Background().Image(fundo).FitArea();

                        // Define a fonte padrão como Impact.
                        page.DefaultTextStyle(TextStyle.Default.FontFamily("Impact"));

                        // Divide a página em duas colunas (esquerda e direita).
                        page.Content().Row(row =>
                        {
                            // ==============================
                            // CARTAZ DA ESQUERDA
                            // ==============================
                            row.RelativeItem().Padding(20).Column(col =>
                            {
                                // Descrição do produto
                                col.Item().PaddingTop(270);
                                col.Item().Height(110) // altura fixa para duas linhas
                                        .AlignCenter()
                                        .Column(descCol =>
                                        {
                                            descCol.Item().Text(oferta1.Descricao)
                                                .FontSize(45)
                                                .AlignCenter()
                                                .FontColor("#222222")
                                                .Bold()
                                                .LineHeight(1.2f);
                                        });

                                //Espaço entre descrição e preço
                                col.Item().Height(10);

                                // Preço formatado
                                col.Item().AlignCenter().Row(priceRow =>
                                {
                                    var culture = new CultureInfo("pt-BR");
                                    var partes = oferta1.Preco.ToString("N2", culture).Split(',');
                                    var reais = partes[0];
                                    var centavos = partes.Length > 1 ? partes[1] : "00";

                                    // "R$" fixo com largura reservada para não quebrar
                                    priceRow.ConstantItem(50).Text("R$").FontSize(35)
                                        .FontColor("#b22222")
                                        .Bold();

                                    // Valor em reais (bem grande)
                                    priceRow.AutoItem().AlignBottom().Text(reais)
                                        .FontSize(134)
                                        .FontColor("#b22222")
                                        .Bold();

                                    // Centavos alinhados pelo topo do número de reais
                                    priceRow.AutoItem().Column(cc =>
                                    {
                                        cc.Item().PaddingTop(20).Text("," + centavos)
                                            .FontSize(46)
                                            .FontColor("#b22222")
                                            .Bold();

                                        // Texto "CADA" menor e em maiúsculo
                                        cc.Item().PaddingLeft(20).Text("CADA")
                                            .FontSize(16)
                                            .FontColor("#222222")
                                            .Bold();
                                    });
                                });
                            });

                            // ==============================
                            // CARTAZ DA DIREITA
                            // ==============================
                            row.RelativeItem().Padding(20).Column(col =>
                            {
                                if (oferta2 != null)
                                {
                                    // Descrição do produto
                                    col.Item().PaddingTop(270); // mesma altura da esquerda
                                    col.Item().Height(110) // altura fixa para duas linhas
                                        .AlignCenter()
                                        .Column(descCol =>
                                        {
                                            descCol.Item().Text(oferta2.Descricao)
                                                .FontSize(45)
                                                .FontColor("#222222")
                                                .Bold()
                                                .LineHeight(1.2f);
                                        });

                                    // Espaço entre descrição e preço
                                    col.Item().Height(10);

                                    // Preço formatado
                                    col.Item().AlignCenter().Row(priceRow =>
                                    {
                                        var culture = new CultureInfo("pt-BR");
                                        var partes = oferta2.Preco.ToString("N2", culture).Split(',');
                                        var reais = partes[0];
                                        var centavos = partes.Length > 1 ? partes[1] : "00";

                                        // "R$" fixo com largura reservada
                                        priceRow.ConstantItem(60).Text("R$").FontSize(40)
                                            .FontColor("#b22222")
                                            .Bold();

                                        // Valor em reais (bem grande)
                                        priceRow.AutoItem().AlignBottom().Text(reais)
                                            .FontSize(132)             // mesmo tamanho da esquerda
                                            .FontColor("#b22222")
                                            .Bold();

                                        // Centavos + "CADA"
                                        priceRow.AutoItem().Column(cc =>
                                        {
                                            cc.Item().PaddingTop(20).Text("," + centavos)
                                                .FontSize(46)
                                                .FontColor("#b22222")
                                                .Bold();

                                            cc.Item().PaddingLeft(20).Text("CADA")
                                                .FontSize(16)
                                                .FontColor("#222222")
                                                .Bold();
                                        });
                                    });
                                }
                            });
                        });
                    });
                }
            });

            return doc.GeneratePdf();
        }
    }
}
