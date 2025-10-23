using Microsoft.AspNetCore.Mvc;
using Services;
using Domain.DTOs;
using System.Text.Json;
using Interfaces;
using Interfaces.Service;

namespace Agile.Controllers
{
    public class PdfController : Controller
    {
        private readonly PdfServices _pdf;
        private readonly CsvServices _csv;
        private readonly IWebHostEnvironment _env;
        private readonly IFundoPersonalizadoService _fundoService;

        public PdfController(PdfServices pdf, CsvServices csv, IWebHostEnvironment env, IFundoPersonalizadoService fundoService)
        {
            _pdf = pdf;
            _csv = csv;
            _env = env;
            _fundoService = fundoService;
        }

        [HttpPost]
        public async Task<IActionResult> GerarCartazA5(IFormFile csv, string? fundoSelecionado)
        {
            if (csv == null || csv.Length == 0)
                return BadRequest("Envie um CSV válido");

            byte[] fundoBytes;

            try
            {
                fundoBytes = await CarregarFundo(fundoSelecionado);
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
        public async Task<IActionResult> GerarCartazA4(IFormFile csv, string? fundoSelecionado)
        {
            if (csv == null || csv.Length == 0)
                return BadRequest("Envie um CSV válido");

            byte[] fundoBytes;

            try
            {
                fundoBytes = await CarregarFundo(fundoSelecionado);
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
        public async Task<IActionResult> GerarPreviewPdf([FromBody] PreviewRequest request)
        {
            try
            {
                Console.WriteLine($"=== GerarPreviewPdf chamado ===");
                Console.WriteLine($"Ofertas: {request.Ofertas?.Count ?? 0}");
                Console.WriteLine($"Fundo: {request.FundoSelecionado}, Tamanho: {request.TamanhoSelecionado}");
                Console.WriteLine($"Configurações: {request.Configuracoes != null}");
                
                if (request.Configuracoes != null)
                {
                    Console.WriteLine($"Configurações detalhadas:");
                    Console.WriteLine($"  NomeAltura: {request.Configuracoes.NomeAltura}");
                    Console.WriteLine($"  PrecoLateral: {request.Configuracoes.PrecoLateral}");
                }

                if (request.Ofertas == null || request.Ofertas.Count == 0)
                {
                    Console.WriteLine("ERRO: Nenhuma oferta encontrada");
                    return BadRequest("Nenhuma oferta encontrada.");
                }

                // Carregar fundo
                Console.WriteLine("Carregando fundo...");
                byte[] fundoBytes = await CarregarFundo(request.FundoSelecionado, request.TamanhoSelecionado);
                Console.WriteLine($"Fundo carregado - Tamanho: {fundoBytes.Length} bytes");

                // Obter dados do usuário da sessão
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId")?.ToString();
                var empresaId = HttpContext.Session.GetString("EmpresaId") ?? "DEFAULT";
                
                Console.WriteLine($"PdfController GerarPreviewPdf - UsuarioId: {usuarioId}, EmpresaId: {empresaId}");
                
                // Converte para OfertaDTO
                Console.WriteLine("Convertendo ofertas...");
                Console.WriteLine($"Ofertas recebidas: {request.Ofertas.Count}");
                for (int i = 0; i < request.Ofertas.Count; i++)
                {
                    var oferta = request.Ofertas[i];
                    Console.WriteLine($"Oferta {i}: Id={oferta.Id}, NomeBase='{oferta.NomeBase}', Preco={oferta.Preco}");
                }
                
                var ofertas = _csv.ConverterParaOfertaDTO(request.Ofertas, usuarioId, empresaId);
                Console.WriteLine($"Ofertas convertidas: {ofertas.Count}");

                // Carregar configurações salvas da sessão se não foram fornecidas
                Domain.DTOs.LayoutConfiguracoes? configuracoesParaUsar = request.Configuracoes;
                
                if (configuracoesParaUsar == null)
                {
                    var configuracoesJson = HttpContext.Session.GetString("LayoutConfiguracoes");
                    if (!string.IsNullOrEmpty(configuracoesJson))
                    {
                        try
                        {
                            configuracoesParaUsar = JsonSerializer.Deserialize<Domain.DTOs.LayoutConfiguracoes>(configuracoesJson);
                            Console.WriteLine($"Configurações carregadas da sessão: NomeAltura={configuracoesParaUsar.NomeAltura}, PrecoLateral={configuracoesParaUsar.PrecoLateral}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao carregar configurações da sessão: {ex.Message}");
                        }
                    }
                    else
                    {
                        // Tentar carregar configurações salvas permanentemente para este fundo e tamanho
                        configuracoesParaUsar = await CarregarConfiguracoesPermanentes(request.FundoSelecionado, request.TamanhoSelecionado);
                        if (configuracoesParaUsar != null)
                        {
                            Console.WriteLine($"Configurações carregadas do arquivo: NomeAltura={configuracoesParaUsar.NomeAltura}, PrecoLateral={configuracoesParaUsar.PrecoLateral}");
                        }
                    }
                }
                else
                {
                    // Salvar configurações na sessão para uso posterior
                    HttpContext.Session.SetString("LayoutConfiguracoes", JsonSerializer.Serialize(request.Configuracoes));
                    Console.WriteLine($"Configurações recebidas e salvas: NomeAltura={request.Configuracoes.NomeAltura}");
                    
                    // Salvar configurações permanentemente em arquivo para este fundo e tamanho
                    await SalvarConfiguracoesPermanentes(request.Configuracoes, request.FundoSelecionado, request.TamanhoSelecionado);
                }

                // Gera PDF de preview com configurações
                Console.WriteLine($"Gerando PDF {request.TamanhoSelecionado}...");
                if (configuracoesParaUsar != null)
                {
                    Console.WriteLine($"APLICANDO CONFIGURAÇÕES: NomeAltura={configuracoesParaUsar.NomeAltura}, PrecoLateral={configuracoesParaUsar.PrecoLateral}");
                }
                else
                {
                    Console.WriteLine("NENHUMA CONFIGURAÇÃO APLICADA - Usando valores padrão");
                }
                
                byte[] pdfBytes = request.TamanhoSelecionado == "A4" 
                    ? _pdf.GerarCartazesA4(ofertas, fundoBytes, configuracoesParaUsar)
                    : _pdf.GerarCartazesA5(ofertas, fundoBytes, configuracoesParaUsar);

                Console.WriteLine($"PDF gerado com sucesso - Tamanho: {pdfBytes.Length} bytes");
                return File(pdfBytes, "application/pdf", "preview_cartazes.pdf");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"Erro de serialização JSON: {jsonEx.Message}");
                return BadRequest($"Erro de serialização: {jsonEx.Message}");
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine($"Erro de argumento: {argEx.Message}");
                return BadRequest($"Erro de argumento: {argEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no GerarPreviewPdf: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return BadRequest($"Erro ao gerar preview: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult TesteSerializacao([FromBody] string request)
        {
            try
            {
                Console.WriteLine("=== TESTE DE SERIALIZAÇÃO ===");
                Console.WriteLine($"Request recebido: {request}");
                
                return Ok(new { 
                    success = true, 
                    message = "Serialização funcionando corretamente",
                    requestLength = request?.Length ?? 0
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no teste: {ex.Message}");
                return BadRequest($"Erro: {ex.Message}");
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

                return File(pdfBytes, "application/pdf", "cartazes.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao gerar PDF: {ex.Message}");
            }
        }

        private async Task<byte[]> CarregarFundo(string? fundoSelecionado, string? tamanho = null)
        {
            Console.WriteLine($"Carregando fundo: {fundoSelecionado}, tamanho: {tamanho}");
            
            if (fundoSelecionado == "padrao")
            {
                // Determina o arquivo padrão baseado no tamanho
                string fileName = tamanho == "A4" ? "fundoPadraoA4.png" : "fundoPadraoA5.png";
                string imagePath = Path.Combine(_env.WebRootPath, "fundos", fileName);
                Console.WriteLine($"Procurando arquivo padrão: {imagePath}");
                
                if (!System.IO.File.Exists(imagePath))
                {
                    Console.WriteLine($"Arquivo não encontrado: {imagePath}");
                    throw new FileNotFoundException("O arquivo de fundo padrão não foi encontrado.");
                }
                
                var bytes = System.IO.File.ReadAllBytes(imagePath);
                Console.WriteLine($"Fundo padrão carregado: {bytes.Length} bytes");
                return bytes;
            }
            else
            {
                // Fundo personalizado
                Console.WriteLine("Carregando fundo personalizado...");
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                Console.WriteLine($"UsuarioId da sessão: {usuarioId}");
                
                if (!usuarioId.HasValue)
                    throw new UnauthorizedAccessException("Usuário não logado.");

                var fundoPersonalizado = await _fundoService.ObterFundoPorId(int.Parse(fundoSelecionado), usuarioId.Value);
                if (fundoPersonalizado == null)
                {
                    Console.WriteLine($"Fundo personalizado não encontrado: ID={fundoSelecionado}, UsuarioId={usuarioId}");
                    throw new FileNotFoundException("Fundo personalizado não encontrado.");
                }

                string imagePath = Path.Combine(_env.WebRootPath, fundoPersonalizado.CaminhoImagem.TrimStart('/'));
                Console.WriteLine($"Caminho do fundo personalizado: {imagePath}");
                
                if (!System.IO.File.Exists(imagePath))
                {
                    Console.WriteLine($"Arquivo de fundo personalizado não encontrado: {imagePath}");
                    throw new FileNotFoundException("Arquivo de fundo personalizado não encontrado.");
                }
                
                var bytes = System.IO.File.ReadAllBytes(imagePath);
                Console.WriteLine($"Fundo personalizado carregado: {bytes.Length} bytes");
                return bytes;
            }
        }

        private async Task SalvarConfiguracoesPermanentes(Domain.DTOs.LayoutConfiguracoes configuracoes, string fundoSelecionado, string tamanhoSelecionado)
        {
            try
            {
                // Criar nome do arquivo baseado no fundo e tamanho
                var configFileName = $"layout_config_{fundoSelecionado}_{tamanhoSelecionado}.json";
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "config", configFileName);
                var configDir = Path.GetDirectoryName(configPath);
                
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }

                var json = JsonSerializer.Serialize(configuracoes, new JsonSerializerOptions { WriteIndented = true });
                await System.IO.File.WriteAllTextAsync(configPath, json);
                Console.WriteLine($"Configurações salvas permanentemente para fundo {fundoSelecionado} tamanho {tamanhoSelecionado} em: {configPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar configurações permanentemente: {ex.Message}");
            }
        }

        private async Task<Domain.DTOs.LayoutConfiguracoes?> CarregarConfiguracoesPermanentes(string fundoSelecionado, string tamanhoSelecionado)
        {
            try
            {
                // Criar nome do arquivo baseado no fundo e tamanho
                var configFileName = $"layout_config_{fundoSelecionado}_{tamanhoSelecionado}.json";
                var configPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "config", configFileName);
                
                if (System.IO.File.Exists(configPath))
                {
                    var json = await System.IO.File.ReadAllTextAsync(configPath);
                    var configuracoes = JsonSerializer.Deserialize<Domain.DTOs.LayoutConfiguracoes>(json);
                    Console.WriteLine($"Configurações carregadas do arquivo para fundo {fundoSelecionado} tamanho {tamanhoSelecionado}: {configPath}");
                    return configuracoes;
                }
                else
                {
                    Console.WriteLine($"Arquivo de configurações não encontrado para fundo {fundoSelecionado} tamanho {tamanhoSelecionado}: {configPath}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar configurações permanentemente: {ex.Message}");
                return null;
            }
        }

    }

    public class PreviewRequest
    {
        public List<ProcessedOferta> Ofertas { get; set; } = new();
        public string FundoSelecionado { get; set; } = "";
        public string TamanhoSelecionado { get; set; } = "";
        public Domain.DTOs.LayoutConfiguracoes? Configuracoes { get; set; }
    }

}
