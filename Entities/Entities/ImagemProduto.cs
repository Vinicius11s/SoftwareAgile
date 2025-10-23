using System;

namespace Domain.Entities
{
    public class ImagemProduto
    {
        public int Id { get; set; }
        public string CodigoBarras { get; set; } = String.Empty;
        public string CaminhoImagem { get; set; } = String.Empty;
        public string NomeArquivo { get; set; } = String.Empty;
        public string UrlOrigem { get; set; } = String.Empty; // Para rastrear origem da imagem
        public string FonteImagem { get; set; } = String.Empty; // Google, OpenFoodFacts, etc.
        public DateTime DataBusca { get; set; }
        public DateTime DataUpload { get; set; }
        public int UsuarioId { get; set; }
        public bool Ativo { get; set; } = true;
        public long TamanhoArquivo { get; set; } // Tamanho em bytes
        public string TipoArquivo { get; set; } = String.Empty; // jpg, png, etc.
        
        // Navegação
        public Usuario Usuario { get; set; } = null!;
    }
}

