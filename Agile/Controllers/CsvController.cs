using Microsoft.AspNetCore.Mvc;
using Services;
using Domain.DTOs;
using System.Text.Json;

namespace Agile.Controllers
{
    public class CsvController : Controller
    {
        private readonly CsvServices _csv;

        public CsvController(CsvServices csv)
        {
            _csv = csv;
        }

        [HttpGet]
        public IActionResult UploadCsv(string? fundo, string? tamanho)
        {
            ViewData["FundoSelecionado"] = fundo ?? "padrao";
            ViewData["TamanhoSelecionado"] = tamanho ?? "A5";
            return View("~/Views/Csv/UploadCsv.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> ProcessarCsv(IFormFile csv, string? fundoSelecionado, string? tamanhoSelecionado)
        {
            if (csv == null || csv.Length == 0)
                return BadRequest("Envie um CSV válido");

            try
            {
                // Obter dados do usuário da sessão
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId")?.ToString();
                var empresaId = HttpContext.Session.GetString("EmpresaId") ?? "DEFAULT";
                
                Console.WriteLine($"CsvController ProcessarCsv - UsuarioId: {usuarioId}, EmpresaId: {empresaId}");
                
                List<ProcessedOferta> ofertas;
                using (var csvStream = csv.OpenReadStream())
                {
                    ofertas = _csv.ProcessarOfertas(csvStream, usuarioId, empresaId);
                }

                if (ofertas == null || ofertas.Count == 0)
                    return BadRequest("O CSV não contém dados ou o formato está incorreto.");

                // Retorna os dados processados para o controller que chamou
                return Json(new { 
                    success = true, 
                    ofertas = ofertas,
                    fundoSelecionado = fundoSelecionado ?? "padrao",
                    tamanhoSelecionado = tamanhoSelecionado ?? "A5"
                });
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
                {
                    Console.WriteLine("ERRO: Dados de preview não encontrados na sessão");
                    return BadRequest("Dados de preview não encontrados.");
                }

                var previewData = JsonSerializer.Deserialize<PreviewData>(previewDataJson);
                if (previewData == null)
                {
                    Console.WriteLine("ERRO: Falha ao deserializar dados de preview");
                    return BadRequest("Erro ao recuperar dados de preview.");
                }

                if (ofertas == null || ofertas.Count == 0)
                {
                    Console.WriteLine("ERRO: Nenhuma oferta foi enviada");
                    return BadRequest("Nenhuma oferta foi enviada.");
                }

                Console.WriteLine($"=== ATUALIZAR PREVIEW ===");
                Console.WriteLine($"Ofertas recebidas: {ofertas.Count}");
                Console.WriteLine($"Ofertas na sessão: {previewData.Ofertas.Count}");
                
                // Log dos IDs das ofertas recebidas
                foreach (var o in ofertas)
                {
                    Console.WriteLine($"Oferta recebida - ID: {o.Id}, Nome: {o.NomeBase}");
                }
                
                // Log dos IDs das ofertas na sessão
                foreach (var orig in previewData.Ofertas)
                {
                    Console.WriteLine($"Oferta na sessão - ID: {orig.Id}, Nome: {orig.NomeBase}");
                }

                // Obter dados do usuário da sessão para aplicar correções
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId")?.ToString();
                var empresaId = HttpContext.Session.GetString("EmpresaId") ?? "DEFAULT";
                
                Console.WriteLine($"CsvController AtualizarPreview - UsuarioId: {usuarioId}, EmpresaId: {empresaId}");

                // Debug das correções de nomes disponíveis
                if (_csv._learningService is Services.DatabaseLearningService dbService)
                {
                    dbService.DebugCorrecoesNomesAsync(usuarioId, empresaId).GetAwaiter().GetResult();
                    
                    // Teste específico para "ARROZ EMPORIO SAO JOAO"
                    dbService.TestarCorrecaoAsync("ARROZ EMPORIO SAO JOAO", "NOME", usuarioId, empresaId).GetAwaiter().GetResult();
                }

                // Converte OfertaEditModel para ProcessedOferta
                var ofertasProcessadas = new List<ProcessedOferta>();

                foreach (var o in ofertas)
                {
                    try
                    {
                        var ofertaProcessada = new ProcessedOferta
                        {
                            Id = o.Id,
                            NomeBase = o.NomeBase ?? string.Empty,
                            Gramagem = o.Gramagem ?? string.Empty,
                            Variedade = o.Variedade ?? string.Empty,
                            Preco = o.Preco,
                            IsFamilia = o.IsFamilia,
                            QuantidadeProdutos = o.QuantidadeProdutos
                        };
                        
                        // Aplicar correções aprendidas antes de gerar a descrição formatada
                        if (_csv._learningService != null)
                        {
                            var nomeAntes = ofertaProcessada.NomeBase;
                            var gramagemAntes = ofertaProcessada.Gramagem;
                            var variedadeAntes = ofertaProcessada.Variedade;
                            
                            ofertaProcessada.NomeBase = _csv._learningService.AplicarCorrecoesAprendidas(ofertaProcessada.NomeBase, "NOME", usuarioId, empresaId);
                            ofertaProcessada.Gramagem = _csv._learningService.AplicarCorrecoesAprendidas(ofertaProcessada.Gramagem, "GRAMAGEM", usuarioId, empresaId);
                            ofertaProcessada.Variedade = _csv._learningService.AplicarCorrecoesAprendidas(ofertaProcessada.Variedade, "VARIEDADE", usuarioId, empresaId);
                            
                            // Debug das correções aplicadas
                            if (nomeAntes != ofertaProcessada.NomeBase)
                                Console.WriteLine($"CORREÇÃO NOME APLICADA NO PREVIEW: '{nomeAntes}' -> '{ofertaProcessada.NomeBase}'");
                            if (gramagemAntes != ofertaProcessada.Gramagem)
                                Console.WriteLine($"CORREÇÃO GRAMAGEM APLICADA NO PREVIEW: '{gramagemAntes}' -> '{ofertaProcessada.Gramagem}'");
                            if (variedadeAntes != ofertaProcessada.Variedade)
                                Console.WriteLine($"CORREÇÃO VARIEDADE APLICADA NO PREVIEW: '{variedadeAntes}' -> '{ofertaProcessada.Variedade}'");
                        }
                        
                        // Gera a descrição formatada usando o método otimizado
                        ofertaProcessada.DescricaoFormatada = ofertaProcessada.GerarDescricaoParaCartaz();
                        
                        ofertasProcessadas.Add(ofertaProcessada);
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
    }
}
