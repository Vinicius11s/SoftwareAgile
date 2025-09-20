using Domain.DTOs;

namespace Interfaces.Service
{
    public interface IFundoPersonalizadoService
    {
        Task<List<(string Nome, string Imagem, string Id)>> ObterFundosParaViewPorTipo(int usuarioId, string tipoImpressao);
        Task<FundoPersonalizadoDTO?> ObterFundoPorId(int id, int usuarioId);
        Task<FundoPersonalizadoDTO> AdicionarFundo(byte[] arquivoBytes, string nomeArquivo, string nome, string tipoImpressao, int usuarioId, string webRootPath);
        Task<bool> AtualizarFundo(int id, byte[]? arquivoBytes, string? nomeArquivo, string nome, string tipoImpressao, int usuarioId, string webRootPath);
        Task<bool> ExcluirFundo(int id, int usuarioId, string webRootPath);
    }
}
