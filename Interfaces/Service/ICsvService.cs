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
    }
}
