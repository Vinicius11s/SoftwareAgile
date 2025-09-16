using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Domain.DTOs;
using Domain.Entities;
using Interfaces.Service;

namespace Services
{
    public class CsvServices : ICsvService
    {
        public List<OfertaDTO> LerOfertas(Stream stream)
        {
            var config = new CsvConfiguration(new CultureInfo("pt-BR"))
            {
                Delimiter = ";",
                HasHeaderRecord = false, // porque o CSV não tem cabeçalho
                BadDataFound = null
            };

            using var reader = new StreamReader(stream);
            using var csv = new CsvReader(reader, config);

            var ofertas = new List<OfertaDTO>();
            while (csv.Read())
            {
                var descricao = csv.GetField(0);
                var preco = csv.GetField<decimal>(1);

                ofertas.Add(new OfertaDTO
                {
                    Descricao = descricao,
                    Preco = preco
                });
            }

            return ofertas;
        }
    }
}

