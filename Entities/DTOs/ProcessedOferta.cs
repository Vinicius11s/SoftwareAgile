using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Domain.DTOs
{
    public class ProcessedOferta
    {
        public int Id { get; set; }
        public string NomeBase { get; set; } = string.Empty;
        public string Gramagem { get; set; } = string.Empty;
        public string Variedade { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string DescricaoOriginal { get; set; } = string.Empty;
        public string DescricaoFormatada { get; set; } = string.Empty; // "NOME\nGRAMAGEM"
        public bool IsFamilia { get; set; }
        [JsonIgnore]
        public List<ProcessedOferta> ProdutosOriginais { get; set; } = new List<ProcessedOferta>();
        public int QuantidadeProdutos { get; set; } = 1;

        public string GerarDescricaoFormatada()
        {
            return GerarDescricaoFormatada(2); // Padrão: 2 linhas
        }

        public string GerarDescricaoParaCartaz()
        {
            // Versão otimizada para cartazes A5 com fonte Impact 45px
            // Limita primeira linha a 16 caracteres, considerando espaço da gramagem
            return GerarDescricaoFormatadaParaCartaz();
        }

        private string GerarDescricaoFormatadaParaCartaz()
        {
            var linhas = new List<string>();
            
            // Monta a descrição completa
            var descricaoCompleta = new List<string>();
            if (!string.IsNullOrEmpty(NomeBase))
                descricaoCompleta.Add(NomeBase);
            if (!string.IsNullOrEmpty(Variedade))
                descricaoCompleta.Add(Variedade);
            if (!string.IsNullOrEmpty(Gramagem))
                descricaoCompleta.Add(Gramagem);
            
            var textoCompleto = string.Join(" ", descricaoCompleta);
            
            Console.WriteLine($"=== DEBUG FORMATACAO ===");
            Console.WriteLine($"NomeBase: '{NomeBase}' ({NomeBase?.Length ?? 0} chars)");
            Console.WriteLine($"Variedade: '{Variedade}' ({Variedade?.Length ?? 0} chars)");
            Console.WriteLine($"Gramagem: '{Gramagem}' ({Gramagem?.Length ?? 0} chars)");
            Console.WriteLine($"Texto completo: '{textoCompleto}' ({textoCompleto.Length} chars)");
            
            // Se a descrição completa tem 32 caracteres ou menos, distribui em 2 linhas
            if (textoCompleto.Length <= 32)
            {
                // Calcula nome + variedade para decidir onde colocar gramagem
                var nomeVariedade = new List<string>();
                if (!string.IsNullOrEmpty(NomeBase))
                    nomeVariedade.Add(NomeBase);
                if (!string.IsNullOrEmpty(Variedade))
                    nomeVariedade.Add(Variedade);
                
                var textoNomeVariedade = string.Join(" ", nomeVariedade);
                
                if (textoNomeVariedade.Length <= 12)
                {
                    // Nome + Variedade ≤ 12 chars: Gramagem vai para linha 2
                    // Linha 1: Nome + Variedade (máximo 16 caracteres)
                    if (textoNomeVariedade.Length > 16)
                    {
                        linhas.Add(textoNomeVariedade.Substring(0, 16));
                    }
                    else
                    {
                        linhas.Add(textoNomeVariedade);
                    }
                    
                    // Linha 2: Só gramagem (mas evite gramagem sozinha se linha 1 couber com gramagem)
                    if (!string.IsNullOrEmpty(Gramagem))
                    {
                        var tentativaLinha1 = (textoNomeVariedade + " " + Gramagem).Trim();
                        if (tentativaLinha1.Length <= 16)
                        {
                            // Melhor manter tudo na primeira linha
                            linhas.Clear();
                            linhas.Add(tentativaLinha1);
                        }
                        else
                        {
                            linhas.Add(Gramagem);
                        }
                    }
                }
                else
                {
                    // Nome + Variedade > 12 chars: Gramagem fica na mesma linha
                    // Linha 1: Nome + Variedade + Gramagem (máximo 16 caracteres)
                    var linha1Completa = textoNomeVariedade + (string.IsNullOrEmpty(Gramagem) ? "" : " " + Gramagem);
                    
                    if (linha1Completa.Length > 16)
                    {
                        // Quebra inteligente: procura o último espaço antes de 16 caracteres
                        var linha1 = QuebrarInteligente(linha1Completa, 16);
                        linhas.Add(linha1);
                        
                        // Linha 2: Resto se houver
                        var resto = linha1Completa.Substring(linha1.Length).Trim();
                        if (!string.IsNullOrEmpty(resto))
                        {
                            linhas.Add(resto);
                        }
                    }
                    else
                    {
                        linhas.Add(linha1Completa);
                    }
                }
            }
            else
            {
                // Se ultrapassar 32 caracteres, aplica lógica inteligente
                // Calcula nome + variedade para decidir onde colocar gramagem
                var nomeVariedade = new List<string>();
                if (!string.IsNullOrEmpty(NomeBase))
                    nomeVariedade.Add(NomeBase);
                if (!string.IsNullOrEmpty(Variedade))
                    nomeVariedade.Add(Variedade);
                
                var textoNomeVariedade = string.Join(" ", nomeVariedade);
                
                if (textoNomeVariedade.Length <= 12)
                {
                    // Nome + Variedade ≤ 12 chars: Gramagem vai para linha 2
                    // Linha 1: Nome + Variedade (máximo 16 caracteres)
                    if (textoNomeVariedade.Length > 16)
                    {
                        linhas.Add(textoNomeVariedade.Substring(0, 16));
                    }
                    else
                    {
                        linhas.Add(textoNomeVariedade);
                    }
                    
                    // Linha 2: Só gramagem
                    if (!string.IsNullOrEmpty(Gramagem))
                    {
                        linhas.Add(Gramagem);
                    }
                }
                else
                {
                    // Nome + Variedade > 12 chars: Gramagem fica na mesma linha
                    // Linha 1: Nome + Variedade + Gramagem (máximo 16 caracteres)
                    var linha1Completa = textoNomeVariedade + (string.IsNullOrEmpty(Gramagem) ? "" : " " + Gramagem);
                    
                    if (linha1Completa.Length > 16)
                    {
                        // Quebra inteligente: procura o último espaço antes de 16 caracteres
                        var linha1 = QuebrarInteligente(linha1Completa, 16);
                        linhas.Add(linha1);
                        
                        // Linha 2: Resto se houver
                        var resto = linha1Completa.Substring(linha1.Length).Trim();
                        if (!string.IsNullOrEmpty(resto))
                        {
                            linhas.Add(resto);
                        }
                    }
                    else
                    {
                        linhas.Add(linha1Completa);
                    }
                }
            }
            
            return string.Join("\n", linhas);
        }

        public string GerarDescricaoFormatada(int maxLinhas, int maxCaracteresPorLinha = 25)
        {
            var linhas = new List<string>();
            
            // Linha 1: Nome base (pode ser quebrado se muito longo)
            if (!string.IsNullOrEmpty(NomeBase))
            {
                linhas.Add(NomeBase);
            }
            
            // Linha 2: Variedade + Gramagem
            var linha2 = new List<string>();
            if (!string.IsNullOrEmpty(Variedade))
                linha2.Add(Variedade);
            if (!string.IsNullOrEmpty(Gramagem))
                linha2.Add(Gramagem);
            
            if (linha2.Any())
            {
                linhas.Add(string.Join(" ", linha2));
            }
            
            // Se temos mais de maxLinhas ou linhas muito longas, precisamos reorganizar
            if (linhas.Count > maxLinhas || linhas.Any(l => l.Length > maxCaracteresPorLinha))
            {
                return ReorganizarParaMaxLinhas(linhas, maxLinhas, maxCaracteresPorLinha);
            }
            
            return string.Join("\n", linhas);
        }

        private string ReorganizarParaMaxLinhas(List<string> linhas, int maxLinhas, int maxCaracteresPorLinha = 25)
        {
            if (maxLinhas < 2) return string.Join("\n", linhas.Take(1));
            
            var resultado = new List<string>();
            
            // Linha 1: Primeira parte do nome
            var nomeBase = linhas[0];
            if (nomeBase.Length <= maxCaracteresPorLinha)
            {
                resultado.Add(nomeBase);
            }
            else
            {
                // Quebra o nome em duas partes
                var meio = nomeBase.Length / 2;
                var espaco = nomeBase.LastIndexOf(' ', meio);
                if (espaco > 0)
                {
                    resultado.Add(nomeBase.Substring(0, espaco));
                    nomeBase = nomeBase.Substring(espaco + 1);
                }
                else
                {
                    resultado.Add(nomeBase.Substring(0, meio));
                    nomeBase = nomeBase.Substring(meio);
                }
            }
            
            // Linha 2: Resto do nome + variedade + gramagem
            var linha2 = new List<string>();
            if (!string.IsNullOrEmpty(nomeBase))
                linha2.Add(nomeBase);
            
            // Adiciona variedade e gramagem se couber
            if (linhas.Count > 1)
            {
                var variedadeGramagem = linhas[1];
                var linha2Texto = string.Join(" ", linha2);
                
                // Se a linha 2 ficar muito longa, quebra
                if (linha2Texto.Length + variedadeGramagem.Length + 1 <= maxCaracteresPorLinha)
                {
                    linha2.Add(variedadeGramagem);
                }
                else
                {
                     // Se não couber, tenta colocar variedade + gramagem separadamente
                    if (!string.IsNullOrEmpty(Variedade))
                    {
                        var variedadeSolo = string.Join(" ", linha2) + " " + Variedade;
                        if (variedadeSolo.Length <= maxCaracteresPorLinha)
                        {
                            linha2.Add(Variedade);
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(Gramagem))
                    {
                        var gramagemSolo = string.Join(" ", linha2) + " " + Gramagem;
                        if (gramagemSolo.Length <= maxCaracteresPorLinha)
                        {
                            linha2.Add(Gramagem);
                        }
                    }
                }
            }
            
            resultado.Add(string.Join(" ", linha2));
            
            return string.Join("\n", resultado);
        }


        private string QuebrarInteligente(string texto, int maxCaracteres)
        {
            if (texto.Length <= maxCaracteres)
                return texto;

            // Procura o último espaço antes do limite
            var ultimoEspaco = texto.LastIndexOf(' ', maxCaracteres);
            
            if (ultimoEspaco > 0)
            {
                // Quebra no último espaço encontrado
                return texto.Substring(0, ultimoEspaco);
            }
            else
            {
                // Se não encontrar espaço, quebra no limite (evita palavra cortada)
                return texto.Substring(0, maxCaracteres);
            }
        }
    }
}

