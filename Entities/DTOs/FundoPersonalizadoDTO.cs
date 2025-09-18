using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class FundoPersonalizadoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = String.Empty;
        public string CaminhoImagem { get; set; } = String.Empty;
        public string NomeArquivo { get; set; } = String.Empty;
        public DateTime DataUpload { get; set; }
        public int UsuarioId { get; set; }
        public string TipoImpressao { get; set; } = String.Empty;
        public bool Ativo { get; set; }
    }
}
