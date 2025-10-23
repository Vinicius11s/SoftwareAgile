using System;

namespace Domain.DTOs
{
    public class PostWebDTO
    {
        public string CodigoBarras { get; set; } = String.Empty;
        public decimal Preco { get; set; }
        public bool TemImagem { get; set; }
        public byte[]? ImagemBytes { get; set; }
        public ImagemProdutoDTO? ImagemInfo { get; set; }
        public DateTime DataProcessamento { get; set; }
        
        // Propriedades auxiliares
        public string PrecoFormatado => Preco.ToString("C", new System.Globalization.CultureInfo("pt-BR"));
        public string StatusImagem => TemImagem ? "✅ Com Imagem" : "❌ Sem Imagem";
        public string DataFormatada => DataProcessamento.ToString("dd/MM/yyyy HH:mm");
    }
}

