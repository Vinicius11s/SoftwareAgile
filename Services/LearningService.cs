using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Domain.DTOs;

namespace Services
{
    public class LearningService
    {
        private readonly string _learningDataPath;
        private readonly string _historyDataPath;
        private List<CorrecaoAprendida> _correcoesAprendidas;
        private List<HistoricoCorrecoes> _historicoCorrecoes;

        public LearningService()
        {
            _learningDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "correcoes_aprendidas.json");
            _historyDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "historico_correcoes.json");
            
            // Garante que o diretório existe
            Directory.CreateDirectory(Path.GetDirectoryName(_learningDataPath));
            
            CarregarDados();
        }

        private void CarregarDados()
        {
            try
            {
                // Carrega correções aprendidas
                if (File.Exists(_learningDataPath))
                {
                    var json = File.ReadAllText(_learningDataPath);
                    _correcoesAprendidas = JsonSerializer.Deserialize<List<CorrecaoAprendida>>(json) ?? new List<CorrecaoAprendida>();
                }
                else
                {
                    _correcoesAprendidas = new List<CorrecaoAprendida>();
                }

                // Carrega histórico
                if (File.Exists(_historyDataPath))
                {
                    var json = File.ReadAllText(_historyDataPath);
                    _historicoCorrecoes = JsonSerializer.Deserialize<List<HistoricoCorrecoes>>(json) ?? new List<HistoricoCorrecoes>();
                }
                else
                {
                    _historicoCorrecoes = new List<HistoricoCorrecoes>();
                }
            }
            catch (Exception ex)
            {
                // Em caso de erro, inicializa listas vazias
                _correcoesAprendidas = new List<CorrecaoAprendida>();
                _historicoCorrecoes = new List<HistoricoCorrecoes>();
                Console.WriteLine($"Erro ao carregar dados de aprendizado: {ex.Message}");
            }
        }

        private void SalvarDados()
        {
            try
            {
                // Salva correções aprendidas
                var json = JsonSerializer.Serialize(_correcoesAprendidas, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_learningDataPath, json);

                // Salva histórico (mantém apenas os últimos 1000 registros)
                var historicoLimitado = _historicoCorrecoes
                    .OrderByDescending(h => h.DataCorrecao)
                    .Take(1000)
                    .ToList();
                
                json = JsonSerializer.Serialize(historicoLimitado, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_historyDataPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar dados de aprendizado: {ex.Message}");
            }
        }

        public void RegistrarCorrecao(string textoOriginal, string textoCorrigido, string tipoCorrecao, string sessaoId)
        {
            // Registra no histórico
            var historico = new HistoricoCorrecoes
            {
                TextoOriginal = textoOriginal,
                TextoCorrigido = textoCorrigido,
                TipoCorrecao = tipoCorrecao,
                DataCorrecao = DateTime.Now,
                SessaoId = sessaoId
            };
            _historicoCorrecoes.Add(historico);

            // Verifica se já existe uma correção similar
            var correcaoExistente = _correcoesAprendidas
                .FirstOrDefault(c => c.TextoOriginal.Equals(textoOriginal, StringComparison.OrdinalIgnoreCase) 
                                  && c.TipoCorrecao == tipoCorrecao);

            if (correcaoExistente != null)
            {
                // Atualiza correção existente
                correcaoExistente.TextoCorrigido = textoCorrigido;
                correcaoExistente.FrequenciaUso++;
                correcaoExistente.UltimaUtilizacao = DateTime.Now;
            }
            else
            {
                // Cria nova correção
                var novaCorrecao = new CorrecaoAprendida
                {
                    Id = _correcoesAprendidas.Count + 1,
                    TextoOriginal = textoOriginal,
                    TextoCorrigido = textoCorrigido,
                    TipoCorrecao = tipoCorrecao,
                    FrequenciaUso = 1,
                    DataCriacao = DateTime.Now,
                    UltimaUtilizacao = DateTime.Now,
                    Ativo = true
                };
                _correcoesAprendidas.Add(novaCorrecao);
            }

            SalvarDados();
        }

        public string AplicarCorrecoesAprendidas(string texto, string tipoCorrecao)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            var correcao = _correcoesAprendidas
                .Where(c => c.Ativo && c.TipoCorrecao == tipoCorrecao)
                .FirstOrDefault(c => c.TextoOriginal.Equals(texto, StringComparison.OrdinalIgnoreCase));

            if (correcao != null)
            {
                // Atualiza frequência de uso
                correcao.FrequenciaUso++;
                correcao.UltimaUtilizacao = DateTime.Now;
                SalvarDados();
                
                return correcao.TextoCorrigido;
            }

            return texto;
        }

        public List<CorrecaoAprendida> ObterCorrecoesAprendidas(string tipoCorrecao = null)
        {
            if (string.IsNullOrEmpty(tipoCorrecao))
                return _correcoesAprendidas.Where(c => c.Ativo).ToList();

            return _correcoesAprendidas
                .Where(c => c.Ativo && c.TipoCorrecao == tipoCorrecao)
                .OrderByDescending(c => c.FrequenciaUso)
                .ToList();
        }

        public void DesativarCorrecao(int id)
        {
            var correcao = _correcoesAprendidas.FirstOrDefault(c => c.Id == id);
            if (correcao != null)
            {
                correcao.Ativo = false;
                SalvarDados();
            }
        }

        public void AtivarCorrecao(int id)
        {
            var correcao = _correcoesAprendidas.FirstOrDefault(c => c.Id == id);
            if (correcao != null)
            {
                correcao.Ativo = true;
                SalvarDados();
            }
        }

        public void RemoverCorrecao(int id)
        {
            _correcoesAprendidas.RemoveAll(c => c.Id == id);
            SalvarDados();
        }

        public List<HistoricoCorrecoes> ObterHistorico(int limite = 50)
        {
            return _historicoCorrecoes
                .OrderByDescending(h => h.DataCorrecao)
                .Take(limite)
                .ToList();
        }

        public Dictionary<string, int> ObterEstatisticas()
        {
            return new Dictionary<string, int>
            {
                ["TotalCorrecoes"] = _correcoesAprendidas.Count,
                ["CorrecoesAtivas"] = _correcoesAprendidas.Count(c => c.Ativo),
                ["TotalHistorico"] = _historicoCorrecoes.Count,
                ["CorrecoesNomes"] = _correcoesAprendidas.Count(c => c.TipoCorrecao == "NOME"),
                ["CorrecoesGramagem"] = _correcoesAprendidas.Count(c => c.TipoCorrecao == "GRAMAGEM"),
                ["CorrecoesVariedade"] = _correcoesAprendidas.Count(c => c.TipoCorrecao == "VARIEDADE")
            };
        }
    }
}


