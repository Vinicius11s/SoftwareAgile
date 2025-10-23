using Domain.DTOs;
using Domain.Entities;
using Interfaces.Service;
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
        public byte[] GerarCartazesA5(List<OfertaDTO> ofertas, byte[]? fundo = null, Domain.DTOs.LayoutConfiguracoes? configuracoes = null)
        {
            Console.WriteLine($"=== GerarCartazesA5 ===");
            Console.WriteLine($"Ofertas: {ofertas.Count}");
            Console.WriteLine($"Fundo: {fundo?.Length ?? 0} bytes");
            Console.WriteLine($"Configurações: {configuracoes != null}");
            
            if (configuracoes != null)
            {
                Console.WriteLine($"Configurações A5: NomeAltura={configuracoes.NomeAltura}");
            }
            
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
                            // Aplicar configurações de posicionamento se fornecidas
                            var nomeTop = configuracoes != null ? 
                                Math.Max(0, Math.Min(400, (int)((configuracoes.NomeAltura / 100.0) * 400))) : // 400px é a altura útil A5
                                240; // Posição padrão
                                
                            if (configuracoes != null)
                            {
                                Console.WriteLine($"A5 - Aplicando configurações: NomeAltura={configuracoes.NomeAltura} -> nomeTop={nomeTop}");
                            }
                                
                                var nomeLeft = 0; // Centralizado por padrão
                                
                                var precoTop = nomeTop + 120; // Posição padrão
                                
                                var precoLeft = configuracoes != null ? 
                                    Math.Max(0, Math.Min(150, (int)((configuracoes.PrecoLateral / 100.0) * 150))) : 
                                    0; // Centralizado por padrão

                                // Descrição do produto com posicionamento customizado
                                col.Item().PaddingTop(nomeTop).PaddingLeft(nomeLeft);
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

                                // Preço formatado com posicionamento customizado
                                var precoPaddingTop = Math.Max(0, precoTop - nomeTop - 110);
                                Console.WriteLine($"A5 Cartaz 1 - nomeTop: {nomeTop}, precoTop: {precoTop}, precoPaddingTop: {precoPaddingTop}");
                                col.Item().PaddingTop(precoPaddingTop).PaddingLeft(precoLeft).AlignCenter().Column(priceCol =>
                                {
                                    var culture = new CultureInfo("pt-BR");
                                    var partes = oferta1.Preco.ToString("N2", culture).Split(',');
                                    var reais = partes[0];
                                    var centavos = partes.Length > 1 ? partes[1] : "00";

                                    // Preço em uma linha mais simples
                                    priceCol.Item().Row(priceRow =>
                                    {
                                        // "R$" 
                                        priceRow.ConstantItem(40).Text("R$").FontSize(30)
                                            .FontColor("#b22222")
                                            .Bold();

                                        // Valor em reais
                                        priceRow.AutoItem().Text(reais)
                                            .FontSize(100)
                                            .FontColor("#b22222")
                                            .Bold();

                                        // Centavos
                                        priceRow.AutoItem().Text("," + centavos)
                                            .FontSize(35)
                                            .FontColor("#b22222")
                                            .Bold();
                                    });

                                    // "CADA" em linha separada
                                    priceCol.Item().Text("CADA")
                                        .FontSize(14)
                                        .FontColor("#222222")
                                        .Bold()
                                        .AlignCenter();
                                });

                            });

                            // ==============================
                            // CARTAZ DA DIREITA
                            // ==============================
                            row.RelativeItem().Padding(20).Column(col =>
                            {
                                if (oferta2 != null)
                                {
                                    // Aplicar configurações de posicionamento se fornecidas (mesmas do cartaz da esquerda)
                                    var nomeTop2 = configuracoes != null ? 
                                        Math.Max(0, Math.Min(400, (int)((configuracoes.NomeAltura / 100.0) * 400))) : // 400px é a altura útil A5
                                        240; // Posição padrão
                                    
                                    var nomeLeft2 = 0; // Centralizado por padrão
                                    
                                    var precoTop2 = nomeTop2 + 120; // Posição padrão
                                    
                                    var precoLeft2 = configuracoes != null ? 
                                        Math.Max(0, Math.Min(150, (int)((configuracoes.PrecoLateral / 100.0) * 150))) : 
                                        0; // Centralizado por padrão

                                    // Descrição do produto com posicionamento customizado
                                    col.Item().PaddingTop(nomeTop2).PaddingLeft(nomeLeft2);
                                    col.Item().Height(110) // altura fixa para duas linhas
                                        .AlignCenter()
                                        .Column(descCol =>
                                        {
                                            descCol.Item().Text(oferta2.Descricao)
                                                .FontSize(45)
                                                .AlignCenter()
                                                .FontColor("#222222")
                                                .Bold()
                                                .LineHeight(1.2f);
                                        });

                                    // Espaço entre descrição e preço
                                    col.Item().Height(10);

                                    // Preço formatado com posicionamento customizado
                                    var precoPaddingTop2 = Math.Max(0, precoTop2 - nomeTop2 - 110);
                                    col.Item().PaddingTop(precoPaddingTop2).PaddingLeft(precoLeft2).AlignCenter().Column(priceCol =>
                                    {
                                        var culture = new CultureInfo("pt-BR");
                                        var partes = oferta2.Preco.ToString("N2", culture).Split(',');
                                        var reais = partes[0];
                                        var centavos = partes.Length > 1 ? partes[1] : "00";

                                        // Preço em uma linha mais simples
                                        priceCol.Item().Row(priceRow =>
                                        {
                                            // "R$" 
                                            priceRow.ConstantItem(40).Text("R$").FontSize(30)
                                                .FontColor("#b22222")
                                                .Bold();

                                            // Valor em reais
                                            priceRow.AutoItem().Text(reais)
                                                .FontSize(100)
                                                .FontColor("#b22222")
                                                .Bold();

                                            // Centavos
                                            priceRow.AutoItem().Text("," + centavos)
                                                .FontSize(35)
                                                .FontColor("#b22222")
                                                .Bold();
                                        });

                                        // "CADA" em linha separada
                                        priceCol.Item().Text("CADA")
                                            .FontSize(14)
                                            .FontColor("#222222")
                                            .Bold()
                                            .AlignCenter();
                                    });

                                }
                            });
                        });
                    });
                }
            });

            return doc.GeneratePdf();
        }

        public byte[] GerarCartazesA4(List<OfertaDTO> ofertas, byte[]? fundo = null, Domain.DTOs.LayoutConfiguracoes? configuracoes = null)
        {
            // Define a licença do QuestPDF como "Community" (gratuita).
            QuestPDF.Settings.License = LicenseType.Community;

            var doc = QDocument.Create(container =>
            {
                // Um cartaz por página A4.
                for (int i = 0; i < ofertas.Count; i++)
                {
                    var oferta = ofertas[i];

                    container.Page(page =>
                    {
                        // Configuração da página em A4 vertical.
                        page.Size(PageSizes.A4.Portrait());
                        page.Margin(0);

                        // Se existir imagem de fundo, aplica.
                        if (fundo != null)
                            page.Background().Image(fundo).FitArea();

                        // Define a fonte padrão como Impact.
                        page.DefaultTextStyle(TextStyle.Default.FontFamily("Impact"));

                        // Cartaz único ocupando toda a página A4
                        page.Content().Column(col =>
                        {
                            // Aplicar configurações de posicionamento se fornecidas
                            var nomeTop = configuracoes != null ? 
                                Math.Max(0, Math.Min(800, (int)((configuracoes.NomeAltura / 100.0) * 800))) : // 800px é a altura total da página
                                (oferta.Descricao.Length <= 30 ? 370 : 340);
                            
                            var nomeLeft = 0; // Centralizado por padrão
                            
                            var precoTop = nomeTop + 200; // Posição padrão
                            
                            var precoLeft = configuracoes != null ? 
                                Math.Max(0, Math.Min(300, (int)((configuracoes.PrecoLateral / 100.0) * 300))) : 
                                0; // Centralizado por padrão
                            
                            // Descrição do produto com posicionamento customizado
                            col.Item().PaddingTop(nomeTop).PaddingLeft(nomeLeft);
                            col.Item().Height(180) // altura fixa para duas linhas
                                    .AlignCenter()
                                    .Column(descCol =>
                                    {
                                        descCol.Item().Text(oferta.Descricao)
                                            .FontSize(70)
                                            .AlignCenter()
                                            .FontColor("#222222")
                                            .Bold()
                                            .LineHeight(1.2f);
                                    });

                            //Espaço entre descrição e preço
                            col.Item().Height(15);

                            // Preço formatado com posicionamento customizado
                            var precoPaddingTop3 = Math.Max(0, precoTop - nomeTop - 195);
                            col.Item().PaddingTop(precoPaddingTop3).PaddingLeft(precoLeft).AlignCenter().Column(priceCol =>
                            {
                                var culture = new CultureInfo("pt-BR");
                                var partes = oferta.Preco.ToString("N2", culture).Split(',');
                                var reais = partes[0];
                                var centavos = partes.Length > 1 ? partes[1] : "00";

                                // Preço em uma linha mais simples
                                priceCol.Item().Row(priceRow =>
                                {
                                    // "R$" 
                                    priceRow.ConstantItem(60).Text("R$").FontSize(50)
                                        .FontColor("#b22222")
                                        .Bold();

                                    // Valor em reais
                                    priceRow.AutoItem().Text(reais)
                                        .FontSize(180)
                                        .FontColor("#b22222")
                                        .Bold();

                                    // Centavos
                                    priceRow.AutoItem().Text("," + centavos)
                                        .FontSize(60)
                                        .FontColor("#b22222")
                                        .Bold();
                                });

                                // "CADA" em linha separada
                                priceCol.Item().Text("CADA")
                                    .FontSize(20)
                                    .FontColor("#222222")
                                    .Bold()
                                    .AlignCenter();
                            });

                        });
                    });
                }
            });

            return doc.GeneratePdf();
        }

        public byte[] GerarCartazesA5(List<OfertaDTO> ofertas, byte[]? fundo = null)
        {
            throw new NotImplementedException();
        }

        public byte[] GerarCartazesA4(List<OfertaDTO> ofertas, byte[]? fundo = null)
        {
            throw new NotImplementedException();
        }
    }
}
