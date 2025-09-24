using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    [Table("HistoricoCorrecoes")]
    public class HistoricoCorrecoesEntity
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
        public string TipoCorrecao { get; set; } = string.Empty;

        public DateTime DataCorrecao { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string UsuarioId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string EmpresaId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string SessaoId { get; set; } = string.Empty;

        [MaxLength(100)]
        public string IpAddress { get; set; } = string.Empty;

        [MaxLength(500)]
        public string UserAgent { get; set; } = string.Empty;
    }
}

