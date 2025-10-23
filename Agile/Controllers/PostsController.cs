using Microsoft.AspNetCore.Mvc;
using Interfaces.Service;
using Microsoft.AspNetCore.Hosting;
using Services;
using Domain.DTOs;
using System.Text.Json;

namespace Agile.Controllers
{
    public class PostsController : Controller
    {
        private readonly IImagemProdutoService _imagemService;
        private readonly IWebHostEnvironment _env;
        private readonly CsvServices _csv;
        private readonly ImageSearchService _imageSearchService;
        private readonly PostWebGeneratorService _postGenerator;

        public PostsController(IImagemProdutoService imagemService, IWebHostEnvironment env, CsvServices csv, ImageSearchService imageSearchService, PostWebGeneratorService postGenerator)
        {
            _imagemService = imagemService;
            _env = env;
            _csv = csv;
            _imageSearchService = imageSearchService;
            _postGenerator = postGenerator;
        }

        [HttpGet]
        public IActionResult EscolherFundo(string tipo)
        {
            // Verificar se usuário está logado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!usuarioId.HasValue)
                return RedirectToAction("Index", "Login");

            // Verificar se é tipo web
            if (tipo != "web")
                return BadRequest("Tipo inválido");

            // Passar o tipo para a view
            ViewData["TipoSelecionado"] = tipo;

            // Adicionar cache-busting para evitar cache do navegador
            ViewData["CacheBuster"] = DateTime.Now.Ticks;
            return View("~/Views/Posts/EscolherFundo.cshtml");
        }

