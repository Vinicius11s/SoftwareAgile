using Domain.DTOs;

namespace Interfaces.Service
{
    public interface IImagemProdutoService
    {
        Task<byte[]?> ObterImagemProduto(string codigoBarras, int usuarioId);
        Task<ImagemProdutoDTO?> ObterImagemProdutoInfo(string codigoBarras, int usuarioId);
        Task<ImagemProdutoDTO> SalvarImagemProduto(string codigoBarras, byte[] imagem, string urlOrigem, string fonteImagem, int usuarioId, string webRootPath);
        Task<List<ImagemProdutoDTO>> ObterImagensPorUsuario(int usuarioId);
        Task<bool> ExcluirImagem(int id, int usuarioId, string webRootPath);
        Task<bool> MarcarComoInativo(int id, int usuarioId);
        Task<List<ImagemProdutoDTO>> ObterImagensInativas(int usuarioId);
        Task<bool> ReativarImagem(int id, int usuarioId);
        Task<Dictionary<string, byte[]>> ObterImagensPorCodigosBarras(List<string> codigosBarras, int usuarioId);
        Task<bool> ExisteImagemPorCodigoBarras(string codigoBarras, int usuarioId);
    }
}

