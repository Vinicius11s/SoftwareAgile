using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.DTOs;
using Domain.Entities;
using Interfaces.Service;

namespace Services
{
    public class CsvServices : ICsvService
    {
        private readonly ProductDescriptionProcessor _processor;
        private readonly ILearningService _learningService;

        public CsvServices(ILearningService learningService = null)
        {
            _learningService = learningService;
            _processor = new ProductDescriptionProcessor(_learningService);
        }

        public List<OfertaDTO> LerOfertas(Stream stream)
        {
            var config = new CsvConfiguration(new CultureInfo("pt-BR"))
            {
                Delimiter = ";",
                HasHeaderRecord = false, // porque o CSV não tem cabeçalho
                BadDataFound = null
            };

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            var ofertas = new List<OfertaDTO>();
            while (csv.Read())
            {
                var descricao = csv.GetField(0);
                var preco = csv.GetField<decimal>(1);

                ofertas.Add(new OfertaDTO
                {
                    Descricao = descricao,
                    Preco = preco
                });
            }

            return ofertas;
        }

        public List<ProcessedOferta> ProcessarOfertas(Stream stream)
        {
            var config = new CsvConfiguration(new CultureInfo("pt-BR"))
            {
                Delimiter = ";",
                HasHeaderRecord = false,
                BadDataFound = null
            };

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            var ofertasProcessadas = new List<ProcessedOferta>();
            while (csv.Read())
            {
                var descricao = csv.GetField(0);
                var preco = csv.GetField<decimal>(1);

                // Processa a descrição (acentuação, gramagem, variedade)
                var ofertaProcessada = _processor.ProcessDescription(descricao, preco);
                ofertasProcessadas.Add(ofertaProcessada);
            }

            // Agrupa por família
            var ofertasAgrupadas = _processor.GroupByFamily(ofertasProcessadas);

            return ofertasAgrupadas;
        }

        public PreviewData CriarPreviewData(List<ProcessedOferta> ofertas, string fundoSelecionado, string tamanhoSelecionado, byte[] fundoBytes)
        {
            return new PreviewData
            {
                Ofertas = ofertas,
                FundoSelecionado = fundoSelecionado,
                TamanhoSelecionado = tamanhoSelecionado,
                FundoBytes = fundoBytes,
                TotalProdutos = ofertas.Sum(o => o.QuantidadeProdutos),
                TotalFamilias = ofertas.Count(o => o.IsFamilia),
                TotalCartazes = (int)Math.Ceiling(ofertas.Count / 2.0) // 2 cartazes por página
            };
        }

        public List<OfertaDTO> ConverterParaOfertaDTO(List<ProcessedOferta> ofertasProcessadas)
        {
            return ofertasProcessadas.Select(o => new OfertaDTO
            {
                Descricao = o.DescricaoFormatada,
                Preco = o.Preco,
                Gramagem = o.Gramagem
            }).ToList();
        }
    }
}

