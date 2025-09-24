using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Domain.DTOs;
using Interfaces.Service;

namespace Services
{
    public class ProductDescriptionProcessor
    {
        private readonly ILearningService _learningService;

        // Regex para identificar gramagens
        private static readonly Regex GramagemRegex = new Regex(@"\b(\d+(?:,\d+)?)\s*(KG|G|L|ML|UN|PCT|CX|DZ|PCT|UND|UNID)\b", 
            RegexOptions.IgnoreCase);
        
        // Regex para identificar tipos/variedades
        private static readonly Regex TipoRegex = new Regex(@"\b(INTEGRAL|DESNATADO|SEMIDESNATADO|T1|T2|T3|TIPO\s*\d+|LARANJA|MARACUJA|UVA|MORANGO|COLA|GUARANA|LIMAO|LIMA|ABACAXI|PESSEGO|MACA|BANANA|CEREJA|FRAMBOESA|MENTA|HORTELA|CITRICO|CITRUS)\b", 
            RegexOptions.IgnoreCase);

        // Dicionário de correção de acentuação
        private static readonly Dictionary<string, string> AcentuacaoCorrecao = new Dictionary<string, string>
        {
            { "ANCELI", "ANCÉLI" },
            { "ITALAC", "ITÁLAC" },
            { "TRINK", "TRINK" }, // Manter como está
            { "REFR", "REFR" }    // Manter como está
        };

        public ProductDescriptionProcessor(ILearningService learningService = null)
        {
            _learningService = learningService;
        }

        public ProcessedOferta ProcessDescription(string descricao, decimal preco)
        {
            var result = new ProcessedOferta
            {
                DescricaoOriginal = descricao,
                Preco = preco,
                Id = Guid.NewGuid().GetHashCode() // ID temporário
            };

            // 1. Corrigir acentuações
            var descricaoCorrigida = CorrigirAcentuacoes(descricao);
            
            // 2. Extrair gramagem
            var gramagemMatch = GramagemRegex.Match(descricaoCorrigida);
            if (gramagemMatch.Success)
            {
                result.Gramagem = gramagemMatch.Value.ToUpper();
                descricaoCorrigida = GramagemRegex.Replace(descricaoCorrigida, "").Trim();
            }
            
            // 3. Extrair variedade/tipo
            var tipoMatch = TipoRegex.Match(descricaoCorrigida);
            if (tipoMatch.Success)
            {
                result.Variedade = tipoMatch.Value.ToUpper();
                descricaoCorrigida = TipoRegex.Replace(descricaoCorrigida, "").Trim();
            }
            
            // 4. Limpar e padronizar nome base
            result.NomeBase = LimparNomeBase(descricaoCorrigida);
            
            // 5. Aplicar correções aprendidas
            if (_learningService != null)
            {
                try
                {
                    result.NomeBase = _learningService.AplicarCorrecoesAprendidas(result.NomeBase, "NOME");
                    result.Gramagem = _learningService.AplicarCorrecoesAprendidas(result.Gramagem, "GRAMAGEM");
                    result.Variedade = _learningService.AplicarCorrecoesAprendidas(result.Variedade, "VARIEDADE");
                }
                catch (Exception ex)
                {
                    // Log do erro mas continua o processamento
                    Console.WriteLine($"Erro ao aplicar correções aprendidas: {ex.Message}");
                }
            }
            
            // 6. Gerar descrição formatada
            result.DescricaoFormatada = result.GerarDescricaoFormatada();
            
            return result;
        }

        private string CorrigirAcentuacoes(string texto)
        {
            var resultado = texto;
            foreach (var correcao in AcentuacaoCorrecao)
            {
                resultado = resultado.Replace(correcao.Key, correcao.Value, StringComparison.OrdinalIgnoreCase);
            }
            return resultado;
        }

        private string LimparNomeBase(string texto)
        {
            // Remove espaços extras e caracteres especiais desnecessários
            var limpo = Regex.Replace(texto, @"\s+", " ").Trim();
            
            // Remove caracteres especiais no final
            limpo = Regex.Replace(limpo, @"[^\w\s]+$", "").Trim();
            
            return limpo.ToUpper();
        }

        public List<ProcessedOferta> GroupByFamily(List<ProcessedOferta> produtos)
        {
            // Agrupa por "nome base" (sem gramagem e variedade)
            var grupos = produtos
                .GroupBy(p => p.NomeBase)
                .ToList();

            var resultado = new List<ProcessedOferta>();

            foreach (var grupo in grupos)
            {
                if (grupo.Count() >= 2) // 2 ou mais produtos = família
                {
                    // Cria um produto "família" consolidado
                    var produtoFamilia = CriarProdutoFamilia(grupo.ToList());
                    resultado.Add(produtoFamilia);
                }
                else
                {
                    // Produto único, mantém como está
                    resultado.AddRange(grupo);
                }
            }

            return resultado;
        }

        private ProcessedOferta CriarProdutoFamilia(List<ProcessedOferta> produtosFamilia)
        {
            // Pega o primeiro produto como base
            var nomeBase = produtosFamilia.First();
            
            // Calcula preço médio
            var precoMedio = produtosFamilia.Average(p => p.Preco);
            
            // Pega a gramagem mais comum (ou a primeira)
            var gramagemComum = produtosFamilia
                .GroupBy(p => p.Gramagem)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? nomeBase.Gramagem;
            
            return new ProcessedOferta
            {
                Id = Guid.NewGuid().GetHashCode(),
                NomeBase = nomeBase.NomeBase,
                Gramagem = gramagemComum,
                Variedade = "", // Remove variedades específicas
                Preco = precoMedio,
                DescricaoOriginal = string.Join(" | ", produtosFamilia.Select(p => p.DescricaoOriginal)),
                IsFamilia = true,
                ProdutosOriginais = produtosFamilia,
                QuantidadeProdutos = produtosFamilia.Count,
                DescricaoFormatada = $"{nomeBase.NomeBase}\n{gramagemComum}"
            };
        }
    }
}
