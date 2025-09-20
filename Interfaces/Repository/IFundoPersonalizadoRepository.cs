using Domain.DTOs;
using Domain.Entities;

namespace Interfaces.Repository
{
    public interface IFundoPersonalizadoRepository
    {
        Task<List<FundoPersonalizadoDTO>> ObterFundosPorUsuarioETipo(int usuarioId, string tipoImpressao);
        Task<FundoPersonalizadoDTO?> ObterFundoPorId(int id, int usuarioId);
        Task<FundoPersonalizadoDTO> AdicionarFundo(FundoPersonalizadoDTO fundo);
        Task<bool> AtualizarFundo(FundoPersonalizadoDTO fundo);
        Task<bool> ExcluirFundo(int id, int usuarioId);
        Task<bool> ExisteFundoComNome(string nome, int usuarioId, int? idExcluir = null);
    }
}
