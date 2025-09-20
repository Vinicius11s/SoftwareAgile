using Domain.DTOs;
using Interfaces.Repository;
using Interfaces.Service;
using System.Globalization;

namespace Services
{
    public class FundoPersonalizadoService : IFundoPersonalizadoService
    {
        private readonly IFundoPersonalizadoRepository _repository;

        public FundoPersonalizadoService(IFundoPersonalizadoRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<(string Nome, string Imagem, string Id)>> ObterFundosParaViewPorTipo(int usuarioId, string tipoImpressao)
        {
            var fundos = await _repository.ObterFundosPorUsuarioETipo(usuarioId, tipoImpressao);
            
            return fundos.Select(f => (
                Nome: f.Nome,
                Imagem: f.CaminhoImagem,
                Id: f.Id.ToString()
            )).ToList();
        }

        public async Task<FundoPersonalizadoDTO?> ObterFundoPorId(int id, int usuarioId)
        {
            return await _repository.ObterFundoPorId(id, usuarioId);
        }

        public async Task<FundoPersonalizadoDTO> AdicionarFundo(byte[] arquivoBytes, string nomeArquivo, string nome, string tipoImpressao, int usuarioId, string webRootPath)
        {
            // Validar arquivo
            if (arquivoBytes == null || arquivoBytes.Length == 0)
                throw new ArgumentException("Arquivo não pode ser vazio");

            if (!IsImageFile(nomeArquivo))
                throw new ArgumentException("Arquivo deve ser uma imagem válida (JPG, PNG, JPEG)");

            // Verificar se já existe fundo com este nome
            if (await _repository.ExisteFundoComNome(nome, usuarioId))
                throw new ArgumentException("Já existe um fundo com este nome");

            // Criar diretório do usuário se não existir
            var userFolder = Path.Combine(webRootPath, "fundos", "usuarios", usuarioId.ToString());
            Directory.CreateDirectory(userFolder);

            // Gerar nome único para o arquivo
            var extension = Path.GetExtension(nomeArquivo);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(userFolder, fileName);

            // Salvar arquivo
            await File.WriteAllBytesAsync(filePath, arquivoBytes);

            // Salvar no banco
            var fundoDto = new FundoPersonalizadoDTO
            {
                Nome = nome,
                CaminhoImagem = $"/fundos/usuarios/{usuarioId}/{fileName}",
                NomeArquivo = fileName,
                UsuarioId = usuarioId,
                TipoImpressao = tipoImpressao,
                Ativo = true
            };

            return await _repository.AdicionarFundo(fundoDto);
        }

        public async Task<bool> AtualizarFundo(int id, byte[]? arquivoBytes, string? nomeArquivo, string nome, string tipoImpressao, int usuarioId, string webRootPath)
        {
            var fundoExistente = await _repository.ObterFundoPorId(id, usuarioId);
            if (fundoExistente == null)
                return false;

            // Verificar se já existe fundo com este nome (excluindo o atual)
            if (await _repository.ExisteFundoComNome(nome, usuarioId, id))
                throw new ArgumentException("Já existe um fundo com este nome");

            var fundoDto = new FundoPersonalizadoDTO
            {
                Id = id,
                Nome = nome,
                CaminhoImagem = fundoExistente.CaminhoImagem,
                NomeArquivo = fundoExistente.NomeArquivo,
                UsuarioId = usuarioId,
                TipoImpressao = tipoImpressao,
                Ativo = true
            };

            // Se um novo arquivo foi enviado
            if (arquivoBytes != null && arquivoBytes.Length > 0 && !string.IsNullOrEmpty(nomeArquivo))
            {
                if (!IsImageFile(nomeArquivo))
                    throw new ArgumentException("Arquivo deve ser uma imagem válida (JPG, PNG, JPEG)");

                // Deletar arquivo antigo
                var oldFilePath = Path.Combine(webRootPath, fundoExistente.CaminhoImagem.TrimStart('/'));
                if (File.Exists(oldFilePath))
                    File.Delete(oldFilePath);

                // Salvar novo arquivo
                var userFolder = Path.Combine(webRootPath, "fundos", "usuarios", usuarioId.ToString());
                Directory.CreateDirectory(userFolder);

                var extension = Path.GetExtension(nomeArquivo);
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(userFolder, fileName);

                await File.WriteAllBytesAsync(filePath, arquivoBytes);

                fundoDto.CaminhoImagem = $"/fundos/usuarios/{usuarioId}/{fileName}";
                fundoDto.NomeArquivo = fileName;
            }

            return await _repository.AtualizarFundo(fundoDto);
        }

        public async Task<bool> ExcluirFundo(int id, int usuarioId, string webRootPath)
        {
            var fundo = await _repository.ObterFundoPorId(id, usuarioId);
            if (fundo == null)
                return false;

            // Deletar arquivo físico
            var filePath = Path.Combine(webRootPath, fundo.CaminhoImagem.TrimStart('/'));
            if (File.Exists(filePath))
                File.Delete(filePath);

            // Marcar como inativo no banco
            return await _repository.ExcluirFundo(id, usuarioId);
        }

        private bool IsImageFile(string fileName)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Contains(extension);
        }
    }
}
