using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class OfertaDTO
    {
        public int Id { get; set; }
        public string? Descricao { get; set; } = string.Empty;
        public string Gramagem { get; set; } = string.Empty;
        public decimal Preco { get; set; }
    }
}
