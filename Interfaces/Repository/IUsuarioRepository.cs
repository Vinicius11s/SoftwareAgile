using Domain.DTOs;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces.Repository
{
    public interface IUsuarioRepository
    {
        public bool ValidarLogin(UsuarioDTO model);
        public Usuario? ObterUsuarioPorLogin(UsuarioDTO model);
    }
}
