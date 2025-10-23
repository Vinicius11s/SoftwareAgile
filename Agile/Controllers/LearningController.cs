using Microsoft.AspNetCore.Mvc;
using Interfaces.Service;

namespace Agile.Controllers
{
    public class LearningController : Controller
    {
        private readonly ILearningService _learningService;

        public LearningController(ILearningService learningService)
        {
            _learningService = learningService;
        }

        public IActionResult Index(bool mostrarDesativadas = false)
        {
            try
            {
                // Obter dados do usuário da sessão
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId")?.ToString();
                var empresaId = HttpContext.Session.GetString("EmpresaId") ?? "DEFAULT";
                
                Console.WriteLine($"Learning Controller: UsuarioId={usuarioId}, EmpresaId={empresaId}, MostrarDesativadas={mostrarDesativadas}");
                
                // Limpa correções inválidas e problemáticas antes de mostrar
                _learningService.LimparCorrecoesInvalidas();
                _learningService.LimparCorrecoesProblematicas();
                
                // Debug específico para variedade
                if (_learningService is Services.DatabaseLearningService dbService)
                {
                    dbService.DebugCorrecoesVariedadeAsync().GetAwaiter().GetResult();
                }
                
                List<Domain.DTOs.CorrecaoAprendida> correcoes;
                
                if (mostrarDesativadas)
                {
                    // Para desativadas, buscar apenas correções desativadas do usuário/empresa
                    if (_learningService is Services.DatabaseLearningService dbService2)
                    {
                        correcoes = dbService2.ObterCorrecoesDesativadasAsync(null, usuarioId, empresaId).GetAwaiter().GetResult();
                    }
                    else
                    {
                        correcoes = _learningService.ObterCorrecoesDesativadas(null, usuarioId, empresaId);
                    }
                }
                else
                {
                    // Para ativas, buscar apenas correções ativas do usuário/empresa
                    if (_learningService is Services.DatabaseLearningService dbService2)
                    {
                        correcoes = dbService2.ObterCorrecoesAprendidasAsync(null, usuarioId, empresaId).GetAwaiter().GetResult();
                    }
                    else
                    {
                        correcoes = _learningService.ObterCorrecoesAprendidas();
                    }
                }
                    
                Console.WriteLine($"Learning Controller: Retornando {correcoes.Count} correções para a view (Desativadas: {mostrarDesativadas})");
                
                foreach (var correcao in correcoes)
                {
                    Console.WriteLine($"Correção: ID={correcao.Id}, Original='{correcao.TextoOriginal}', Corrigido='{correcao.TextoCorrigido}', Tipo='{correcao.TipoCorrecao}', Ativo={correcao.Ativo}, UsuarioId='{correcao.UsuarioId}'");
                }
                
                ViewData["MostrarDesativadas"] = mostrarDesativadas;
                return View("~/Views/Learning/Learning.cshtml", correcoes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no Learning Controller: {ex.Message}");
                return BadRequest($"Erro ao carregar correções: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult AtivarCorrecao(int id)
        {
            try
            {
                _learningService.AtivarCorrecao(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao ativar correção: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult DesativarCorrecao(int id)
        {
            try
            {
                _learningService.DesativarCorrecao(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao desativar correção: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult RemoverCorrecao(int id)
        {
            try
            {
                _learningService.RemoverCorrecao(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao remover correção: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarCorrecao([FromBody] RegistrarCorrecaoRequest request)
        {
            try
            {
                // Obter dados do usuário da sessão
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                var empresaId = HttpContext.Session.GetString("EmpresaId") ?? "DEFAULT";
                var sessaoId = HttpContext.Session.Id;

                Console.WriteLine($"=== REGISTRAR CORREÇÃO ===");
                Console.WriteLine($"UsuarioId: {usuarioId}");
                Console.WriteLine($"EmpresaId: {empresaId}");
                Console.WriteLine($"SessaoId: {sessaoId}");
                Console.WriteLine($"TextoOriginal: '{request.TextoOriginal}'");
                Console.WriteLine($"TextoCorrigido: '{request.TextoCorrigido}'");
                Console.WriteLine($"TipoCorrecao: '{request.TipoCorrecao}'");

                if (usuarioId.HasValue)
                {
                    // Usuário logado - usar ID do usuário
                    await _learningService.RegistrarCorrecaoAsync(
                        request.TextoOriginal, 
                        request.TextoCorrigido, 
                        request.TipoCorrecao, 
                        sessaoId, 
                        usuarioId.Value.ToString(), 
                        empresaId
                    );
                }
                else
                {
                    // Usuário não logado - usar ANONIMO
                    await _learningService.RegistrarCorrecaoAsync(
                        request.TextoOriginal, 
                        request.TextoCorrigido, 
                        request.TipoCorrecao, 
                        sessaoId, 
                        "ANONIMO", 
                        empresaId
                    );
                }

                return Ok(new { success = true, message = "Correção registrada com sucesso!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao registrar correção: {ex.Message}");
                return BadRequest($"Erro ao registrar correção: {ex.Message}");
            }
        }
    }

    public class RegistrarCorrecaoRequest
    {
        public string TextoOriginal { get; set; } = string.Empty;
        public string TextoCorrigido { get; set; } = string.Empty;
        public string TipoCorrecao { get; set; } = string.Empty;
    }
}
