using Domain.DTOs;
using Interfaces.Repository;
using Interfaces.Service;
using System.IO;

namespace Services
{
    public class ImagemProdutoService : IImagemProdutoService
    {
        private readonly IImagemProdutoRepository _repository;

        public ImagemProdutoService(IImagemProdutoRepository repository)
        {
            _repository = repository;
        }

        public async Task<byte[]?> ObterImagemProduto(string codigoBarras, int usuarioId)
        {
            var imagemInfo = await _repository.ObterImagemPorCodigoBarras(codigoBarras, usuarioId);
            if (imagemInfo == null)
                return null;

            // Verificar se o arquivo físico existe
            var caminhoCompleto = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagemInfo.CaminhoImagem.TrimStart('/'));
            if (!File.Exists(caminhoCompleto))
                return null;

            return await File.ReadAllBytesAsync(caminhoCompleto);
        }

        public async Task<ImagemProdutoDTO?> ObterImagemProdutoInfo(string codigoBarras, int usuarioId)
        {
            return await _repository.ObterImagemPorCodigoBarras(codigoBarras, usuarioId);
        }

        public async Task<ImagemProdutoDTO> SalvarImagemProduto(string codigoBarras, byte[] imagem, string urlOrigem, string fonteImagem, int usuarioId, string webRootPath)
        {
            // Validar imagem
            if (imagem == null || imagem.Length == 0)
                throw new ArgumentException("Imagem não pode ser vazia");

            if (imagem.Length > 10 * 1024 * 1024) // 10MB máximo
                throw new ArgumentException("Imagem muito grande. Máximo 10MB");

            // Determinar tipo de arquivo baseado no conteúdo
            var tipoArquivo = DeterminarTipoArquivo(imagem);
            if (string.IsNullOrEmpty(tipoArquivo))
                throw new ArgumentException("Formato de imagem não suportado");

            // Criar diretório do usuário se não existir
            var userFolder = Path.Combine(webRootPath, "imagens", "produtos", usuarioId.ToString());
            Directory.CreateDirectory(userFolder);

            // Gerar nome único para o arquivo
            var fileName = $"{codigoBarras}_{Guid.NewGuid()}.{tipoArquivo}";
            var filePath = Path.Combine(userFolder, fileName);

            // Salvar arquivo
            await File.WriteAllBytesAsync(filePath, imagem);

            // Criar DTO
            var imagemDto = new ImagemProdutoDTO
            {
                CodigoBarras = codigoBarras,
                CaminhoImagem = $"/imagens/produtos/{usuarioId}/{fileName}",
                NomeArquivo = fileName,
                UrlOrigem = urlOrigem,
                FonteImagem = fonteImagem,
                DataBusca = DateTime.Now,
                DataUpload = DateTime.Now,
                UsuarioId = usuarioId,
                Ativo = true,
                TamanhoArquivo = imagem.Length,
                TipoArquivo = tipoArquivo
            };

            return await _repository.AdicionarImagem(imagemDto);
        }

        public async Task<List<ImagemProdutoDTO>> ObterImagensPorUsuario(int usuarioId)
        {
            return await _repository.ObterImagensPorUsuario(usuarioId);
        }

        public async Task<bool> ExcluirImagem(int id, int usuarioId, string webRootPath)
        {
            var imagem = await _repository.ObterImagemPorCodigoBarras("", usuarioId); // Buscar por ID seria melhor
            if (imagem == null)
                return false;

            // Deletar arquivo físico
            var filePath = Path.Combine(webRootPath, imagem.CaminhoImagem.TrimStart('/'));
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Deletar do banco
            return await _repository.ExcluirImagem(id, usuarioId);
        }

        public async Task<bool> MarcarComoInativo(int id, int usuarioId)
        {
            return await _repository.MarcarComoInativo(id, usuarioId);
        }

        public async Task<List<ImagemProdutoDTO>> ObterImagensInativas(int usuarioId)
        {
            return await _repository.ObterImagensInativas(usuarioId);
        }

        public async Task<bool> ReativarImagem(int id, int usuarioId)
        {
            return await _repository.ReativarImagem(id, usuarioId);
        }

        public async Task<Dictionary<string, byte[]>> ObterImagensPorCodigosBarras(List<string> codigosBarras, int usuarioId)
        {
            var imagensInfo = await _repository.ObterImagensPorCodigosBarras(codigosBarras, usuarioId);
            var resultado = new Dictionary<string, byte[]>();

            foreach (var imagemInfo in imagensInfo)
            {
                var imagemBytes = await ObterImagemProduto(imagemInfo.CodigoBarras, usuarioId);
                if (imagemBytes != null)
                {
                    resultado[imagemInfo.CodigoBarras] = imagemBytes;
                }
            }

            return resultado;
        }

        public async Task<bool> ExisteImagemPorCodigoBarras(string codigoBarras, int usuarioId)
        {
            return await _repository.ExisteImagemPorCodigoBarras(codigoBarras, usuarioId);
        }

        private static string DeterminarTipoArquivo(byte[] imagem)
        {
            if (imagem.Length < 4)
                return string.Empty;

            // Verificar assinaturas de arquivo
            if (imagem[0] == 0xFF && imagem[1] == 0xD8 && imagem[2] == 0xFF)
                return "jpg";
            
            if (imagem[0] == 0x89 && imagem[1] == 0x50 && imagem[2] == 0x4E && imagem[3] == 0x47)
                return "png";
            
            if (imagem[0] == 0x47 && imagem[1] == 0x49 && imagem[2] == 0x46)
                return "gif";
            
            if (imagem[0] == 0x42 && imagem[1] == 0x4D)
                return "bmp";

            return string.Empty;
        }
    }
}

