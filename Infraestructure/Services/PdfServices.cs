using Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.Globalization;
using QDocument = QuestPDF.Fluent.Document;

namespace Infra.Services
{
    public class PdfServices
    {
        public byte[] GerarCartazes(List<Oferta> ofertas, byte[]? fundo = null)
        {
            // Define a licença do QuestPDF como "Community" (gratuita).
            QuestPDF.Settings.License = LicenseType.Community;

            // Inicia a criação do documento PDF usando o método 'Create' do QuestPDF.
            // O 'container' é o construtor do documento.
            var doc = QDocument.Create(container =>
            {
                // Inicia um loop 'for' para processar as ofertas em pares.
                // O loop avança de 2 em 2 (i += 2) para criar duas ofertas por página.
                for (int i = 0; i < ofertas.Count; i += 2)
                {
                    // Obtém a primeira oferta do par.
                    var oferta1 = ofertas[i];
                    // Obtém a segunda oferta do par, verificando se ela existe para evitar um erro de índice fora dos limites.
                    var oferta2 = i + 1 < ofertas.Count ? ofertas[i + 1] : null;

                    // Define uma nova página no documento.
                    container.Page(page =>
                    {
                        // Define o tamanho da página como A4 e a orientação como paisagem (Landscape).
                        page.Size(PageSizes.A4.Landscape());
                        // Define as margens da página como zero.
                        page.Margin(0);

                        // Verifica se um fundo (imagem de fundo) foi fornecido.
                        if (fundo != null)
                            // Se sim, define a imagem de fundo da página.
                            page.Background().Image(fundo).FitArea();

                        // Define a fonte padrão para todo o texto do documento como "Impact".
                        page.DefaultTextStyle(TextStyle.Default.FontFamily("Impact"));

                        // Inicia a área de conteúdo da página.
                        page.Content()
                            // Adiciona um espaçamento horizontal de 15 unidades.
                            .PaddingHorizontal(15)
                            // Adiciona um espaçamento superior de 300 unidades.
                            .PaddingTop(300)
                            // Organiza o conteúdo em uma linha horizontal.
                            .Row(row =>
                            {
                                // Cria uma coluna relativa (que ocupa metade do espaço disponível) para a oferta da esquerda.
                                row.RelativeItem().Column(col =>
                                {
                                    // Adiciona um item (que será a primeira oferta) na coluna.
                                    col.Item()
                                        // Adiciona um espaçamento inferior de 20 unidades.
                                        .PaddingBottom(20)
                                        // Organiza o conteúdo em uma sub-coluna para a descrição e o preço.
                                        .Column(sub =>
                                        {
                                            // Adiciona um item (a descrição da oferta) na sub-coluna.
                                            sub.Item()
                                                // Adiciona um espaçamento inferior de 5 unidades.
                                                .PaddingBottom(5)
                                                // Centraliza o texto horizontalmente.
                                                .AlignCenter()
                                                // Define o texto com a descrição da oferta.
                                                .Text(oferta1.Descricao)
                                                // Define o tamanho da fonte.
                                                .FontSize(40)
                                                // Define a cor da fonte.
                                                .FontColor("#222222")
                                                // Deixa o texto em negrito.
                                                .Bold()
                                                // Define a altura da linha.
                                                .LineHeight(1.2f);

                                            // Adiciona um item para o preço, centralizando-o verticalmente.
                                            sub.Item().AlignCenter().Row(priceRow =>
                                            {
                                                // Define o espaçamento entre os elementos da linha do preço.
                                                priceRow.Spacing(2);

                                                // Cria um objeto 'CultureInfo' para formatação em português do Brasil.
                                                var culture = new CultureInfo("pt-BR");
                                                // Formata o preço com duas casas decimais e separa a parte inteira e a decimal usando a vírgula.
                                                var partes = oferta1.Preco.ToString("N2", culture).Split(',');
                                                // Armazena a parte inteira do preço.
                                                var reais = partes[0];
                                                // Armazena a parte decimal (centavos), garantindo "00" se não houver.
                                                var centavos = partes.Length > 1 ? partes[1] : "00";

                                                // Adiciona o "R$" alinhado à direita. O "item" agrupa o conteúdo.
                                                priceRow.RelativeItem().AlignRight().Text("R$")
                                                    .FontSize(30)
                                                    .FontColor("#b22222")
                                                    .Bold();

                                                // Adiciona os reais alinhados à direita.
                                                priceRow.RelativeItem().AlignRight().Text(reais)
                                                    .FontSize(130)
                                                    .FontColor("#b22222")
                                                    .Bold();

                                                // Adiciona o container para os centavos e "cada", alinhado à esquerda.
                                                priceRow.RelativeItem().AlignTop().Column(cc =>
                                                {
                                                    cc.Item().Text("," + centavos)
                                                        .FontSize(46)
                                                        .FontColor("#b22222")
                                                        .Bold();

                                                    cc.Item().Text("cada")
                                                        .FontSize(28)
                                                        .FontColor("#222222");
                                                });
                                            });
                                        });
                                });

                                // Cria uma coluna relativa (que ocupa a outra metade do espaço disponível) para a oferta da direita.
                                row.RelativeItem().Column(col =>
                                {
                                    // Verifica se a segunda oferta existe.
                                    if (oferta2 != null)
                                    {
                                        // Repete a mesma lógica de layout da primeira oferta.
                                        col.Item()
                                            .PaddingBottom(20)
                                            .Column(sub =>
                                            {
                                                sub.Item()
                                                    .PaddingBottom(5)
                                                    .AlignCenter()
                                                    .Text(oferta2.Descricao)
                                                    .FontSize(48)
                                                    .FontColor("#222222")
                                                    .Bold()
                                                    .LineHeight(1.2f);

                                                sub.Item().AlignCenter().Row(priceRow =>
                                                {
                                                    priceRow.Spacing(2);

                                                    var culture = new CultureInfo("pt-BR");
                                                    var partes = oferta2.Preco.ToString("N2", culture).Split(',');
                                                    var reais = partes[0];
                                                    var centavos = partes.Length > 1 ? partes[1] : "00";

                                                    priceRow.RelativeItem().AlignMiddle().Text("R$")
                                                        .FontSize(30)
                                                        .FontColor("#b22222")
                                                        .Bold();

                                                    priceRow.RelativeItem().AlignMiddle().Text(reais)
                                                        .FontSize(90)
                                                        .FontColor("#b22222")
                                                        .Bold();

                                                    priceRow.RelativeItem().AlignMiddle().Column(cc =>
                                                    {
                                                        cc.Item().Text("," + centavos)
                                                            .FontSize(36)
                                                            .FontColor("#b22222")
                                                            .Bold();

                                                        cc.Item().Text("cada")
                                                            .FontSize(28)
                                                            .FontColor("#222222");
                                                    });
                                                });
                                            });
                                    }
                                });
                            });
                    });
                }
            });

            // Gera o PDF a partir do documento criado e retorna o resultado como um array de bytes.
            return doc.GeneratePdf();
        }
    }
}