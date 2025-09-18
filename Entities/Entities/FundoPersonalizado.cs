using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class FundoPersonalizado
    {
        public int Id { get; set; }
        public string Nome { get; set; } = String.Empty;
        public string CaminhoImagem { get; set; } = String.Empty;
        public string NomeArquivo { get; set; } = String.Empty;
        public DateTime DataUpload { get; set; }
        public int UsuarioId { get; set; }
        public string TipoImpressao { get; set; } = String.Empty; // A4, A5, etc.
        public bool Ativo { get; set; } = true;
        
        // Navegação
        public Usuario Usuario { get; set; } = null!;
    }
}
