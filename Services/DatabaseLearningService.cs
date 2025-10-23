using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
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
                if (string.IsNullOrWhiteSpace(tipoCorrecao) || textoOriginal == textoCorrigido)
                {
                    Console.WriteLine($"Correção ignorada - valores inválidos: Original='{textoOriginal}', Corrigido='{textoCorrigido}', Tipo='{tipoCorrecao}'");
                    return;
                }
                
                // Permite correções mesmo com texto original vazio (para casos de remoção)
                if (string.IsNullOrWhiteSpace(textoOriginal))
                {
                    Console.WriteLine($"Correção ignorada - texto original vazio: Original='{textoOriginal}', Corrigido='{textoCorrigido}'");
                    return;
                }

                // Validações mais permissivas para permitir limpezas/encurtamentos legítimos
                // Bloqueia apenas correções extremamente curtas (≤ 1 char)
                if (textoCorrigido.Trim().Length <= 1)
                {
                    Console.WriteLine($"Correção ignorada - texto muito curto: Corrigido='{textoCorrigido}' ({textoCorrigido?.Length ?? 0} chars)");
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
                Console.WriteLine($"=== REGISTRAR CORREÇÃO ===");
                Console.WriteLine($"Original: '{textoOriginal}' (Length: {textoOriginal?.Length ?? 0})");
                Console.WriteLine($"Corrigido: '{textoCorrigido}' (Length: {textoCorrigido?.Length ?? 0})");
                Console.WriteLine($"Tipo: {tipoCorrecao}");
                Console.WriteLine($"Sessão: {sessaoId}");
                Console.WriteLine($"São iguais? {textoOriginal == textoCorrigido}");
                
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
            {
                Console.WriteLine($"AplicarCorrecoesAprendidasAsync: Texto vazio para tipo '{tipoCorrecao}'");
                return texto;
            }

            Console.WriteLine($"AplicarCorrecoesAprendidasAsync: Buscando correção para '{texto}' (Tipo: {tipoCorrecao}, UsuarioId: {usuarioId}, EmpresaId: {empresaId})");
            
            // Busca correção direta (texto original -> texto corrigido)
            var correcaoDireta = await _context.CorrecoesAprendidas
                .Where(c => c.Ativo && c.TipoCorrecao == tipoCorrecao)
                .Where(c => c.UsuarioId == (usuarioId ?? "ANONIMO") && c.EmpresaId == (empresaId ?? "DEFAULT"))
                .Where(c => c.TextoOriginal.ToLower() == texto.ToLower())
                .OrderByDescending(c => c.DataAtualizacao)
                .FirstOrDefaultAsync();
                
            Console.WriteLine($"Query executada - Encontrou {(correcaoDireta != null ? 1 : 0)} correção(s)");
            
            if (correcaoDireta != null)
            {
                Console.WriteLine($"CORREÇÃO DIRETA ENCONTRADA: '{correcaoDireta.TextoOriginal}' -> '{correcaoDireta.TextoCorrigido}' (Freq: {correcaoDireta.FrequenciaUso})");
                
                // Atualiza frequência de uso
                correcaoDireta.FrequenciaUso++;
                correcaoDireta.UltimaUtilizacao = DateTime.Now;
                correcaoDireta.DataAtualizacao = DateTime.Now;
                await _context.SaveChangesAsync();
                
                return correcaoDireta.TextoCorrigido;
            }
            
            // Segunda tentativa: match ignorando acentos e caixa
            var candidatos = await _context.CorrecoesAprendidas
                .Where(c => c.Ativo && c.TipoCorrecao == tipoCorrecao)
                .Where(c => c.UsuarioId == (usuarioId ?? "ANONIMO") && c.EmpresaId == (empresaId ?? "DEFAULT"))
                .OrderByDescending(c => c.DataAtualizacao)
                .ToListAsync();

            var textoNormalizado = NormalizeText(texto);
            Console.WriteLine($"Texto normalizado: '{texto}' -> '{textoNormalizado}'");
            
            foreach (var candidato in candidatos)
            {
                var candidatoNormalizado = NormalizeText(candidato.TextoOriginal);
                Console.WriteLine($"Candidato: '{candidato.TextoOriginal}' -> '{candidatoNormalizado}'");
                if (candidatoNormalizado == textoNormalizado)
                {
                    Console.WriteLine($"CORREÇÃO (NORMALIZADA) ENCONTRADA: '{candidato.TextoOriginal}' -> '{candidato.TextoCorrigido}'");
                    candidato.FrequenciaUso++;
                    candidato.UltimaUtilizacao = DateTime.Now;
                    candidato.DataAtualizacao = DateTime.Now;
                    await _context.SaveChangesAsync();
                    return candidato.TextoCorrigido;
                }
            }

            // Terceira tentativa: substituição por token (palavra inteira), diacrítico-insensível
            // Ex.: REFR -> REFRESCO, LV -> LONGA VIDA
            var tokens = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            bool alterado = false;
            for (int i = 0; i < tokens.Count; i++)
            {
                var tokenNorm = NormalizeText(tokens[i]);
                var candidatoToken = candidatos.FirstOrDefault(c => NormalizeText(c.TextoOriginal) == tokenNorm);
                if (candidatoToken != null)
                {
                    Console.WriteLine($"SUBSTITUIÇÃO POR TOKEN: '{tokens[i]}' -> '{candidatoToken.TextoCorrigido}'");
                    tokens[i] = candidatoToken.TextoCorrigido;
                    candidatoToken.FrequenciaUso++;
                    candidatoToken.UltimaUtilizacao = DateTime.Now;
                    candidatoToken.DataAtualizacao = DateTime.Now;
                    alterado = true;
                }
            }
            if (alterado)
            {
                await _context.SaveChangesAsync();
                var textoSubstituido = string.Join(" ", tokens).Trim();
                return textoSubstituido;
            }

            // Se não encontrou correção direta, busca correção em cadeia
            // Exemplo: Se temos "COLA" -> "LATA" e "LATA" -> "ORIGINAL", 
            // quando o sistema extrair "COLA", deve retornar "ORIGINAL"
            var correcaoEmCadeia = await BuscarCorrecaoEmCadeiaAsync(texto, tipoCorrecao, usuarioId, empresaId);
            if (correcaoEmCadeia != null)
            {
                Console.WriteLine($"CORREÇÃO EM CADEIA ENCONTRADA: '{texto}' -> '{correcaoEmCadeia}'");
                return correcaoEmCadeia;
            }
            
            Console.WriteLine($"NENHUMA CORREÇÃO ENCONTRADA para '{texto}' (Tipo: {tipoCorrecao})");
            return texto;
        }

        private static string NormalizeText(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var formD = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in formD)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            var noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);
            // Normaliza espaços e caixa
            var collapsed = string.Join(" ", noDiacritics.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            return collapsed.ToUpperInvariant().Trim();
        }

        public string AplicarCorrecoesAprendidas(string texto, string tipoCorrecao, string usuarioId = null, string empresaId = null)
        {
            try
            {
                return AplicarCorrecoesAprendidasAsync(texto, tipoCorrecao, usuarioId, empresaId).GetAwaiter().GetResult();
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

        public List<CorrecaoAprendida> ObterTodasCorrecoesAprendidas(string tipoCorrecao = null, string usuarioId = null, string empresaId = null)
        {
            try
            {
                var resultado = ObterTodasCorrecoesAprendidasAsync(tipoCorrecao, usuarioId, empresaId).GetAwaiter().GetResult();
                Console.WriteLine($"ObterTodasCorrecoesAprendidas retornou {resultado.Count} correções");
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao obter todas as correções: {ex.Message}");
                return new List<CorrecaoAprendida>();
            }
        }

        public async Task<List<CorrecaoAprendida>> ObterTodasCorrecoesAprendidasAsync(string tipoCorrecao = null, string usuarioId = null, string empresaId = null)
        {
            try
            {
                Console.WriteLine($"ObterTodasCorrecoesAprendidasAsync chamado com tipoCorrecao: {tipoCorrecao}, usuarioId: {usuarioId}, empresaId: {empresaId}");

                var query = _context.CorrecoesAprendidas
                    .Where(c => c.UsuarioId == (usuarioId ?? "ANONIMO") && c.EmpresaId == (empresaId ?? "DEFAULT"));

                // Não filtrar por Ativo = true (inclui desativadas)
                if (!string.IsNullOrEmpty(tipoCorrecao))
                    query = query.Where(c => c.TipoCorrecao == tipoCorrecao);

                var correcoes = await query
                    .OrderByDescending(c => c.FrequenciaUso)
                    .ToListAsync();

                Console.WriteLine($"Query retornou {correcoes.Count} correções do banco (incluindo desativadas) para usuário {usuarioId ?? "ANONIMO"}");
                
                // Log detalhado das correções
                foreach (var c in correcoes.Take(5)) // Log das primeiras 5
                {
                    Console.WriteLine($"Correção: ID={c.Id}, Original='{c.TextoOriginal}', Corrigido='{c.TextoCorrigido}', Ativo={c.Ativo}, UsuarioId='{c.UsuarioId}', EmpresaId='{c.EmpresaId}'");
                }

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
                Console.WriteLine($"Erro no ObterTodasCorrecoesAprendidasAsync: {ex.Message}");
                return new List<CorrecaoAprendida>();
            }
        }

        public List<CorrecaoAprendida> ObterCorrecoesDesativadas(string tipoCorrecao = null, string usuarioId = null, string empresaId = null)
        {
            return ObterCorrecoesDesativadasAsync(tipoCorrecao, usuarioId, empresaId).GetAwaiter().GetResult();
        }

        public async Task<List<CorrecaoAprendida>> ObterCorrecoesDesativadasAsync(string tipoCorrecao = null, string usuarioId = null, string empresaId = null)
        {
            try
            {
                Console.WriteLine($"ObterCorrecoesDesativadasAsync chamado com tipoCorrecao: {tipoCorrecao}, usuarioId: {usuarioId}, empresaId: {empresaId}");

                var query = _context.CorrecoesAprendidas
                    .Where(c => c.Ativo == false && c.UsuarioId == (usuarioId ?? "ANONIMO") && c.EmpresaId == (empresaId ?? "DEFAULT"));

                if (!string.IsNullOrEmpty(tipoCorrecao))
                    query = query.Where(c => c.TipoCorrecao == tipoCorrecao);

                var correcoes = await query
                    .OrderByDescending(c => c.DataAtualizacao)
                    .ToListAsync();

                Console.WriteLine($"Query retornou {correcoes.Count} correções DESATIVADAS do banco para usuário {usuarioId ?? "ANONIMO"}");
                
                // Log detalhado das correções
                foreach (var c in correcoes.Take(5)) // Log das primeiras 5
                {
                    Console.WriteLine($"Correção DESATIVADA: ID={c.Id}, Original='{c.TextoOriginal}', Corrigido='{c.TextoCorrigido}', Ativo={c.Ativo}, UsuarioId='{c.UsuarioId}', EmpresaId='{c.EmpresaId}'");
                }

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

                Console.WriteLine($"Convertido para {resultado.Count} DTOs DESATIVADAS");
                return resultado;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no ObterCorrecoesDesativadasAsync: {ex.Message}");
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
                // Regra mais permissiva: só marca como problemática se o texto ficou praticamente vazio
                var correcoesProblematicas = await _context.CorrecoesAprendidas
                    .Where(c => c.TextoCorrigido.Trim().Length <= 1)
                    .ToListAsync();

                if (correcoesProblematicas.Any())
                {
                    Console.WriteLine($"Removendo {correcoesProblematicas.Count} correções problemáticas (texto cortado demais)");
                    _context.CorrecoesAprendidas.RemoveRange(correcoesProblematicas);
                }

                // Mantém apenas o filtro mínimo absoluto
                var correcoesMuitoCurta = await _context.CorrecoesAprendidas
                    .Where(c => c.TextoCorrigido.Trim().Length <= 1)
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

        private async Task<string> BuscarCorrecaoEmCadeiaAsync(string textoInicial, string tipoCorrecao, string usuarioId, string empresaId)
        {
            var textoAtual = textoInicial;
            var textosVisitados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var maxIteracoes = 10; // Evita loops infinitos
            var iteracao = 0;

            while (iteracao < maxIteracoes)
            {
                if (textosVisitados.Contains(textoAtual))
                {
                    Console.WriteLine($"LOOP DETECTADO na correção em cadeia para '{textoInicial}'");
                    break;
                }

                textosVisitados.Add(textoAtual);

                var proximaCorrecao = await _context.CorrecoesAprendidas
                    .Where(c => c.Ativo && c.TipoCorrecao == tipoCorrecao)
                    .Where(c => c.UsuarioId == (usuarioId ?? "ANONIMO") && c.EmpresaId == (empresaId ?? "DEFAULT"))
                    .Where(c => c.TextoOriginal.ToLower() == textoAtual.ToLower())
                    .OrderByDescending(c => c.DataAtualizacao)
                    .FirstOrDefaultAsync();

                if (proximaCorrecao == null)
                {
                    break; // Não há mais correções na cadeia
                }

                Console.WriteLine($"CADEIA: '{textoAtual}' -> '{proximaCorrecao.TextoCorrigido}'");
                textoAtual = proximaCorrecao.TextoCorrigido;
                iteracao++;
            }

            // Se encontrou uma correção diferente do texto inicial, retorna
            if (textoAtual != textoInicial)
            {
                return textoAtual;
            }

            return null;
        }

        public async Task DebugCorrecoesVariedadeAsync()
        {
            try
            {
                Console.WriteLine("=== DEBUG CORREÇÕES VARIEDADE ===");
                
                var correcoesVariedade = await _context.CorrecoesAprendidas
                    .Where(c => c.TipoCorrecao == "VARIEDADE")
                    .ToListAsync();
                
                Console.WriteLine($"Total de correções de variedade: {correcoesVariedade.Count}");
                
                foreach (var correcao in correcoesVariedade)
                {
                    Console.WriteLine($"ID: {correcao.Id}, Original: '{correcao.TextoOriginal}', Corrigido: '{correcao.TextoCorrigido}', Ativo: {correcao.Ativo}, Freq: {correcao.FrequenciaUso}");
                }
                
                var historicoVariedade = await _context.HistoricoCorrecoes
                    .Where(h => h.TipoCorrecao == "VARIEDADE")
                    .OrderByDescending(h => h.DataCorrecao)
                    .Take(10)
                    .ToListAsync();
                
                Console.WriteLine($"Últimas 10 correções de variedade no histórico:");
                foreach (var hist in historicoVariedade)
                {
                    Console.WriteLine($"Data: {hist.DataCorrecao}, Original: '{hist.TextoOriginal}', Corrigido: '{hist.TextoCorrigido}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no debug: {ex.Message}");
            }
        }

        public async Task DebugCorrecoesNomesAsync(string usuarioId = null, string empresaId = null)
        {
            try
            {
                Console.WriteLine("=== DEBUG CORREÇÕES NOMES ===");
                Console.WriteLine($"UsuarioId: {usuarioId}, EmpresaId: {empresaId}");
                
                var correcoesNomes = await _context.CorrecoesAprendidas
                    .Where(c => c.TipoCorrecao == "NOME")
                    .Where(c => c.UsuarioId == (usuarioId ?? "ANONIMO") && c.EmpresaId == (empresaId ?? "DEFAULT"))
                    .ToListAsync();
                
                Console.WriteLine($"Total de correções de nomes para este usuário: {correcoesNomes.Count}");
                
                foreach (var correcao in correcoesNomes)
                {
                    Console.WriteLine($"ID: {correcao.Id}, Original: '{correcao.TextoOriginal}', Corrigido: '{correcao.TextoCorrigido}', Ativo: {correcao.Ativo}, UsuarioId: '{correcao.UsuarioId}', EmpresaId: '{correcao.EmpresaId}'");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no debug de nomes: {ex.Message}");
            }
        }

        public async Task TestarCorrecaoAsync(string texto, string tipoCorrecao, string usuarioId = null, string empresaId = null)
        {
            try
            {
                Console.WriteLine($"=== TESTE DE CORREÇÃO ===");
                Console.WriteLine($"Texto: '{texto}', Tipo: {tipoCorrecao}, UsuarioId: {usuarioId}, EmpresaId: {empresaId}");
                
                var resultado = await AplicarCorrecoesAprendidasAsync(texto, tipoCorrecao, usuarioId, empresaId);
                Console.WriteLine($"Resultado: '{resultado}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro no teste: {ex.Message}");
            }
        }
    }
}
