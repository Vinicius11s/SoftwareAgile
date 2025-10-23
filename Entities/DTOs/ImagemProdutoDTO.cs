using System;

namespace Domain.DTOs
{
    public class ImagemProdutoDTO
    {
        public int Id { get; set; }
        public string CodigoBarras { get; set; } = String.Empty;
        public string CaminhoImagem { get; set; } = String.Empty;
        public string NomeArquivo { get; set; } = String.Empty;
        public string UrlOrigem { get; set; } = String.Empty;
        public string FonteImagem { get; set; } = String.Empty;
        public DateTime DataBusca { get; set; }
        public DateTime DataUpload { get; set; }
        public int UsuarioId { get; set; }
        public bool Ativo { get; set; }
        public long TamanhoArquivo { get; set; }
        public string TipoArquivo { get; set; } = String.Empty;
        
        // Propriedades auxiliares para exibição
        public string TamanhoFormatado => FormatFileSize(TamanhoArquivo);
        public string DataBuscaFormatada => DataBusca.ToString("dd/MM/yyyy HH:mm");
        public string DataUploadFormatada => DataUpload.ToString("dd/MM/yyyy HH:mm");
        
        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024:F1} KB";
            if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024 * 1024):F1} MB";
            return $"{bytes / (1024 * 1024 * 1024):F1} GB";
        }
    }
}

