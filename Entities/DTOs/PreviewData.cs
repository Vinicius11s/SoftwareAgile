using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class PreviewData
    {
        public List<ProcessedOferta> Ofertas { get; set; } = new List<ProcessedOferta>();
        public string FundoSelecionado { get; set; } = string.Empty;
        public string TamanhoSelecionado { get; set; } = string.Empty;
        public byte[] FundoBytes { get; set; } = Array.Empty<byte>();
        public int TotalProdutos { get; set; }
        public int TotalFamilias { get; set; }
        public int TotalCartazes { get; set; }
    }
}

