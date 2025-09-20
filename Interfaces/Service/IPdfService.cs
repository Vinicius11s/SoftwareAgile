using Domain.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Service
{
    public interface IPdfService
    {
        public byte[] GerarCartazesA5(List<OfertaDTO> ofertas, byte[]? fundo = null);
        public byte[] GerarCartazesA4(List<OfertaDTO> ofertas, byte[]? fundo = null);
    }
}
