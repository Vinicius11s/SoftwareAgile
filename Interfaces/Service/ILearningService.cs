using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;

namespace Interfaces.Service
{
    public interface ILearningService
    {
        void RegistrarCorrecao(string textoOriginal, string textoCorrigido, string tipoCorrecao, string sessaoId);
        Task RegistrarCorrecaoAsync(string textoOriginal, string textoCorrigido, string tipoCorrecao, string sessaoId, string usuarioId = null, string empresaId = null);
        string AplicarCorrecoesAprendidas(string texto, string tipoCorrecao, string usuarioId = null, string empresaId = null);
        List<CorrecaoAprendida> ObterCorrecoesAprendidas(string tipoCorrecao = null);
        List<CorrecaoAprendida> ObterTodasCorrecoesAprendidas(string tipoCorrecao = null, string usuarioId = null, string empresaId = null);
        Task<List<CorrecaoAprendida>> ObterTodasCorrecoesAprendidasAsync(string tipoCorrecao = null, string usuarioId = null, string empresaId = null);
        List<CorrecaoAprendida> ObterCorrecoesDesativadas(string tipoCorrecao = null, string usuarioId = null, string empresaId = null);
        Task<List<CorrecaoAprendida>> ObterCorrecoesDesativadasAsync(string tipoCorrecao = null, string usuarioId = null, string empresaId = null);
        void DesativarCorrecao(int id);
        void AtivarCorrecao(int id);
        void RemoverCorrecao(int id);
        List<HistoricoCorrecoes> ObterHistorico(int limite = 50);
        Dictionary<string, int> ObterEstatisticas();
        public void LimparCorrecoesInvalidas();
        public void LimparCorrecoesProblematicas();
    }
}











