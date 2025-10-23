using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("CorrecoesAprendidas")]
    public class CorrecaoAprendidaEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string TextoOriginal { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string TextoCorrigido { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TipoCorrecao { get; set; } = string.Empty; // "NOME", "GRAMAGEM", "VARIEDADE"

        public int FrequenciaUso { get; set; } = 1;

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public DateTime UltimaUtilizacao { get; set; } = DateTime.Now;

        public bool Ativo { get; set; } = true;

        [MaxLength(100)]
        public string UsuarioId { get; set; } = string.Empty; // Para identificar o usuário/cliente

        [MaxLength(100)]
        public string EmpresaId { get; set; } = string.Empty; // Para identificar a empresa

        // Propriedades de auditoria
        public DateTime DataAtualizacao { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string UsuarioAtualizacao { get; set; } = string.Empty;
    }
}


