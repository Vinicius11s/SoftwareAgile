using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class HistoricoCorrecoes
    {
        public int Id { get; set; }
        public string TextoOriginal { get; set; } = string.Empty;
        public string TextoCorrigido { get; set; } = string.Empty;
        public string TipoCorrecao { get; set; } = string.Empty;
        public DateTime DataCorrecao { get; set; } = DateTime.Now;
        public string UsuarioId { get; set; } = string.Empty;
        public string SessaoId { get; set; } = string.Empty;
    }
}


