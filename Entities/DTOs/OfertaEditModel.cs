using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class OfertaEditModel
    {
        public int Id { get; set; }
        public string NomeBase { get; set; } = string.Empty;
        public string Gramagem { get; set; } = string.Empty;
        public string Variedade { get; set; } = string.Empty;
        public string PrecoString { get; set; } = string.Empty; // Recebe como string
        public bool IsFamilia { get; set; }
        public int QuantidadeProdutos { get; set; } = 1;

        public decimal Preco
        {
            get
            {
                if (string.IsNullOrEmpty(PrecoString))
                    return 0;

                // Remove espaços e caracteres especiais
                var precoLimpo = PrecoString.Trim().Replace("R$", "").Replace(" ", "");
                
                // Tenta parse com ponto como separador decimal
                if (decimal.TryParse(precoLimpo, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal result))
                    return result;
                
                // Tenta parse com vírgula como separador decimal (formato brasileiro)
                if (decimal.TryParse(precoLimpo.Replace(".", "").Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal resultBR))
                    return resultBR;
                
                return 0;
            }
        }
    }
}
