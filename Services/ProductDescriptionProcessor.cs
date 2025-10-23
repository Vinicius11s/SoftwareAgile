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
        
        // Regex para identificar tipos/variedades (inclui LATA, ORIGINAL, ZERO, DIET, LIGHT)
        private static readonly Regex TipoRegex = new Regex(@"\b(INTEGRAL|DESNATADO|SEMIDESNATADO|T1|T2|T3|TIPO\s*\d+|LARANJA|MARACUJA|UVA|MORANGO|COLA|GUARANA|LIMAO|LIMA|ABACAXI|PESSEGO|MACA|BANANA|CEREJA|FRAMBOESA|MENTA|HORTELA|CITRICO|CITRUS|LATA|ORIGINAL|ZERO|DIET|LIGHT)\b", 
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

        public ProcessedOferta ProcessDescription(string descricao, decimal preco, string usuarioId = null, string empresaId = null)
        {
            Console.WriteLine($"ProductDescriptionProcessor.ProcessDescription: '{descricao}' (UsuarioId: {usuarioId}, EmpresaId: {empresaId})");
            Console.WriteLine($"LearningService disponível: {_learningService != null}");
            
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
                Console.WriteLine($"EXTRAÇÃO VARIEDADE: '{tipoMatch.Value.ToUpper()}' extraído de '{descricaoCorrigida}'");
                descricaoCorrigida = TipoRegex.Replace(descricaoCorrigida, "").Trim();
            }
            
            // 4. Limpar e padronizar nome base
            result.NomeBase = LimparNomeBase(descricaoCorrigida);
            
            // 5. Aplicar correções aprendidas
            if (_learningService != null)
            {
                try
                {
                    var nomeAntes = result.NomeBase;
                    var gramagemAntes = result.Gramagem;
                    var variedadeAntes = result.Variedade;
                    
                    result.NomeBase = _learningService.AplicarCorrecoesAprendidas(result.NomeBase, "NOME", usuarioId, empresaId);
                    result.Gramagem = _learningService.AplicarCorrecoesAprendidas(result.Gramagem, "GRAMAGEM", usuarioId, empresaId);
                    result.Variedade = _learningService.AplicarCorrecoesAprendidas(result.Variedade, "VARIEDADE", usuarioId, empresaId);
                    
                    // Debug das correções aplicadas
                    if (nomeAntes != result.NomeBase)
                        Console.WriteLine($"CORREÇÃO NOME APLICADA: '{nomeAntes}' -> '{result.NomeBase}' (UsuarioId: {usuarioId}, EmpresaId: {empresaId})");
                    if (gramagemAntes != result.Gramagem)
                        Console.WriteLine($"CORREÇÃO GRAMAGEM APLICADA: '{gramagemAntes}' -> '{result.Gramagem}' (UsuarioId: {usuarioId}, EmpresaId: {empresaId})");
                    if (variedadeAntes != result.Variedade)
                        Console.WriteLine($"CORREÇÃO VARIEDADE APLICADA: '{variedadeAntes}' -> '{result.Variedade}' (UsuarioId: {usuarioId}, EmpresaId: {empresaId})");
                }
                catch (Exception ex)
                {
                    // Log do erro mas continua o processamento
                    Console.WriteLine($"Erro ao aplicar correções aprendidas: {ex.Message}");
                }
            }
            
            // 6. Gerar descrição formatada otimizada para cartazes
            result.DescricaoFormatada = result.GerarDescricaoParaCartaz();
            
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
            Console.WriteLine($"=== AGRUPAMENTO POR FAMÍLIA ===");
            Console.WriteLine($"Produtos recebidos: {produtos.Count}");
            for (int i = 0; i < produtos.Count; i++)
            {
                Console.WriteLine($"Produto {i}: '{produtos[i].NomeBase}' - R$ {produtos[i].Preco}");
            }
            
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

            Console.WriteLine($"=== RESULTADO FINAL ===");
            Console.WriteLine($"Produtos finais: {resultado.Count}");
            for (int i = 0; i < resultado.Count; i++)
            {
                Console.WriteLine($"Produto {i}: '{resultado[i].NomeBase}' - R$ {resultado[i].Preco} - IsFamilia: {resultado[i].IsFamilia}");
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
            
            // Determina se deve mostrar variedades ou não
            var variedadesUnicas = produtosFamilia.Select(p => p.Variedade).Where(v => !string.IsNullOrEmpty(v)).Distinct().ToList();
            var variedadeFinal = variedadesUnicas.Count == 1 ? variedadesUnicas.First() : "";
            
            var produtoFamilia = new ProcessedOferta
            {
                Id = Guid.NewGuid().GetHashCode(),
                NomeBase = nomeBase.NomeBase,
                Gramagem = gramagemComum,
                Variedade = variedadeFinal, // Preserva variedade se todas forem iguais
                Preco = precoMedio,
                DescricaoOriginal = string.Join(" | ", produtosFamilia.Select(p => p.DescricaoOriginal)),
                IsFamilia = true,
                ProdutosOriginais = produtosFamilia,
                QuantidadeProdutos = produtosFamilia.Count
            };
            
            // Gera a descrição formatada usando o método otimizado
            produtoFamilia.DescricaoFormatada = produtoFamilia.GerarDescricaoParaCartaz();
            
            Console.WriteLine($"Família criada: Nome='{produtoFamilia.NomeBase}', Variedade='{produtoFamilia.Variedade}', Gramagem='{produtoFamilia.Gramagem}', Preço={produtoFamilia.Preco}");
            Console.WriteLine($"Descrição formatada: '{produtoFamilia.DescricaoFormatada}'");
            
            return produtoFamilia;
        }
    }
}
