using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public List<ProcessedOferta> ProdutosOriginais { get; set; } = new List<ProcessedOferta>();
        public int QuantidadeProdutos { get; set; } = 1;

        public string GerarDescricaoFormatada()
        {
            var linhas = new List<string>();
            
            // Linha 1: Nome base
            if (!string.IsNullOrEmpty(NomeBase))
                linhas.Add(NomeBase);
            
            // Linha 2: Variedade + Gramagem
            var linha2 = new List<string>();
            if (!string.IsNullOrEmpty(Variedade))
                linha2.Add(Variedade);
            if (!string.IsNullOrEmpty(Gramagem))
                linha2.Add(Gramagem);
            
            if (linha2.Any())
                linhas.Add(string.Join(" ", linha2));
            
            return string.Join("\n", linhas);
        }
    }
}

