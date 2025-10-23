using Microsoft.AspNetCore.Mvc;
using Interfaces.Service;
using Microsoft.AspNetCore.Hosting;

namespace Agile.Controllers
{
    public class FundosController : Controller
    {
        private readonly IFundoPersonalizadoService _fundoService;
        private readonly IWebHostEnvironment _env;

        public FundosController(IFundoPersonalizadoService fundoService, IWebHostEnvironment env)
        {
            _fundoService = fundoService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> EscolherFundo(string tamanho)
        {
            // Verificar se usuário está logado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!usuarioId.HasValue)
                return RedirectToAction("Index", "Login");

            // Passar o tamanho para a view
            ViewData["TamanhoSelecionado"] = tamanho;

            // Carregar fundos padrão e personalizados do usuário
            var fundos = new List<(string Nome, string Imagem, string Id)>();
            
            if (tamanho == "A4")
            {
                fundos.Add(("Cartaz Padrão", "~/fundos/fundoPadraoA4.png", "padrao"));
                var fundosPersonalizados = await _fundoService.ObterFundosParaViewPorTipo(usuarioId.Value, tamanho);
                fundos.AddRange(fundosPersonalizados);
            }
            else if (tamanho == "A5")
            {
                fundos.Add(("Cartaz Padrão", "~/fundos/fundoPadraoA5.png", "padrao"));
                var fundosPersonalizados = await _fundoService.ObterFundosParaViewPorTipo(usuarioId.Value, tamanho);
                fundos.AddRange(fundosPersonalizados);
            }
            else
            {
                return BadRequest("Selecione um Tamanho");
            }

            // Adicionar cache-busting para evitar cache do navegador
            ViewData["CacheBuster"] = DateTime.Now.Ticks;
            return View("~/Views/Fundos/EscolherFundo.cshtml", fundos);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFundoPersonalizado(IFormFile arquivo, string nome, string tamanho)
        {
            try
            {
                // Verificar se usuário está logado
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (!usuarioId.HasValue)
                    return Json(new { success = false, message = "Usuário não logado" });

                if (arquivo == null || arquivo.Length == 0)
                    return Json(new { success = false, message = "Selecione um arquivo" });

                if (string.IsNullOrEmpty(nome))
                    return Json(new { success = false, message = "Digite um nome para o fundo" });

                // Validar tipo de arquivo
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(arquivo.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return Json(new { success = false, message = "Apenas arquivos JPG, JPEG e PNG são permitidos" });

                // Validar tamanho do arquivo (máximo 5MB)
                if (arquivo.Length > 5 * 1024 * 1024)
                    return Json(new { success = false, message = "Arquivo muito grande. Máximo 5MB" });

                // Converter para bytes
                byte[] arquivoBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await arquivo.CopyToAsync(memoryStream);
                    arquivoBytes = memoryStream.ToArray();
                }

                // Adicionar fundo personalizado
                var fundoAdicionado = await _fundoService.AdicionarFundo(
                    arquivoBytes, 
                    arquivo.FileName, 
                    nome, 
                    tamanho, 
                    usuarioId.Value, 
                    _env.WebRootPath
                );

                return Json(new { 
                    success = true, 
                    message = "Fundo personalizado adicionado com sucesso!",
                    fundo = new {
                        nome = fundoAdicionado.Nome,
                        imagem = fundoAdicionado.CaminhoImagem,
                        id = fundoAdicionado.Id.ToString()
                    }
                });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro interno: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ObterFundoPersonalizado(int id)
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (!usuarioId.HasValue)
                    return Json(new { success = false, message = "Usuário não logado" });

                var fundo = await _fundoService.ObterFundoPorId(id, usuarioId.Value);
                if (fundo == null)
                    return Json(new { success = false, message = "Fundo não encontrado" });

                return Json(new { 
                    success = true, 
                    fundo = new {
                        id = fundo.Id,
                        nome = fundo.Nome,
                        caminhoImagem = fundo.CaminhoImagem
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro interno: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExcluirFundo([FromBody] ExcluirFundoRequest request)
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                if (!usuarioId.HasValue)
                    return Json(new { success = false, message = "Usuário não logado" });

                // Verificar se é um fundo padrão (não pode ser excluído)
                if (request.FundoId == "padrao")
                    return Json(new { success = false, message = "Não é possível excluir o fundo padrão" });

                // Converter ID para int
                if (!int.TryParse(request.FundoId, out int fundoId))
                    return Json(new { success = false, message = "ID do fundo inválido" });

                // Excluir o fundo
                var sucesso = await _fundoService.ExcluirFundo(fundoId, usuarioId.Value, _env.WebRootPath);
                
                if (sucesso)
                    return Json(new { success = true, message = "Fundo excluído com sucesso" });
                else
                    return Json(new { success = false, message = "Fundo não encontrado ou não pertence ao usuário" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro interno: " + ex.Message });
            }
        }
    }

    public class ExcluirFundoRequest
    {
        public string FundoId { get; set; }
    }
    
}
