using Domain.DTOs;
using Interfaces.Service;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace Agile.Controllers
{
    public class LoginController : Controller
    {
        private IUsuarioService usuarioService;

        public LoginController(IUsuarioService usuarioService)
        {
            this.usuarioService = usuarioService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ValidarLogin(UsuarioDTO model)
        {
            // Valida as anotações do DTO (por exemplo, se o campo é obrigatório)
            if (!ModelState.IsValid)
            {
                return View("Index", model); // Retorna com erro se o DTO for inválido
            }

            // Chama a camada de serviço para validar as credenciais
            if (usuarioService.ValidarLogin(model))
            {
                // Obter dados completos do usuário
                var usuario = usuarioService.ObterUsuarioPorLogin(model);
                if (usuario != null)
                {
                    // Armazenar dados do usuário na sessão
                    HttpContext.Session.SetString("UsuarioLogado", "true");
                    HttpContext.Session.SetString("NomeUsuario", usuario.Nome);
                    HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                    
                    // Definir EmpresaId baseado no usuário (por enquanto, usar o ID do usuário como empresa)
                    // TODO: Implementar sistema de empresas quando necessário
                    HttpContext.Session.SetString("EmpresaId", usuario.Id.ToString());
                }

                // Se o login for bem-sucedido, redireciona para a página inicial
                return RedirectToAction("EscolherTamanho", "Cartazes");
            }
            else
            {
                // Se o login falhar, adiciona uma mensagem de erro e retorna para a tela de login
                ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
                return View("Index", model);
            }
        }

        public IActionResult Logout()
        {
            // Limpar a sessão
            HttpContext.Session.Clear();
            
            // Redirecionar para a página inicial
            return RedirectToAction("Index", "Agile");
        }
    }
}


