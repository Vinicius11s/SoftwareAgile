using Microsoft.AspNetCore.Mvc;
using Services;
using Domain.Entities;
using System.IO;
using System.Collections.Generic;
using QuestPDF;
using Domain.DTOs;
using Interfaces.Service;
using System.Text.Json;

namespace Agile.Controllers
{
    public class CartazesController : Controller
    {
        private readonly CsvServices _csv;
        private readonly PdfServices _pdf;
        private readonly IWebHostEnvironment _env;
        private readonly IFundoPersonalizadoService _fundoService;
        private readonly ILearningService _learningService;
        public CartazesController(CsvServices csv, PdfServices pdf, IWebHostEnvironment env, IFundoPersonalizadoService fundoService, ILearningService learningService)
        {
            _csv = csv;
            _pdf = pdf;
            _env = env;
            _fundoService = fundoService;
            _learningService = learningService;
        }

        public IActionResult Index()
        {
            return View();
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
                    return RedirectToAction("Index");
                }

                var previewData = JsonSerializer.Deserialize<PreviewData>(previewDataJson);
                if (previewData == null)
                {
                    return RedirectToAction("Index");
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
            }
            else
            {
                if(tamanho == "A5")
                {
                    fundos.Add(("Cartaz Padrão", "~/fundos/fundoPadraoA5.png", "padrao"));
                    var fundosPersonalizados = await _fundoService.ObterFundosParaViewPorTipo(usuarioId.Value, tamanho);
                }
                else
                    return BadRequest("Selecione um Tamanho");               
            }  
            return View(fundos);
        }
        [HttpGet]
        public IActionResult UploadCsv(string? fundo, string? tamanho)
        {
            ViewData["FundoSelecionado"] = fundo ?? "padrao"; // padrão se nada for passado
            ViewData["TamanhoSelecionado"] = tamanho ?? "A5"; // padrão se nada for passado
            return View();
        }
        [HttpPost]
        public IActionResult GerarCartazA5(IFormFile csv, string? fundoSelecionado)
        {
            if (csv == null || csv.Length == 0)
                return BadRequest("Envie um CSV válido");

            byte[] fundoBytes;

            try
            {
                // Define caminho da imagem de fundo baseado no fundo selecionado
                string nomeArquivo = fundoSelecionado switch
                {
                    "padrao" => "fundoPadraoA5.png",
                    _ => "fundoPadraoA5.png" // padrão
                };

                string imagePath = Path.Combine(_env.WebRootPath, "fundos", nomeArquivo);

                if (!System.IO.File.Exists(imagePath))
                    return BadRequest($"O arquivo de fundo '{nomeArquivo}' não foi encontrado.");

                fundoBytes = System.IO.File.ReadAllBytes(imagePath);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao carregar o fundo: {ex.Message}");
            }

            try
            {
                List<OfertaDTO> ofertas;
                using (var csvStream = csv.OpenReadStream())
                {
                    ofertas = _csv.LerOfertas(csvStream);
                }

                if (ofertas == null || ofertas.Count == 0)
                    return BadRequest("O CSV não contém dados ou o formato está incorreto.");

                var pdfBytes = _pdf.GerarCartazesA5(ofertas, fundoBytes);

                return File(pdfBytes, "application/pdf", "cartazes.pdf");
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
        [HttpPost]
        public IActionResult GerarCartazA4(IFormFile csv, string? fundoSelecionado)
        {
            if (csv == null || csv.Length == 0)
                return BadRequest("Envie um CSV válido");

            byte[] fundoBytes;

            try
            {
                // Define caminho da imagem de fundo baseado no fundo selecionado
                string nomeArquivo = fundoSelecionado switch
                {
                    "padrao" => "fundoPadraoA4.png",
                    _ => "fundoPadraoA4.png" // padrão
                };

                string imagePath = Path.Combine(_env.WebRootPath, "fundos", nomeArquivo);

                if (!System.IO.File.Exists(imagePath))
                    return BadRequest($"O arquivo de fundo '{nomeArquivo}' não foi encontrado.");

                fundoBytes = System.IO.File.ReadAllBytes(imagePath);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao carregar o fundo: {ex.Message}");
            }

            try
            {
                List<OfertaDTO> ofertas;
                using (var csvStream = csv.OpenReadStream())
                {
                    ofertas = _csv.LerOfertas(csvStream);
                }

                if (ofertas == null || ofertas.Count == 0)
                    return BadRequest("O CSV não contém dados ou o formato está incorreto.");

                var pdfBytes = _pdf.GerarCartazesA4(ofertas, fundoBytes);

                return File(pdfBytes, "application/pdf", "cartazes.pdf");
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

        [HttpPost]
        public IActionResult PreviewCartazes(IFormFile csv, string? fundoSelecionado, string? tamanhoSelecionado)
        {
            if (csv == null || csv.Length == 0)
                return BadRequest("Envie um CSV válido");

            byte[] fundoBytes;

            try
            {
                // Define caminho da imagem de fundo baseado no fundo selecionado
                string nomeArquivo = fundoSelecionado switch
                {
                    "padrao" => tamanhoSelecionado == "A4" ? "fundoPadraoA4.png" : "fundoPadraoA5.png",
                    _ => tamanhoSelecionado == "A4" ? "fundoPadraoA4.png" : "fundoPadraoA5.png"
                };

                string imagePath = Path.Combine(_env.WebRootPath, "fundos", nomeArquivo);

                if (!System.IO.File.Exists(imagePath))
                    return BadRequest($"O arquivo de fundo '{nomeArquivo}' não foi encontrado.");

                fundoBytes = System.IO.File.ReadAllBytes(imagePath);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao carregar o fundo: {ex.Message}");
            }

            try
            {
                List<ProcessedOferta> ofertas;
                using (var csvStream = csv.OpenReadStream())
                {
                    ofertas = _csv.ProcessarOfertas(csvStream);
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

        [HttpPost]
        public IActionResult AtualizarPreview([FromForm] List<OfertaEditModel> ofertas)
        {
            try
            {
                // Recupera dados da session
                var previewDataJson = HttpContext.Session.GetString("PreviewData");
                if (string.IsNullOrEmpty(previewDataJson))
                    return BadRequest("Dados de preview não encontrados.");

                var previewData = JsonSerializer.Deserialize<PreviewData>(previewDataJson);
                if (previewData == null)
                    return BadRequest("Erro ao recuperar dados de preview.");

                if (ofertas == null || ofertas.Count == 0)
                    return BadRequest("Nenhuma oferta foi enviada.");

                // Converte OfertaEditModel para ProcessedOferta e registra correções
                var ofertasProcessadas = new List<ProcessedOferta>();
                var sessaoId = HttpContext.Session.Id;

                foreach (var o in ofertas)
                {
                    try
                    {
                        // Recupera o item original para comparar
                        var itemOriginal = previewData.Ofertas.FirstOrDefault(orig => orig.Id == o.Id);
                        
                        // Registra correções se houver mudanças
                        if (itemOriginal != null && _learningService != null)
                        {
                            try
                            {
                                if (itemOriginal.NomeBase != o.NomeBase)
                                {
                                    Console.WriteLine($"Registrando correção NOME: '{itemOriginal.NomeBase}' -> '{o.NomeBase}'");
                                    _learningService.RegistrarCorrecao(itemOriginal.NomeBase, o.NomeBase, "NOME", sessaoId);
                                }
                                
                                if (itemOriginal.Gramagem != o.Gramagem)
                                {
                                    Console.WriteLine($"Registrando correção GRAMAGEM: '{itemOriginal.Gramagem}' -> '{o.Gramagem}'");
                                    _learningService.RegistrarCorrecao(itemOriginal.Gramagem, o.Gramagem, "GRAMAGEM", sessaoId);
                                }
                                
                                if (itemOriginal.Variedade != o.Variedade)
                                {
                                    Console.WriteLine($"Registrando correção VARIEDADE: '{itemOriginal.Variedade}' -> '{o.Variedade}'");
                                    _learningService.RegistrarCorrecao(itemOriginal.Variedade, o.Variedade, "VARIEDADE", sessaoId);
                                }
                            }
                            catch (Exception learningEx)
                            {
                                // Log do erro de aprendizado mas continua o processamento
                                Console.WriteLine($"Erro no sistema de aprendizado: {learningEx.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"LearningService null ou itemOriginal null. LearningService: {_learningService != null}, ItemOriginal: {itemOriginal != null}");
                        }

                        ofertasProcessadas.Add(new ProcessedOferta
                        {
                            Id = o.Id,
                            NomeBase = o.NomeBase ?? string.Empty,
                            Gramagem = o.Gramagem ?? string.Empty,
                            Variedade = o.Variedade ?? string.Empty,
                            Preco = o.Preco,
                            IsFamilia = o.IsFamilia,
                            QuantidadeProdutos = o.QuantidadeProdutos,
                            DescricaoFormatada = $"{o.NomeBase}\n{o.Gramagem} {o.Variedade}".Trim()
                        });
                    }
                    catch (Exception itemEx)
                    {
                        // Log do erro específico do item
                        Console.WriteLine($"Erro ao processar item {o.Id}: {itemEx.Message}");
                        continue; // Pula este item e continua com os outros
                    }
                }

                if (ofertasProcessadas.Count == 0)
                    return BadRequest("Nenhuma oferta foi processada com sucesso.");

                // Atualiza as ofertas
                previewData.Ofertas = ofertasProcessadas;

                // Recalcula estatísticas
                previewData.TotalProdutos = ofertasProcessadas.Sum(o => o.QuantidadeProdutos);
                previewData.TotalFamilias = ofertasProcessadas.Count(o => o.IsFamilia);
                previewData.TotalCartazes = (int)Math.Ceiling(ofertasProcessadas.Count / 2.0);

                // Salva na session
                HttpContext.Session.SetString("PreviewData", JsonSerializer.Serialize(previewData));

                return PartialView("_PreviewContainer", previewData);
            }
            catch (Exception ex)
            {
                // Log detalhado do erro
                Console.WriteLine($"Erro completo na atualização: {ex}");
                return BadRequest($"Erro ao atualizar preview: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult GerarPdfFinal()
        {
            try
            {
                // Recupera dados do preview
                var previewDataJson = HttpContext.Session.GetString("PreviewData");
                if (string.IsNullOrEmpty(previewDataJson))
                    return BadRequest("Dados de preview não encontrados.");

                var previewData = JsonSerializer.Deserialize<PreviewData>(previewDataJson);
                if (previewData == null)
                    return BadRequest("Erro ao recuperar dados de preview.");

                // Converte para OfertaDTO
                var ofertas = _csv.ConverterParaOfertaDTO(previewData.Ofertas);

                // Gera PDF final
                byte[] pdfBytes = previewData.TamanhoSelecionado == "A4" 
                    ? _pdf.GerarCartazesA4(ofertas, previewData.FundoBytes)
                    : _pdf.GerarCartazesA5(ofertas, previewData.FundoBytes);

                // Limpa a session
                HttpContext.Session.Remove("PreviewData");

                return File(pdfBytes, "application/pdf", "cartazes.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao gerar PDF: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult ExcluirItem(int itemId)
        {
            try
            {
                // Recupera dados da session
                var previewDataJson = HttpContext.Session.GetString("PreviewData");
                if (string.IsNullOrEmpty(previewDataJson))
                    return BadRequest("Dados de preview não encontrados.");

                var previewData = JsonSerializer.Deserialize<PreviewData>(previewDataJson);
                if (previewData == null)
                    return BadRequest("Erro ao recuperar dados de preview.");

                // Remove o item da lista
                previewData.Ofertas = previewData.Ofertas.Where(o => o.Id != itemId).ToList();

                // Recalcula estatísticas
                previewData.TotalProdutos = previewData.Ofertas.Sum(o => o.QuantidadeProdutos);
                previewData.TotalFamilias = previewData.Ofertas.Count(o => o.IsFamilia);
                previewData.TotalCartazes = (int)Math.Ceiling(previewData.Ofertas.Count / 2.0);

                // Salva na session
                HttpContext.Session.SetString("PreviewData", JsonSerializer.Serialize(previewData));

                return PartialView("_PreviewContainer", previewData);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao excluir item: {ex.Message}");
            }
        }

        public IActionResult Learning()
        {
            try
            {
                // Limpa correções inválidas antes de mostrar
                _learningService.LimparCorrecoesInvalidas();
                
                var correcoes = _learningService.ObterCorrecoesAprendidas();
                Console.WriteLine($"Learning Controller: Retornando {correcoes.Count} correções para a view");
                
                foreach (var correcao in correcoes)
                {
                    Console.WriteLine($"Correção: ID={correcao.Id}, Original='{correcao.TextoOriginal}', Corrigido='{correcao.TextoCorrigido}', Tipo='{correcao.TipoCorrecao}'");
                }
                
                return View(correcoes);
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
    }
}