        [HttpGet]
        public IActionResult UploadCsv(string? fundo, string? tipo)
        {
            // Verificar se usuário está logado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!usuarioId.HasValue)
                return RedirectToAction("Index", "Login");

            // Verificar se é tipo web
            if (tipo != "web")
                return BadRequest("Tipo inválido");

            // Passar os parâmetros para a view
            ViewData["FundoSelecionado"] = fundo ?? "padrao";
            ViewData["TipoSelecionado"] = tipo;
            ViewData["CacheBuster"] = DateTime.Now.Ticks;

            return View("~/Views/Posts/UploadCsv.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> UploadCsv(IFormFile csv, string? fundo, string? tipo)
        {
            if (csv == null || csv.Length == 0)
                return BadRequest("Envie um CSV válido");

            if (tipo != "web")
                return BadRequest("Tipo inválido");

            // Verificar se usuário está logado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!usuarioId.HasValue)
                return RedirectToAction("Index", "Login");

            try
            {
                // Processar CSV com códigos de barras e preços
                List<PostWebDTO> posts;
                using (var csvStream = csv.OpenReadStream())
                {
                    posts = await ProcessarPostsWeb(csvStream, usuarioId.Value);
                }

                if (posts == null || posts.Count == 0)
                    return BadRequest("O CSV não contém dados ou o formato está incorreto.");

                        // Salvar imagens temporariamente para preview
                        await SalvarImagensTemporarias(posts, usuarioId.Value);

                        // Criar preview data
                        var previewData = new PostWebPreviewData
                        {
                            Posts = posts,
                            FundoSelecionado = fundo ?? "padrao",
                            TipoSelecionado = tipo,
                            TotalProdutos = posts.Count,
                            TotalComImagens = posts.Count(p => p.TemImagem),
                            TotalSemImagens = posts.Count(p => !p.TemImagem)
                        };

                        Console.WriteLine($"📋 Preview criado: {previewData.TotalProdutos} produtos, {previewData.TotalComImagens} com imagens, {previewData.TotalSemImagens} sem imagens");

                // Armazenar temporariamente na session
                HttpContext.Session.SetString("PostWebPreviewData", JsonSerializer.Serialize(previewData));

                return View("~/Views/Posts/Preview.cshtml", previewData);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro inesperado: {ex.Message}");
            }
        }

        private async Task<List<PostWebDTO>> ProcessarPostsWeb(Stream csvStream, int usuarioId)
        {
            var config = new CsvHelper.Configuration.CsvConfiguration(new System.Globalization.CultureInfo("pt-BR"))
            {
                Delimiter = ";",
                HasHeaderRecord = false,
                BadDataFound = null,
                MissingFieldFound = null,
                IgnoreBlankLines = true,
                TrimOptions = CsvHelper.Configuration.TrimOptions.Trim
            };

            using var reader = new StreamReader(csvStream);
            using var csv = new CsvHelper.CsvReader(reader, config);

            var posts = new List<PostWebDTO>();
            while (csv.Read())
            {
                try
                {
                    // Verificar se a linha tem dados válidos
                    var codigoBarras = csv.GetField(0)?.Trim();
                    var precoStr = csv.GetField(1)?.Trim();

                    // Pular linhas vazias ou inválidas
                    if (string.IsNullOrEmpty(codigoBarras) || string.IsNullOrEmpty(precoStr))
                    {
                        Console.WriteLine($"Linha vazia ou inválida ignorada: {csv.Parser.RawRecord}");
                        continue;
                    }

                    // Tentar converter o preço
                    if (!decimal.TryParse(precoStr, System.Globalization.NumberStyles.Any, 
                        new System.Globalization.CultureInfo("pt-BR"), out decimal preco))
                    {
                        Console.WriteLine($"Preço inválido ignorado: {precoStr}");
                        continue;
                    }

                    // Validar código de barras (deve ter pelo menos 8 dígitos)
                    if (codigoBarras.Length < 8 || !codigoBarras.All(char.IsDigit))
                    {
                        Console.WriteLine($"Código de barras inválido ignorado: {codigoBarras}");
                        continue;
                    }

                    Console.WriteLine($"Processando: {codigoBarras} - R$ {preco:F2}");

                    // Primeiro, verificar se já existe imagem no banco local
                    var imagemBytes = await _imagemService.ObterImagemProduto(codigoBarras, usuarioId);
                    var imagemInfo = await _imagemService.ObterImagemProdutoInfo(codigoBarras, usuarioId);
                    
                    if (imagemBytes != null)
                    {
                        Console.WriteLine($"✅ Imagem local encontrada para {codigoBarras} - Tamanho: {imagemBytes.Length} bytes");
                        
                        // Verificar se é um placeholder (imagem pequena com fundo cinza)
                        if (imagemBytes.Length < 10000) // Placeholders são pequenos
                        {
                            Console.WriteLine($"🗑️ Imagem placeholder detectada para {codigoBarras}, removendo...");
                            await _imagemService.ExcluirImagem(imagemInfo.Id, usuarioId, _env.WebRootPath);
                            imagemBytes = null;
                            imagemInfo = null;
                            Console.WriteLine($"✅ Placeholder removido, buscando imagem real...");
                        }
                    }

                    // Se não encontrou imagem local, buscar na web
                    if (imagemBytes == null)
                    {
                        Console.WriteLine($"🔍 Nenhuma imagem local encontrada para {codigoBarras}");
                        Console.WriteLine($"🌐 FORÇANDO busca na web para código: {codigoBarras}");
                        Console.WriteLine($"🔍 Chamando ImageSearchService.BuscarImagemPorCodigoBarras...");
                        var imagemWeb = await _imageSearchService.BuscarImagemPorCodigoBarras(codigoBarras);
                        Console.WriteLine($"🔍 Resultado da busca: {(imagemWeb != null ? $"Encontrada - {imagemWeb.Length} bytes" : "NÃO ENCONTRADA")}");
                        
                        if (imagemWeb != null)
                        {
                            Console.WriteLine($"✅ Imagem encontrada na web para {codigoBarras} - Tamanho: {imagemWeb.Length} bytes");
                            // Salvar imagem encontrada no banco local
                            try
                            {
                                var imagemSalva = await _imagemService.SalvarImagemProduto(
                                    codigoBarras, 
                                    imagemWeb, 
                                    "Web Search", 
                                    "Google/OpenFoodFacts", 
                                    usuarioId, 
                                    _env.WebRootPath
                                );
                                
                                imagemBytes = imagemWeb;
                                imagemInfo = imagemSalva;
                                Console.WriteLine($"💾 Imagem salva no banco para código: {codigoBarras}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"❌ Erro ao salvar imagem: {ex.Message}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"❌ Nenhuma imagem encontrada na web para {codigoBarras}");
                            // Criar imagem placeholder quando não encontrar imagem real
                            Console.WriteLine($"🖼️ Criando imagem placeholder para código: {codigoBarras}");
                            imagemBytes = CriarImagemPlaceholder(codigoBarras, preco);
                            
                            // Salvar placeholder no banco local
                            try
                            {
                                var imagemSalva = await _imagemService.SalvarImagemProduto(
                                    codigoBarras, 
                                    imagemBytes, 
                                    "Placeholder", 
                                    "Sistema", 
                                    usuarioId, 
                                    _env.WebRootPath
                                );
                                
                                imagemInfo = imagemSalva;
                                Console.WriteLine($"Placeholder salvo para código: {codigoBarras}");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Erro ao salvar placeholder: {ex.Message}");
                            }
                        }
                    }

                    var post = new PostWebDTO
                    {
                        CodigoBarras = codigoBarras,
                        Preco = preco,
                        TemImagem = imagemBytes != null,
                        ImagemBytes = imagemBytes,
                        ImagemInfo = imagemInfo,
                        DataProcessamento = DateTime.Now
                    };

                    Console.WriteLine($"📊 Post criado: {codigoBarras} - TemImagem: {post.TemImagem} - ImagemBytes: {(imagemBytes?.Length ?? 0)} bytes");
                    
                    // Verificar se a imagem é válida
                    if (imagemBytes != null)
                    {
                        try
                        {
                            using var ms = new MemoryStream(imagemBytes);
                            using var img = System.Drawing.Image.FromStream(ms);
                            Console.WriteLine($"✅ Imagem válida: {img.Width}x{img.Height} pixels");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Imagem inválida: {ex.Message}");
                        }
                    }
                    
                    posts.Add(post);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar linha {csv.Parser.Row}: {ex.Message}");
                    Console.WriteLine($"Dados da linha: {csv.Parser.RawRecord}");
                    continue; // Pular esta linha e continuar com a próxima
                }
            }

            Console.WriteLine($"Total de posts processados: {posts.Count}");
            return posts;
        }

        [HttpPost]
        public async Task<IActionResult> GerarPosts()
        {
            // Verificar se usuário está logado
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            if (!usuarioId.HasValue)
                return RedirectToAction("Index", "Login");

            // Recuperar dados do preview da session
            var previewDataJson = HttpContext.Session.GetString("PostWebPreviewData");
            if (string.IsNullOrEmpty(previewDataJson))
                return BadRequest("Dados do preview não encontrados");

            var previewData = JsonSerializer.Deserialize<PostWebPreviewData>(previewDataJson);
            if (previewData == null)
                return BadRequest("Dados do preview inválidos");

            try
            {
                // Gerar posts para produtos com imagens
                var postsComImagens = previewData.Posts.Where(p => p.TemImagem).ToList();
                var postsGerados = new List<byte[]>();

                foreach (var post in postsComImagens)
                {
                    var postBytes = _postGenerator.GerarPostWeb(post, previewData.FundoSelecionado, _env.WebRootPath);
                    postsGerados.Add(postBytes);
                }

                // Criar ZIP com todos os posts
                var zipBytes = CriarZipComPosts(postsGerados);
                
                return File(zipBytes, "application/zip", "posts_web.zip");
            }
            catch (Exception ex)
            {
                return BadRequest($"Erro ao gerar posts: {ex.Message}");
            }
        }

        private byte[] CriarZipComPosts(List<byte[]> posts)
        {
            using var memoryStream = new MemoryStream();
            using var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true);
            
            for (int i = 0; i < posts.Count; i++)
            {
                var entry = archive.CreateEntry($"post_{i + 1}.png");
                using var entryStream = entry.Open();
                entryStream.Write(posts[i], 0, posts[i].Length);
            }
            
            return memoryStream.ToArray();
        }

        private byte[] CriarImagemPlaceholder(string codigoBarras, decimal preco)
        {
            try
            {
                // Criar imagem placeholder 400x400
                using var bitmap = new System.Drawing.Bitmap(400, 400);
                using var graphics = System.Drawing.Graphics.FromImage(bitmap);

                // Configurar qualidade
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Fundo gradiente
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new System.Drawing.Rectangle(0, 0, 400, 400),
                    System.Drawing.Color.FromArgb(240, 240, 240),
                    System.Drawing.Color.FromArgb(200, 200, 200),
                    System.Drawing.Drawing2D.LinearGradientMode.Vertical
                );
                graphics.FillRectangle(brush, 0, 0, 400, 400);

                // Borda
                using var pen = new System.Drawing.Pen(System.Drawing.Color.Gray, 2);
                graphics.DrawRectangle(pen, 1, 1, 398, 398);

                // Ícone de produto
                using var iconFont = new System.Drawing.Font("Arial", 48, System.Drawing.FontStyle.Bold);
                using var iconBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(100, 100, 100));
                var iconText = "📦";
                var iconSize = graphics.MeasureString(iconText, iconFont);
                var iconX = (400 - iconSize.Width) / 2;
                var iconY = 100;
                graphics.DrawString(iconText, iconFont, iconBrush, iconX, iconY);

                // Código de barras
                using var codeFont = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular);
                using var codeBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(80, 80, 80));
                var codeText = $"Código: {codigoBarras}";
                var codeSize = graphics.MeasureString(codeText, codeFont);
                var codeX = (400 - codeSize.Width) / 2;
                var codeY = 200;
                graphics.DrawString(codeText, codeFont, codeBrush, codeX, codeY);

