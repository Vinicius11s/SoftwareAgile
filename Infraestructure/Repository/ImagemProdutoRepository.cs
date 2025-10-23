using Domain.DTOs;
using Domain.Entities;
using Infraestructure.Context;
using Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infraestructure.Repository
{
    public class ImagemProdutoRepository : IImagemProdutoRepository
    {
        private readonly EmpresaContexto _context;

        public ImagemProdutoRepository(EmpresaContexto context)
        {
            _context = context;
        }

        public async Task<ImagemProdutoDTO?> ObterImagemPorCodigoBarras(string codigoBarras, int usuarioId)
        {
            var imagem = await _context.ImagensProduto
                .Where(i => i.CodigoBarras == codigoBarras && i.UsuarioId == usuarioId && i.Ativo)
                .FirstOrDefaultAsync();

            return imagem != null ? ConverterParaDTO(imagem) : null;
        }

        public async Task<ImagemProdutoDTO> AdicionarImagem(ImagemProdutoDTO imagemDto)
        {
            var imagem = new ImagemProduto
            {
                CodigoBarras = imagemDto.CodigoBarras,
                CaminhoImagem = imagemDto.CaminhoImagem,
                NomeArquivo = imagemDto.NomeArquivo,
                UrlOrigem = imagemDto.UrlOrigem,
                FonteImagem = imagemDto.FonteImagem,
                DataBusca = imagemDto.DataBusca,
                DataUpload = imagemDto.DataUpload,
                UsuarioId = imagemDto.UsuarioId,
                Ativo = imagemDto.Ativo,
                TamanhoArquivo = imagemDto.TamanhoArquivo,
                TipoArquivo = imagemDto.TipoArquivo
            };

            _context.ImagensProduto.Add(imagem);
            await _context.SaveChangesAsync();

            imagemDto.Id = imagem.Id;
            return imagemDto;
        }

        public async Task<bool> AtualizarImagem(ImagemProdutoDTO imagemDto)
        {
            var imagem = await _context.ImagensProduto
                .FirstOrDefaultAsync(i => i.Id == imagemDto.Id && i.UsuarioId == imagemDto.UsuarioId);

            if (imagem == null)
                return false;

            imagem.CaminhoImagem = imagemDto.CaminhoImagem;
            imagem.NomeArquivo = imagemDto.NomeArquivo;
            imagem.UrlOrigem = imagemDto.UrlOrigem;
            imagem.FonteImagem = imagemDto.FonteImagem;
            imagem.TamanhoArquivo = imagemDto.TamanhoArquivo;
            imagem.TipoArquivo = imagemDto.TipoArquivo;
            imagem.Ativo = imagemDto.Ativo;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExcluirImagem(int id, int usuarioId)
        {
            var imagem = await _context.ImagensProduto
                .FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == usuarioId);

            if (imagem == null)
                return false;

            _context.ImagensProduto.Remove(imagem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ImagemProdutoDTO>> ObterImagensPorUsuario(int usuarioId)
        {
            var imagens = await _context.ImagensProduto
                .Where(i => i.UsuarioId == usuarioId && i.Ativo)
                .OrderByDescending(i => i.DataBusca)
                .ToListAsync();

            return imagens.Select(ConverterParaDTO).ToList();
        }

        public async Task<bool> ExisteImagemPorCodigoBarras(string codigoBarras, int usuarioId)
        {
            return await _context.ImagensProduto
                .AnyAsync(i => i.CodigoBarras == codigoBarras && i.UsuarioId == usuarioId && i.Ativo);
        }

        public async Task<List<ImagemProdutoDTO>> ObterImagensPorCodigosBarras(List<string> codigosBarras, int usuarioId)
        {
            var imagens = await _context.ImagensProduto
                .Where(i => codigosBarras.Contains(i.CodigoBarras) && i.UsuarioId == usuarioId && i.Ativo)
                .ToListAsync();

            return imagens.Select(ConverterParaDTO).ToList();
        }

        public async Task<bool> MarcarComoInativo(int id, int usuarioId)
        {
            var imagem = await _context.ImagensProduto
                .FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == usuarioId);

            if (imagem == null)
                return false;

            imagem.Ativo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ImagemProdutoDTO>> ObterImagensInativas(int usuarioId)
        {
            var imagens = await _context.ImagensProduto
                .Where(i => i.UsuarioId == usuarioId && !i.Ativo)
                .OrderByDescending(i => i.DataBusca)
                .ToListAsync();

            return imagens.Select(ConverterParaDTO).ToList();
        }

        public async Task<bool> ReativarImagem(int id, int usuarioId)
        {
            var imagem = await _context.ImagensProduto
                .FirstOrDefaultAsync(i => i.Id == id && i.UsuarioId == usuarioId);

            if (imagem == null)
                return false;

            imagem.Ativo = true;
            await _context.SaveChangesAsync();
            return true;
        }

        private static ImagemProdutoDTO ConverterParaDTO(ImagemProduto imagem)
        {
            return new ImagemProdutoDTO
            {
                Id = imagem.Id,
                CodigoBarras = imagem.CodigoBarras,
                CaminhoImagem = imagem.CaminhoImagem,
                NomeArquivo = imagem.NomeArquivo,
                UrlOrigem = imagem.UrlOrigem,
                FonteImagem = imagem.FonteImagem,
                DataBusca = imagem.DataBusca,
                DataUpload = imagem.DataUpload,
                UsuarioId = imagem.UsuarioId,
                Ativo = imagem.Ativo,
                TamanhoArquivo = imagem.TamanhoArquivo,
                TipoArquivo = imagem.TipoArquivo
            };
        }
    }
}

