using CsvHelper;
using CsvHelper.Configuration;
using Interfaces.Models;
using System.Formats.Asn1;
using System.Globalization;


namespace OfertasWeb.Services;


public static class CsvOfertasReader
{
    public static List<OfertaCSV> Ler(Stream csvStream)
    {
        var config = new CsvConfiguration(new CultureInfo("pt-BR"))
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null
        };


        using var reader = new StreamReader(csvStream);
        using var csv = new CsvReader(reader, config);


        var records = new List<OfertaCsv>();
        try
        {
            records = csv.GetRecords<OfertaCsv>().ToList();
        }
        catch
        {
            // Tentativa com ponto decimal (en-US)
            csv.Configuration.CultureInfo = new CultureInfo("en-US");
            csv.Context.Reader.Seek(0, SeekOrigin.Begin);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            records = csv.GetRecords<OfertaCsv>().ToList();
        }


        return records;
    }
}