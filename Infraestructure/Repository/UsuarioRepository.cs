using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;
using Domain.Entities;
using Interfaces.Repository;
using Infraestructure.Context;
using Microsoft.EntityFrameworkCore;


namespace Infraestructure.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly EmpresaContexto contexto;
        public UsuarioRepository(EmpresaContexto contexto) { this.contexto = contexto; }

        public bool ValidarLogin(UsuarioDTO model)
        {

            if (model == null) return false;

            // credencial "chumbada"
            if (model.Login == "admin" && model.Senha == "123456")
                return true;

            // fallback para verificação no banco
            return contexto.Usuarios.Any(u => u.Login == model.Login && u.Senha == model.Senha);

        }

        public Usuario? ObterUsuarioPorLogin(UsuarioDTO model)
        {
            return contexto.Usuarios.FirstOrDefault(u => u.Login == model.Login && u.Senha == model.Senha);
        }
    }
}
