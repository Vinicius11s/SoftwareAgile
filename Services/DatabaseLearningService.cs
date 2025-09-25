using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.DTOs;
using Domain.Entities;
using Infraestructure.Context;
using Interfaces.Service;
using Microsoft.EntityFrameworkCore;

namespace Services
{
    public class DatabaseLearningService : ILearningService
    {
        private readonly EmpresaContexto _context;

        public DatabaseLearningService(EmpresaContexto context)
        {
            _context = context;
        }

        public async Task RegistrarCorrecaoAsync(string textoOriginal, string textoCorrigido, string tipoCorrecao, string sessaoId, string usuarioId = null, string empresaId = null)
        {
            try
            {
                // Validação: não permite registrar correções com valores vazios
                if (string.IsNullOrWhiteSpace(textoOriginal) || string.IsNullOrWhiteSpace(textoCorrigido) || 
                    string.IsNullOrWhiteSpace(tipoCorrecao) || textoOriginal == textoCorrigido)
                {
                    Console.WriteLine($"Correção ignorada - valores inválidos: Original='{textoOriginal}', Corrigido='{textoCorrigido}', Tipo='{tipoCorrecao}'");
                    return;
                }

                // Validação: não permite correções que cortam texto (corrigido menor que original)
                if (textoCorrigido.Length < textoOriginal.Length * 0.7) // Se corrigido for 30% menor que original
                {
                    Console.WriteLine($"Correção ignorada - texto cortado demais: Original='{textoOriginal}' ({textoOriginal.Length} chars), Corrigido='{textoCorrigido}' ({textoCorrigido.Length} chars)");
                    return;
                }

                // Validação: não permite correções que resultam em texto muito curto
                if (textoCorrigido.Length < 3)
                {
                    Console.WriteLine($"Correção ignorada - texto muito curto: Corrigido='{textoCorrigido}' ({textoCorrigido.Length} chars)");
                    return;
                }

                // Registra no histórico
                var historico = new HistoricoCorrecoesEntity
                {
                    TextoOriginal = textoOriginal ?? string.Empty,
                    TextoCorrigido = textoCorrigido ?? string.Empty,
                    TipoCorrecao = tipoCorrecao ?? string.Empty,
                    DataCorrecao = DateTime.Now,
                    SessaoId = sessaoId ?? string.Empty,
                    UsuarioId = usuarioId ?? "ANONIMO",
                    EmpresaId = empresaId ?? "DEFAULT"
                };
                _context.HistoricoCorrecoes.Add(historico);

                // Verifica se já existe uma correção similar
                var correcaoExistente = await _context.CorrecoesAprendidas
                    .Where(c => c.TextoOriginal.ToLower() == textoOriginal.ToLower())
                    .Where(c => c.TipoCorrecao == tipoCorrecao)
                    .Where(c => c.UsuarioId == (usuarioId ?? "ANONIMO"))
                    .Where(c => c.EmpresaId == (empresaId ?? "DEFAULT"))
                    .FirstOrDefaultAsync();

                if (correcaoExistente != null)
                {
                    // Atualiza correção existente
                    correcaoExistente.TextoCorrigido = textoCorrigido ?? string.Empty;
                    correcaoExistente.FrequenciaUso++;
                    correcaoExistente.UltimaUtilizacao = DateTime.Now;
                    correcaoExistente.DataAtualizacao = DateTime.Now;
                    correcaoExistente.UsuarioAtualizacao = usuarioId ?? "ANONIMO";
                }
                else
                {
                // Cria nova correção
                var novaCorrecao = new CorrecaoAprendidaEntity
                {
                    TextoOriginal = textoOriginal ?? string.Empty,
                    TextoCorrigido = textoCorrigido ?? string.Empty,
                    TipoCorrecao = tipoCorrecao ?? string.Empty,
                    FrequenciaUso = 1,
                    DataCriacao = DateTime.Now,
                    UltimaUtilizacao = DateTime.Now,
                    DataAtualizacao = DateTime.Now,
                    Ativo = true,
                    UsuarioId = usuarioId ?? "ANONIMO",
                    EmpresaId = empresaId ?? "DEFAULT",
                    UsuarioAtualizacao = usuarioId ?? "ANONIMO"
                };
                    _context.CorrecoesAprendidas.Add(novaCorrecao);
                }

                await _context.SaveChangesAsync();
                Console.WriteLine("SaveChangesAsync executado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro detalhado no RegistrarCorrecaoAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public void RegistrarCorrecao(string textoOriginal, string textoCorrigido, string tipoCorrecao, string sessaoId)
        {
            try
            {
                Console.WriteLine($"DatabaseLearningService.RegistrarCorrecao chamado: '{textoOriginal}' -> '{textoCorrigido}' (Tipo: {tipoCorrecao})");
                RegistrarCorrecaoAsync(textoOriginal, textoCorrigido, tipoCorrecao, sessaoId).GetAwaiter().GetResult();
                Console.WriteLine($"Correção registrada com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao registrar correção: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        public async Task<string> AplicarCorrecoesAprendidasAsync(string texto, string tipoCorrecao, string usuarioId = null, string empresaId = null)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            var correcao = await _context.CorrecoesAprendidas
                .Where(c => c.Ativo && c.TipoCorrecao == tipoCorrecao)
                .Where(c => c.UsuarioId == (usuarioId ?? "ANONIMO") && c.EmpresaId == (empresaId ?? "DEFAULT"))
                .Where(c => c.TextoOriginal.ToLower() == texto.ToLower())
                .FirstOrDefaultAsync();

            if (correcao != null)
            {
                // Atualiza frequência de uso
                correcao.FrequenciaUso++;
                correcao.UltimaUtilizacao = DateTime.Now;
                correcao.DataAtualizacao = DateTime.Now;
                await _context.SaveChangesAsync();
                
                return correcao.TextoCorrigido;
            }

            return texto;
        }

        public string AplicarCorrecoesAprendidas(string texto, string tipoCorrecao)
        {
            try
            {
                return AplicarCorrecoesAprendidasAsync(texto, tipoCorrecao).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao aplicar correções: {ex.Message}");
                return texto; // Retorna o texto original em caso de erro
            }
        }

        public async Task<List<CorrecaoAprendida>> ObterCorrecoesAprendidasAsync(string tipoCorrecao = null, string usuarioId = null, string empresaId = null)
        {
            try
            {
                var query = _context.CorrecoesAprendidas
                    .Where(c => c.Ativo)
                    .Where(c => c.UsuarioId == (usuarioId ?? "ANONIMO") && c.EmpresaId == (empresaId ?? "DEFAULT"));

                if (!string.IsNullOrEmpty(tipoCorrecao))
                    query = query.Where(c => c.TipoCorrecao == tipoCorrecao);

                var correcoes = await query
                    .OrderByDescending(c => c.FrequenciaUso)
                    .ToListAsync();

                Console.WriteLine($"Query retornou {correcoes.Count} correções do banco");

                var resultado = correcoes.Select(c => new CorrecaoAprendida
                {
                    Id = c.Id,
                    TextoOriginal = c.TextoOriginal,
                    TextoCorrigido = c.TextoCorrigido,
                    TipoCorrecao = c.TipoCorrecao,
                    FrequenciaUso = c.FrequenciaUso,
                    DataCriacao = c.DataCriacao,
                    UltimaUtilizacao = c.UltimaUtilizacao,
                    Ativo = c.Ativo,
                    UsuarioId = c.UsuarioId
                }).ToList();

                Console.WriteLine($"Convertido para {resultado.Count} DTOs");
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no ObterCorrecoesAprendidasAsync: {ex.Message}");
                return new List<CorrecaoAprendida>();
            }
        }

        public List<CorrecaoAprendida> ObterCorrecoesAprendidas(string tipoCorrecao = null)
        {
            try
            {
                var resultado = ObterCorrecoesAprendidasAsync(tipoCorrecao).GetAwaiter().GetResult();
                Console.WriteLine($"ObterCorrecoesAprendidas retornou {resultado.Count} correções");
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter correções: {ex.Message}");
                return new List<CorrecaoAprendida>();
            }
        }

        public async Task DesativarCorrecaoAsync(int id)
        {
            var correcao = await _context.CorrecoesAprendidas.FindAsync(id);
            if (correcao != null)
            {
                correcao.Ativo = false;
                correcao.DataAtualizacao = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public void DesativarCorrecao(int id)
        {
            DesativarCorrecaoAsync(id).Wait();
        }

        public async Task AtivarCorrecaoAsync(int id)
        {
            var correcao = await _context.CorrecoesAprendidas.FindAsync(id);
            if (correcao != null)
            {
                correcao.Ativo = true;
                correcao.DataAtualizacao = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public void AtivarCorrecao(int id)
        {
            AtivarCorrecaoAsync(id).Wait();
        }

        public async Task RemoverCorrecaoAsync(int id)
        {
            var correcao = await _context.CorrecoesAprendidas.FindAsync(id);
            if (correcao != null)
            {
                _context.CorrecoesAprendidas.Remove(correcao);
                await _context.SaveChangesAsync();
            }
        }

        public void RemoverCorrecao(int id)
        {
            RemoverCorrecaoAsync(id).Wait();
        }

        public async Task<List<HistoricoCorrecoes>> ObterHistoricoAsync(int limite = 50, string usuarioId = null, string empresaId = null)
        {
            var historico = await _context.HistoricoCorrecoes
                .Where(h => h.UsuarioId == (usuarioId ?? "ANONIMO") && h.EmpresaId == (empresaId ?? "DEFAULT"))
                .OrderByDescending(h => h.DataCorrecao)
                .Take(limite)
                .ToListAsync();

            return historico.Select(h => new HistoricoCorrecoes
            {
                Id = h.Id,
                TextoOriginal = h.TextoOriginal,
                TextoCorrigido = h.TextoCorrigido,
                TipoCorrecao = h.TipoCorrecao,
                DataCorrecao = h.DataCorrecao,
                UsuarioId = h.UsuarioId,
                SessaoId = h.SessaoId
            }).ToList();
        }

        public List<HistoricoCorrecoes> ObterHistorico(int limite = 50)
        {
            return ObterHistoricoAsync(limite).Result;
        }

        public async Task<Dictionary<string, int>> ObterEstatisticasAsync(string usuarioId = null, string empresaId = null)
        {
            var query = _context.CorrecoesAprendidas
                .Where(c => c.UsuarioId == (usuarioId ?? "ANONIMO") && c.EmpresaId == (empresaId ?? "DEFAULT"));

            var totalCorrecoes = await query.CountAsync();
            var correcoesAtivas = await query.CountAsync(c => c.Ativo);
            var totalHistorico = await _context.HistoricoCorrecoes
                .Where(h => h.UsuarioId == (usuarioId ?? "ANONIMO") && h.EmpresaId == (empresaId ?? "DEFAULT"))
                .CountAsync();

            return new Dictionary<string, int>
            {
                ["TotalCorrecoes"] = totalCorrecoes,
                ["CorrecoesAtivas"] = correcoesAtivas,
                ["TotalHistorico"] = totalHistorico,
                ["CorrecoesNomes"] = await query.CountAsync(c => c.TipoCorrecao == "NOME"),
                ["CorrecoesGramagem"] = await query.CountAsync(c => c.TipoCorrecao == "GRAMAGEM"),
                ["CorrecoesVariedade"] = await query.CountAsync(c => c.TipoCorrecao == "VARIEDADE")
            };
        }

        public Dictionary<string, int> ObterEstatisticas()
        {
            return ObterEstatisticasAsync().Result;
        }

        public async Task LimparCorrecoesInvalidasAsync()
        {
            try
            {
                // Remove correções com valores vazios
                var correcoesInvalidas = await _context.CorrecoesAprendidas
                    .Where(c => string.IsNullOrWhiteSpace(c.TextoOriginal) || 
                               string.IsNullOrWhiteSpace(c.TextoCorrigido) ||
                               c.TextoOriginal == c.TextoCorrigido)
                    .ToListAsync();

                if (correcoesInvalidas.Any())
                {
                    Console.WriteLine($"Removendo {correcoesInvalidas.Count} correções inválidas");
                    _context.CorrecoesAprendidas.RemoveRange(correcoesInvalidas);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Correções inválidas removidas com sucesso!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao limpar correções inválidas: {ex.Message}");
            }
        }

        public void LimparCorrecoesInvalidas()
        {
            LimparCorrecoesInvalidasAsync().GetAwaiter().GetResult();
        }

        public async Task LimparCorrecoesProblematicasAsync()
        {
            try
            {
                // Remove correções que cortam texto demais
                var correcoesProblematicas = await _context.CorrecoesAprendidas
                    .Where(c => c.TextoCorrigido.Length < c.TextoOriginal.Length * 0.7)
                    .ToListAsync();

                if (correcoesProblematicas.Any())
                {
                    Console.WriteLine($"Removendo {correcoesProblematicas.Count} correções problemáticas (texto cortado demais)");
                    _context.CorrecoesAprendidas.RemoveRange(correcoesProblematicas);
                }

                // Remove correções com texto muito curto
                var correcoesMuitoCurta = await _context.CorrecoesAprendidas
                    .Where(c => c.TextoCorrigido.Length < 3)
                    .ToListAsync();

                if (correcoesMuitoCurta.Any())
                {
                    Console.WriteLine($"Removendo {correcoesMuitoCurta.Count} correções com texto muito curto");
                    _context.CorrecoesAprendidas.RemoveRange(correcoesMuitoCurta);
                }

                // Remove correções que terminam com espaço (indicam corte)
                var correcoesComEspaco = await _context.CorrecoesAprendidas
                    .Where(c => c.TextoCorrigido.EndsWith(" "))
                    .ToListAsync();

                if (correcoesComEspaco.Any())
                {
                    Console.WriteLine($"Removendo {correcoesComEspaco.Count} correções que terminam com espaço");
                    _context.CorrecoesAprendidas.RemoveRange(correcoesComEspaco);
                }

                if (correcoesProblematicas.Any() || correcoesMuitoCurta.Any() || correcoesComEspaco.Any())
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Correções problemáticas removidas com sucesso!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao limpar correções problemáticas: {ex.Message}");
            }
        }

        public void LimparCorrecoesProblematicas()
        {
            LimparCorrecoesProblematicasAsync().GetAwaiter().GetResult();
        }
    }
}
