using Domain.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Service
{
    public interface IUsuarioService
    {
        bool ValidarLogin(UsuarioDTO model);
        Usuario? ObterUsuarioPorLogin(UsuarioDTO model);
    }
}
