using Entities;
using System.Collections.Generic;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QDocument = QuestPDF.Fluent.Document;

namespace Infra.Services
{
    public class PdfServices
    {
        public byte[] GerarCartazes(List<Oferta> ofertas, byte[]? fundo = null)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var doc = QDocument.Create(container =>
            {
                // Agrupa as ofertas em pares para criar uma página a cada duas ofertas
                for (int i = 0; i < ofertas.Count; i += 2)
                {
                    var oferta1 = ofertas[i];
                    var oferta2 = i + 1 < ofertas.Count ? ofertas[i + 1] : null;

                    container.Page(page =>
                    {
                        // Define o tamanho da página como A4 em orientação de paisagem (horizontal)
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(0);

                        page.Background().Image(fundo).FitArea();
                        page.Content().PaddingHorizontal(25).PaddingTop(300).AlignCenter().Row(row =>
                        {
                            // Container para a primeira oferta
                            row.RelativeItem().PaddingRight(10).Column(c =>
                            {
                                c.Item().AlignCenter().AlignBottom().PaddingBottom(20).Column(subCol =>
                                {
                                    subCol.Item().PaddingBottom(5)
                                        .Text(oferta1.Descricao)
                                        .FontSize(48)
                                        .FontColor("#222222")
                                        .Bold()
                                        .AlignCenter()
                                        .LineHeight(1.2f);

                                    subCol.Item().Text($"R$ {oferta1.Preco:F2}")
                                        .FontSize(80)
                                        .FontColor("#b22222")
                                        .Bold()
                                        .AlignCenter();
                                });
                            });

                            // Container para a segunda oferta, se existir
                            if (oferta2 != null)
                            {
                                row.RelativeItem().PaddingLeft(10).Column(c =>
                                {
                                    c.Item().AlignCenter().AlignBottom().PaddingBottom(20).Column(subCol =>
                                    {
                                        subCol.Item().PaddingBottom(5)
                                            .Text(oferta2.Descricao)
                                            .FontSize(48)
                                            .FontColor("#222222")
                                            .Bold()
                                            .AlignCenter()
                                            .LineHeight(1.2f);

                                        subCol.Item().Text($"R$ {oferta2.Preco:F2}")
                                            .FontSize(80)
                                            .FontColor("#b22222")
                                            .Bold()
                                            .AlignCenter();
                                    });
                                });
                            }
                        });
                    });
                }
            });

            return doc.GeneratePdf();
        }
    }
}
