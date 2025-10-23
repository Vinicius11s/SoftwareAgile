using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class CorrecaoAprendida
    {
        public int Id { get; set; }
        public string TextoOriginal { get; set; } = string.Empty;
        public string TextoCorrigido { get; set; } = string.Empty;
        public string TipoCorrecao { get; set; } = string.Empty; // "NOME", "GRAMAGEM", "VARIEDADE"
        public int FrequenciaUso { get; set; } = 1;
        public DateTime DataCriacao { get; set; } = DateTime.Now;
        public DateTime UltimaUtilizacao { get; set; } = DateTime.Now;
        public bool Ativo { get; set; } = true;
        public string UsuarioId { get; set; } = string.Empty; // Para futuras implementações multi-usuário
    }
}


