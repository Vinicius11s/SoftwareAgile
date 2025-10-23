using Microsoft.AspNetCore.Mvc;
using Services;
using Domain.DTOs;
using Interfaces.Service;
using System.Text.Json;

namespace Agile.Controllers
{
    public class CartazesController : Controller
    {
        private readonly CsvServices _csv;
        private readonly IWebHostEnvironment _env;
        private readonly IFundoPersonalizadoService _fundoService;
        private readonly ILearningService _learningService;

        public CartazesController(CsvServices csv, IWebHostEnvironment env, IFundoPersonalizadoService fundoService, ILearningService learningService)
        {
            _csv = csv;
            _env = env;
            _fundoService = fundoService;
            _learningService = learningService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("EscolherTamanho");
        }

        public IActionResult EscolherTamanho()
        {
            return View("~/Views/Cartazes/EscolherTamanho.cshtml");
        }

        public IActionResult PersonalizarLayout()
        {
            // Recupera dados do preview da sessão
            var previewDataJson = HttpContext.Session.GetString("PreviewData");
            if (string.IsNullOrEmpty(previewDataJson))
            {
                return RedirectToAction("EscolherTamanho");
            }

            var previewData = JsonSerializer.Deserialize<PreviewData>(previewDataJson);
            if (previewData == null)
            {
                return RedirectToAction("EscolherTamanho");
            }

            return View(previewData);
        }

        public IActionResult Planos()
        {
            return View();
        }

        public IActionResult Preview()
        {
            try
            {
                var previewDataJson = HttpContext.Session.GetString("PreviewData");
                if (string.IsNullOrEmpty(previewDataJson))
                {
                    return RedirectToAction("EscolherTamanho");
                }

                var previewData = JsonSerializer.Deserialize<PreviewData>(previewDataJson);
                if (previewData == null)
                {
                    return RedirectToAction("EscolherTamanho");
                }

                // Carregar configurações salvas da sessão
                var configuracoesJson = HttpContext.Session.GetString("LayoutConfiguracoes");
                if (!string.IsNullOrEmpty(configuracoesJson))
                {
                    try
                    {
                        var configuracoes = JsonSerializer.Deserialize<Domain.DTOs.LayoutConfiguracoes>(configuracoesJson);
                        Console.WriteLine($"Preview: Configurações carregadas da sessão: NomeAltura={configuracoes.NomeAltura}, PrecoLateral={configuracoes.PrecoLateral}");
                        // As configurações serão aplicadas automaticamente no JavaScript quando gerar o PDF
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro ao carregar configurações da sessão: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Preview: Nenhuma configuração salva encontrada na sessão");
                }

                return View(previewData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar preview: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult UploadCsv(string? fundo, string? tamanho)
        {
            ViewData["FundoSelecionado"] = fundo ?? "padrao";
            ViewData["TamanhoSelecionado"] = tamanho ?? "A5";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PreviewCartazes(IFormFile csv, string? fundoSelecionado, string? tamanhoSelecionado)
        {
            if (csv == null || csv.Length == 0)
                return BadRequest("Envie um CSV válido");

            byte[] fundoBytes;

            try
            {
                // Define caminho da imagem de fundo baseado no fundo selecionado
                if (fundoSelecionado == "padrao")
                {
                    string nomeArquivo = tamanhoSelecionado == "A4" ? "fundoPadraoA4.png" : "fundoPadraoA5.png";
                    string imagePath = Path.Combine(_env.WebRootPath, "fundos", nomeArquivo);
                    if (!System.IO.File.Exists(imagePath))
                        return BadRequest($"O arquivo de fundo '{nomeArquivo}' não foi encontrado.");
                    fundoBytes = System.IO.File.ReadAllBytes(imagePath);
                }
                else
                {
                    // Fundo personalizado
                    var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                    if (!usuarioId.HasValue)
                        return BadRequest("Usuário não logado.");

                    var fundoPersonalizado = await _fundoService.ObterFundoPorId(int.Parse(fundoSelecionado), usuarioId.Value);
                    if (fundoPersonalizado == null)
                        return BadRequest("Fundo personalizado não encontrado.");

                    string imagePath = Path.Combine(_env.WebRootPath, fundoPersonalizado.CaminhoImagem.TrimStart('/'));
                    if (!System.IO.File.Exists(imagePath))
                        return BadRequest("Arquivo de fundo personalizado não encontrado.");
                    
                    fundoBytes = System.IO.File.ReadAllBytes(imagePath);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao carregar o fundo: {ex.Message}");
            }

            try
            {
                // Obter dados do usuário da sessão
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId")?.ToString();
                var empresaId = HttpContext.Session.GetString("EmpresaId") ?? "DEFAULT";
                
                Console.WriteLine($"Processando CSV - UsuarioId: {usuarioId}, EmpresaId: {empresaId}");
                
                List<ProcessedOferta> ofertas;
                using (var csvStream = csv.OpenReadStream())
                {
                    ofertas = _csv.ProcessarOfertas(csvStream, usuarioId, empresaId);
                }

                if (ofertas == null || ofertas.Count == 0)
                    return BadRequest("O CSV não contém dados ou o formato está incorreto.");

                // Criar preview data
                var previewData = _csv.CriarPreviewData(ofertas, fundoSelecionado ?? "padrao", tamanhoSelecionado ?? "A5", fundoBytes);

                // Armazenar temporariamente na session
                HttpContext.Session.SetString("PreviewData", JsonSerializer.Serialize(previewData));

                return View("Preview", previewData);
            }
            catch (CsvHelper.BadDataException)
            {
                return BadRequest("O formato do CSV está incorreto. Use 'descricao;preco'.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro inesperado: {ex.Message}");
            }
        }
    }
}