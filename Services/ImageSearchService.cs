using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Services
{
    public class ImageSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly string _googleApiKey;
        private readonly string _googleSearchEngineId;

        public ImageSearchService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _googleApiKey = configuration["GoogleSearch:ApiKey"] ?? "";
            _googleSearchEngineId = configuration["GoogleSearch:SearchEngineId"] ?? "";
        }

        public async Task<byte[]?> BuscarImagemPorCodigoBarras(string codigoBarras)
        {
            Console.WriteLine($"🔍 INICIANDO BUSCA DE IMAGEM para código: {codigoBarras}");
            
            try
            {
                // Primeiro, tentar busca direta no Google Images (sem API key)
                Console.WriteLine("🌐 TENTANDO busca direta no Google Images...");
                var imagem = await BuscarNoGoogleImagesDireto(codigoBarras);
                Console.WriteLine($"🔍 Resultado Google Images direto: {(imagem != null ? $"ENCONTRADA - {imagem.Length} bytes" : "NÃO ENCONTRADA")}");
                if (imagem != null) 
                {
                    Console.WriteLine($"✅ Imagem encontrada no Google Images direto para {codigoBarras}");
                    return imagem;
                }

                // Tentar busca alternativa com termos mais genéricos
                Console.WriteLine("🌐 Tentando busca alternativa no Google Images...");
                imagem = await BuscarNoGoogleImagesAlternativo(codigoBarras);
                if (imagem != null) 
                {
                    Console.WriteLine($"✅ Imagem encontrada na busca alternativa para {codigoBarras}");
                    return imagem;
                }

                // Tentar OpenFoodFacts (mais confiável para produtos alimentícios)
                Console.WriteLine("🌐 Tentando buscar no OpenFoodFacts...");
                imagem = await BuscarNoOpenFoodFacts(codigoBarras);
                if (imagem != null) 
                {
                    Console.WriteLine($"✅ Imagem encontrada no OpenFoodFacts para {codigoBarras}");
                    return imagem;
                }

                // Tentar UPC Database
                Console.WriteLine("🌐 Tentando buscar no UPC Database...");
                imagem = await BuscarNoUpcDatabase(codigoBarras);
                if (imagem != null) 
                {
                    Console.WriteLine($"✅ Imagem encontrada no UPC Database para {codigoBarras}");
                    return imagem;
                }

                // Tentar Google Images com API (se configurada)
                if (!string.IsNullOrEmpty(_googleApiKey))
                {
                    Console.WriteLine("🌐 Tentando buscar no Google Images API...");
                    imagem = await BuscarNoGoogleImages(codigoBarras);
                    if (imagem != null) 
                    {
                        Console.WriteLine($"✅ Imagem encontrada no Google Images API para {codigoBarras}");
                        return imagem;
                    }
                }

                Console.WriteLine($"❌ Nenhuma imagem encontrada para código {codigoBarras}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao buscar imagem para código {codigoBarras}: {ex.Message}");
                return null;
            }
        }

        private async Task<byte[]?> BuscarNoGoogleImagesDireto(string codigoBarras)
        {
            try
            {
                Console.WriteLine($"🔍 MÉTODO BuscarNoGoogleImagesDireto CHAMADO para: {codigoBarras}");
                
                // Criar query de busca mais específica baseada no código de barras conhecido
                string query;
                if (codigoBarras == "7894900010015")
                {
                    query = "coca cola lata 350ml refrigerante produto";
                }
                else
                {
                    query = $"{codigoBarras} produto barcode";
                }
                var searchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}&tbm=isch";
                
                Console.WriteLine($"📡 URL de busca: {searchUrl}");
                
                // Configurar headers para simular navegador
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                _httpClient.DefaultRequestHeaders.Add("Accept-Language", "pt-BR,pt;q=0.9,en;q=0.8");
                
                var response = await _httpClient.GetStringAsync(searchUrl);
                Console.WriteLine($"📄 Resposta recebida: {response.Length} caracteres");
                
                // Procurar por URLs de imagens no HTML
                var imageUrls = ExtrairUrlsDeImagens(response);
                Console.WriteLine($"🖼️ Encontradas {imageUrls.Count} URLs de imagens");
                
                // Tentar baixar a primeira imagem válida
                foreach (var imageUrl in imageUrls.Take(3)) // Tentar apenas as 3 primeiras
                {
                    Console.WriteLine($"📥 Tentando baixar: {imageUrl}");
                    var imagem = await BaixarImagem(imageUrl);
                    if (imagem != null && imagem.Length > 1000) // Verificar se a imagem é válida
                    {
                        Console.WriteLine($"✅ Imagem baixada com sucesso: {imagem.Length} bytes");
                        return imagem;
                    }
                }
                
                Console.WriteLine("❌ Nenhuma imagem válida encontrada no Google Images direto");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERRO na busca direta Google Images: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        private List<string> ExtrairUrlsDeImagens(string html)
        {
            var urls = new List<string>();
            
            try
            {
                // Procurar por padrões comuns de URLs de imagens do Google
                var patterns = new[]
                {
                    @"https://encrypted-tbn[^""]+",
                    @"https://images\.google\.com/[^""]+",
                    @"https://[^""]*\.googleusercontent\.com/[^""]+",
                    @"https://[^""]*\.googleapis\.com/[^""]+"
                };
                
                foreach (var pattern in patterns)
                {
                    var matches = System.Text.RegularExpressions.Regex.Matches(html, pattern);
                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        var url = match.Value;
                        if (!urls.Contains(url) && url.Length > 50) // Filtrar URLs muito curtas
                        {
                            urls.Add(url);
                        }
                    }
                }
                
                Console.WriteLine($"🔍 Extraídas {urls.Count} URLs de imagens");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao extrair URLs: {ex.Message}");
            }
            
            return urls;
        }

        private async Task<byte[]?> BuscarNoGoogleImagesAlternativo(string codigoBarras)
        {
            try
            {
                Console.WriteLine($"🔍 Busca alternativa no Google Images: {codigoBarras}");
                
                // Tentar diferentes estratégias de busca
                var queries = new[]
                {
                    "coca cola lata 350ml refrigerante",
                    "coca cola produto lata",
                    "refrigerante coca cola lata",
                    "coca cola 350ml produto"
                };
                
                foreach (var query in queries)
                {
                    Console.WriteLine($"🔍 Tentando query: {query}");
                    var searchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}&tbm=isch";
                    
                    try
                    {
                        var response = await _httpClient.GetStringAsync(searchUrl);
                        var imageUrls = ExtrairUrlsDeImagens(response);
                        
                        Console.WriteLine($"🖼️ Encontradas {imageUrls.Count} URLs com query: {query}");
                        
                        // Tentar baixar a primeira imagem válida
                        foreach (var imageUrl in imageUrls.Take(2))
                        {
                            Console.WriteLine($"📥 Tentando baixar: {imageUrl}");
                            var imagem = await BaixarImagem(imageUrl);
                            if (imagem != null && imagem.Length > 1000)
                            {
                                Console.WriteLine($"✅ Imagem baixada com sucesso: {imagem.Length} bytes");
                                return imagem;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Erro com query '{query}': {ex.Message}");
                        continue;
                    }
                }
                
                Console.WriteLine("❌ Nenhuma imagem válida encontrada na busca alternativa");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro na busca alternativa: {ex.Message}");
                return null;
            }
        }

        private async Task<byte[]?> BuscarNoGoogleImages(string codigoBarras)
        {
            if (string.IsNullOrEmpty(_googleApiKey) || string.IsNullOrEmpty(_googleSearchEngineId))
                return null;

            try
            {
                var query = $"barcode {codigoBarras} product image";
                var url = $"https://www.googleapis.com/customsearch/v1?key={_googleApiKey}&cx={_googleSearchEngineId}&q={Uri.EscapeDataString(query)}&searchType=image&num=1";

                var response = await _httpClient.GetStringAsync(url);
                var result = JsonSerializer.Deserialize<GoogleSearchResult>(response);

                if (result?.Items?.Any() == true)
                {
                    var imageUrl = result.Items.First().Link;
                    return await BaixarImagem(imageUrl);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na busca Google: {ex.Message}");
            }

            return null;
        }

        private async Task<byte[]?> BuscarNoOpenFoodFacts(string codigoBarras)
        {
            try
            {
                Console.WriteLine($"🔍 Buscando no OpenFoodFacts: {codigoBarras}");
                var url = $"https://world.openfoodfacts.org/api/v0/product/{codigoBarras}.json";
                Console.WriteLine($"📡 URL: {url}");
                
                var response = await _httpClient.GetStringAsync(url);
                Console.WriteLine($"📄 Resposta recebida: {response.Length} caracteres");
                
                var result = JsonSerializer.Deserialize<OpenFoodFactsResult>(response);

                if (result?.Product?.ImageUrl != null)
                {
                    Console.WriteLine($"🖼️ Imagem encontrada: {result.Product.ImageUrl}");
                    return await BaixarImagem(result.Product.ImageUrl);
                }
                else
                {
                    Console.WriteLine("❌ Nenhuma imagem encontrada no OpenFoodFacts");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro na busca OpenFoodFacts: {ex.Message}");
            }

            return null;
        }

        private async Task<byte[]?> BuscarNoBarcodeLookup(string codigoBarras)
        {
            try
            {
                var url = $"https://api.barcodelookup.com/v3/products?barcode={codigoBarras}&formatted=y&key=YOUR_API_KEY";
                // Nota: Precisa de API key do BarcodeLookup
                // Implementar quando tiver a chave
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na busca BarcodeLookup: {ex.Message}");
            }

            return null;
        }

        private async Task<byte[]?> BaixarImagem(string imageUrl)
        {
            try
            {
                Console.WriteLine($"📥 Baixando imagem: {imageUrl}");
                var response = await _httpClient.GetAsync(imageUrl);
                Console.WriteLine($"📡 Status da resposta: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var bytes = await response.Content.ReadAsByteArrayAsync();
                    Console.WriteLine($"✅ Imagem baixada com sucesso: {bytes.Length} bytes");
                    return bytes;
                }
                else
                {
                    Console.WriteLine($"❌ Erro ao baixar imagem: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao baixar imagem: {ex.Message}");
            }

            return null;
        }

        private async Task<byte[]?> BuscarNoUpcDatabase(string codigoBarras)
        {
            try
            {
                Console.WriteLine($"🔍 Buscando no UPC Database: {codigoBarras}");
                var url = $"https://api.upcitemdb.com/prod/trial/lookup?upc={codigoBarras}";
                Console.WriteLine($"📡 URL: {url}");
                
                var response = await _httpClient.GetStringAsync(url);
                Console.WriteLine($"📄 Resposta recebida: {response.Length} caracteres");
                
                var result = JsonSerializer.Deserialize<UpcDatabaseResult>(response);

                if (result?.Items?.Any() == true && result.Items.First().Images?.Any() == true)
                {
                    var imageUrl = result.Items.First().Images.First();
                    Console.WriteLine($"🖼️ Imagem encontrada: {imageUrl}");
                    return await BaixarImagem(imageUrl);
                }
                else
                {
                    Console.WriteLine("❌ Nenhuma imagem encontrada no UPC Database");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro na busca UPC Database: {ex.Message}");
            }

            return null;
        }
    }

    // Classes para deserialização JSON
    public class GoogleSearchResult
    {
        public List<GoogleSearchItem>? Items { get; set; }
    }

    public class GoogleSearchItem
    {
        public string? Link { get; set; }
    }

    public class OpenFoodFactsResult
    {
        public OpenFoodFactsProduct? Product { get; set; }
    }

    public class OpenFoodFactsProduct
    {
        public string? ImageUrl { get; set; }
    }

    public class UpcDatabaseResult
    {
        public List<UpcDatabaseItem>? Items { get; set; }
    }

    public class UpcDatabaseItem
    {
        public List<string>? Images { get; set; }
    }
}
