using Domain.DTOs;
using Domain.Entities;
using Interfaces.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infraestructure.Repository;
using Interfaces.Repository;

namespace Services
{
    public class UsuarioService : IUsuarioService
    {
        private IUsuarioRepository repository;
        public UsuarioService(IUsuarioRepository repository) {
            this.repository = repository;
        }

        public bool ValidarLogin(UsuarioDTO model)
        {
            return repository.ValidarLogin(model);
        }

        public Usuario? ObterUsuarioPorLogin(UsuarioDTO model)
        {
            return repository.ObterUsuarioPorLogin(model);
        }
    }
}
