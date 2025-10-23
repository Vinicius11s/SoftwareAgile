using Domain.DTOs;

namespace Interfaces.Repository
{
    public interface IImagemProdutoRepository
    {
        Task<ImagemProdutoDTO?> ObterImagemPorCodigoBarras(string codigoBarras, int usuarioId);
        Task<ImagemProdutoDTO> AdicionarImagem(ImagemProdutoDTO imagemDto);
        Task<bool> AtualizarImagem(ImagemProdutoDTO imagemDto);
        Task<bool> ExcluirImagem(int id, int usuarioId);
        Task<List<ImagemProdutoDTO>> ObterImagensPorUsuario(int usuarioId);
        Task<bool> ExisteImagemPorCodigoBarras(string codigoBarras, int usuarioId);
        Task<List<ImagemProdutoDTO>> ObterImagensPorCodigosBarras(List<string> codigosBarras, int usuarioId);
        Task<bool> MarcarComoInativo(int id, int usuarioId);
        Task<List<ImagemProdutoDTO>> ObterImagensInativas(int usuarioId);
        Task<bool> ReativarImagem(int id, int usuarioId);
    }
}

