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
        string AplicarCorrecoesAprendidas(string texto, string tipoCorrecao);
        List<CorrecaoAprendida> ObterCorrecoesAprendidas(string tipoCorrecao = null);
        void DesativarCorrecao(int id);
        void AtivarCorrecao(int id);
        void RemoverCorrecao(int id);
        List<HistoricoCorrecoes> ObterHistorico(int limite = 50);
        Dictionary<string, int> ObterEstatisticas();
        public void LimparCorrecoesInvalidas();
        public void LimparCorrecoesProblematicas();
    }
}

