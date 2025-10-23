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
        public readonly ILearningService _learningService;

        public CsvServices(ILearningService learningService = null)
        {
            Console.WriteLine($"CsvServices construtor - LearningService: {learningService != null}");
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
            return ProcessarOfertas(stream, null, null);
        }

        public List<ProcessedOferta> ProcessarOfertas(Stream stream, string usuarioId = null, string empresaId = null)
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

                // Processa a descrição (acentuação, gramagem, variedade) com parâmetros de usuário
                var ofertaProcessada = _processor.ProcessDescription(descricao, preco, usuarioId, empresaId);
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
            var ofertas = ofertasProcessadas.Select((o, index) => new OfertaDTO
            {
                Descricao = o.DescricaoFormatada,
                Preco = o.Preco,
                Gramagem = o.Gramagem
            }).ToList();
            
            Console.WriteLine($"=== CONVERSÃO PARA OFERTADTO ===");
            for (int i = 0; i < ofertas.Count; i++)
            {
                Console.WriteLine($"Oferta {i}: '{ofertas[i].Descricao}' - R$ {ofertas[i].Preco}");
            }
            
            return ofertas;
        }

        public List<OfertaDTO> ConverterParaOfertaDTO(List<ProcessedOferta> ofertasProcessadas, string usuarioId = null, string empresaId = null)
        {
            Console.WriteLine($"ConverterParaOfertaDTO - UsuarioId: {usuarioId}, EmpresaId: {empresaId}");
            
            var ofertas = new List<OfertaDTO>();
            
            foreach (var o in ofertasProcessadas)
            {
                // Aplicar correções nos campos individuais antes de formatar
                var nomeBaseCorrigido = o.NomeBase;
                var gramagemCorrigida = o.Gramagem;
                var variedadeCorrigida = o.Variedade;
                
                if (_learningService != null)
                {
                    try
                    {
                        // Aplicar correções nos campos individuais
                        Console.WriteLine($"Aplicando correções para NomeBase: '{o.NomeBase}'");
                        nomeBaseCorrigido = _learningService.AplicarCorrecoesAprendidas(o.NomeBase, "NOME", usuarioId, empresaId);
                        
                        if (!string.IsNullOrEmpty(o.Gramagem))
                        {
                            Console.WriteLine($"Aplicando correções para Gramagem: '{o.Gramagem}'");
                            gramagemCorrigida = _learningService.AplicarCorrecoesAprendidas(o.Gramagem, "GRAMAGEM", usuarioId, empresaId);
                        }
                        
                        if (!string.IsNullOrEmpty(o.Variedade))
                        {
                            Console.WriteLine($"Aplicando correções para Variedade: '{o.Variedade}'");
                            variedadeCorrigida = _learningService.AplicarCorrecoesAprendidas(o.Variedade, "VARIEDADE", usuarioId, empresaId);
                        }
                        
                        // Log das correções aplicadas
                        if (o.NomeBase != nomeBaseCorrigido)
                            Console.WriteLine($"CORREÇÃO NOME APLICADA: '{o.NomeBase}' -> '{nomeBaseCorrigido}'");
                        if (o.Gramagem != gramagemCorrigida)
                            Console.WriteLine($"CORREÇÃO GRAMAGEM APLICADA: '{o.Gramagem}' -> '{gramagemCorrigida}'");
                        if (o.Variedade != variedadeCorrigida)
                            Console.WriteLine($"CORREÇÃO VARIEDADE APLICADA: '{o.Variedade}' -> '{variedadeCorrigida}'");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao aplicar correções na conversão: {ex.Message}");
                    }
                }
                
                // Gerar descrição formatada com os campos corrigidos
                var descricaoFormatada = GerarDescricaoFormatada(nomeBaseCorrigido, gramagemCorrigida, variedadeCorrigida);
                
                ofertas.Add(new OfertaDTO
                {
                    Descricao = descricaoFormatada,
                    Preco = o.Preco,
                    Gramagem = gramagemCorrigida
                });
            }
            
            Console.WriteLine($"=== CONVERSÃO PARA OFERTADTO (COM CORREÇÕES) ===");
            for (int i = 0; i < ofertas.Count; i++)
            {
                Console.WriteLine($"Oferta {i}: '{ofertas[i].Descricao}' - R$ {ofertas[i].Preco}");
            }
            
            return ofertas;
        }
        
        private string GerarDescricaoFormatada(string nomeBase, string gramagem, string variedade)
        {
            var partes = new List<string>();
            
            if (!string.IsNullOrEmpty(nomeBase))
                partes.Add(nomeBase);
                
            if (!string.IsNullOrEmpty(variedade))
                partes.Add(variedade);
                
            if (!string.IsNullOrEmpty(gramagem))
                partes.Add(gramagem);
            
            return string.Join(" ", partes);
        }
    }
}

