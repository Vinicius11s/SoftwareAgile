using System.Collections.Generic;

namespace Domain.DTOs
{
    public class PostWebPreviewData
    {
        public List<PostWebDTO> Posts { get; set; } = new List<PostWebDTO>();
        public string FundoSelecionado { get; set; } = String.Empty;
        public string TipoSelecionado { get; set; } = String.Empty;
        public int TotalProdutos { get; set; }
        public int TotalComImagens { get; set; }
        public int TotalSemImagens { get; set; }
        
        // Propriedades calculadas
        public double PercentualComImagens => TotalProdutos > 0 ? (double)TotalComImagens / TotalProdutos * 100 : 0;
        public double PercentualSemImagens => TotalProdutos > 0 ? (double)TotalSemImagens / TotalProdutos * 100 : 0;
    }
}

