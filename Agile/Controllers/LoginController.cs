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
                // Se o login for bem-sucedido, redireciona para a página inicial
                return RedirectToAction("Index", "Cartaz");
            }
            else
            {
                // Se o login falhar, adiciona uma mensagem de erro e retorna para a tela de login
                ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
                return View("Index", model);
            }
        }
    }
}