                // Preço
                using var priceFont = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
                using var priceBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(0, 120, 0));
                var priceText = preco.ToString("C", new System.Globalization.CultureInfo("pt-BR"));
                var priceSize = graphics.MeasureString(priceText, priceFont);
                var priceX = (400 - priceSize.Width) / 2;
                var priceY = 250;
                graphics.DrawString(priceText, priceFont, priceBrush, priceX, priceY);

                // Texto "Imagem não encontrada"
                using var notFoundFont = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Italic);
                using var notFoundBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(150, 150, 150));
                var notFoundText = "Imagem não encontrada";
                var notFoundSize = graphics.MeasureString(notFoundText, notFoundFont);
                var notFoundX = (400 - notFoundSize.Width) / 2;
                var notFoundY = 300;
                graphics.DrawString(notFoundText, notFoundFont, notFoundBrush, notFoundX, notFoundY);

                // Converter para byte array
                using var memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar placeholder: {ex.Message}");
                // Retornar imagem vazia em caso de erro
                return new byte[0];
            }
        }

        private async Task SalvarImagensTemporarias(List<PostWebDTO> posts, int usuarioId)
        {
            try
            {
                var tempDir = Path.Combine(_env.WebRootPath, "temp", "imagens", usuarioId.ToString());
                Directory.CreateDirectory(tempDir);

                foreach (var post in posts.Where(p => p.TemImagem && p.ImagemBytes != null))
                {
                    var fileName = $"{post.CodigoBarras}_{DateTime.Now:yyyyMMddHHmmss}.png";
                    var filePath = Path.Combine(tempDir, fileName);
                    
                    await System.IO.File.WriteAllBytesAsync(filePath, post.ImagemBytes);
                    post.ImagemInfo = new ImagemProdutoDTO
                    {
                        CaminhoImagem = $"/temp/imagens/{usuarioId}/{fileName}",
                        NomeArquivo = fileName
                    };
                    
                    Console.WriteLine($"💾 Imagem temporária salva: {fileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao salvar imagens temporárias: {ex.Message}");
            }
        }
    }
}
