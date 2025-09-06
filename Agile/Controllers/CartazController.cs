using Microsoft.AspNetCore.Mvc;
using Services;
using Domain.Entities;
using System.IO;
using System.Collections.Generic;
using QuestPDF;
using Domain.DTOs;

namespace Agile.Controllers
{
    public class CartazController : Controller
    {
        private readonly CsvServices _csv;
        private readonly PdfServices _pdf;
        private readonly IWebHostEnvironment _env;

        public CartazController(CsvServices csv, PdfServices pdf, IWebHostEnvironment env)
        {
            _csv = csv;
            _pdf = pdf;
            _env = env;
        }


        [HttpGet]
        public IActionResult EscolherFundo()
        {
            var fundos = new List<(string Nome, string Imagem, string Id)>
            {
                ("Cartaz Mercearia", "~/images/cartazMercearia.jpeg", "mercearia"),
                ("Cartaz Feira", "~/images/cartazFeira.jpeg", "feira"),
            };

            return View(fundos);
        }
        [HttpGet]
        public IActionResult UploadCsv(string? fundo)
        {
            ViewData["FundoSelecionado"] = fundo ?? "mercearia"; // padrão se nada for passado
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
                    "mercearia" => "cartazMercearia.jpeg",
                    "feira" => "cartazFeira.jpeg",
                    _ => "cartazMercearia.jpeg" // padrão
                };

                string imagePath = Path.Combine(_env.WebRootPath, "images", nomeArquivo);

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

                var pdfBytes = PdfServices.GerarCartazes(ofertas, fundoBytes);

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