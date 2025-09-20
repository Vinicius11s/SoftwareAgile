using Domain.DTOs;
using Domain.Entities;
using Infraestructure.Context;
using Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Repository
{
    public class FundoPersonalizadoRepository : IFundoPersonalizadoRepository
    {
        private readonly EmpresaContexto _context;

        public FundoPersonalizadoRepository(EmpresaContexto context)
        {
            _context = context;
        }

        public async Task<List<FundoPersonalizadoDTO>> ObterFundosPorUsuarioETipo(int usuarioId, string tipoImpressao)
        {
            return await _context.FundosPersonalizados
                .Where(f => f.UsuarioId == usuarioId && f.TipoImpressao == tipoImpressao && f.Ativo)
                .Select(f => new FundoPersonalizadoDTO
                {
                    Id = f.Id,
                    Nome = f.Nome,
                    CaminhoImagem = f.CaminhoImagem,
                    NomeArquivo = f.NomeArquivo,
                    DataUpload = f.DataUpload,
                    UsuarioId = f.UsuarioId,
                    TipoImpressao = f.TipoImpressao,
                    Ativo = f.Ativo
                })
                .OrderBy(f => f.Nome)
                .ToListAsync();
        }

        public async Task<FundoPersonalizadoDTO?> ObterFundoPorId(int id, int usuarioId)
        {
            var fundo = await _context.FundosPersonalizados
                .Where(f => f.Id == id && f.UsuarioId == usuarioId && f.Ativo)
                .Select(f => new FundoPersonalizadoDTO
                {
                    Id = f.Id,
                    Nome = f.Nome,
                    CaminhoImagem = f.CaminhoImagem,
                    NomeArquivo = f.NomeArquivo,
                    DataUpload = f.DataUpload,
                    UsuarioId = f.UsuarioId,
                    TipoImpressao = f.TipoImpressao,
                    Ativo = f.Ativo
                })
                .FirstOrDefaultAsync();

            return fundo;
        }

        public async Task<FundoPersonalizadoDTO> AdicionarFundo(FundoPersonalizadoDTO fundoDto)
        {
            var fundo = new FundoPersonalizado
            {
                Nome = fundoDto.Nome,
                CaminhoImagem = fundoDto.CaminhoImagem,
                NomeArquivo = fundoDto.NomeArquivo,
                DataUpload = DateTime.Now,
                UsuarioId = fundoDto.UsuarioId,
                TipoImpressao = fundoDto.TipoImpressao,
                Ativo = true
            };

            _context.FundosPersonalizados.Add(fundo);
            await _context.SaveChangesAsync();

            fundoDto.Id = fundo.Id;
            fundoDto.DataUpload = fundo.DataUpload;
            return fundoDto;
        }

        public async Task<bool> AtualizarFundo(FundoPersonalizadoDTO fundoDto)
        {
            var fundo = await _context.FundosPersonalizados
                .FirstOrDefaultAsync(f => f.Id == fundoDto.Id && f.UsuarioId == fundoDto.UsuarioId);

            if (fundo == null)
                return false;

            fundo.Nome = fundoDto.Nome;
            fundo.CaminhoImagem = fundoDto.CaminhoImagem;
            fundo.NomeArquivo = fundoDto.NomeArquivo;
            fundo.TipoImpressao = fundoDto.TipoImpressao;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExcluirFundo(int id, int usuarioId)
        {
            var fundo = await _context.FundosPersonalizados
                .FirstOrDefaultAsync(f => f.Id == id && f.UsuarioId == usuarioId);

            if (fundo == null)
                return false;

            // Soft delete - marca como inativo
            fundo.Ativo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExisteFundoComNome(string nome, int usuarioId, int? idExcluir = null)
        {
            var query = _context.FundosPersonalizados
                .Where(f => f.UsuarioId == usuarioId && f.Ativo && f.Nome == nome);

            if (idExcluir.HasValue)
                query = query.Where(f => f.Id != idExcluir.Value);

            return await query.AnyAsync();
        }
    }
}
