using Microsoft.AspNetCore.Mvc;
using Services;
using Domain.Entities;
using System.IO;
using System.Collections.Generic;
using QuestPDF;
using Domain.DTOs;
using Interfaces.Service;

namespace Agile.Controllers
{
    public class CartazesController : Controller
    {
        private readonly CsvServices _csv;
        private readonly PdfServices _pdf;
        private readonly IWebHostEnvironment _env;
        private readonly IFundoPersonalizadoService _fundoService;
        public CartazesController(CsvServices csv, PdfServices pdf, IWebHostEnvironment env, IFundoPersonalizadoService fundoService)
        {
            _csv = csv;
            _pdf = pdf;
            _env = env;
            _fundoService = fundoService;
        }

        public IActionResult Index()
        {
            return View();
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
    }
}