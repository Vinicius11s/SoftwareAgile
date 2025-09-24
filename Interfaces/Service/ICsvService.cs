using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;

namespace Interfaces.Service
{
    public interface ICsvService
    {
        public List<OfertaDTO> LerOfertas(Stream stream);
        public List<ProcessedOferta> ProcessarOfertas(Stream stream);
        public PreviewData CriarPreviewData(List<ProcessedOferta> ofertas, string fundoSelecionado, string tamanhoSelecionado, byte[] fundoBytes);
        public List<OfertaDTO> ConverterParaOfertaDTO(List<ProcessedOferta> ofertasProcessadas);
    }
}
