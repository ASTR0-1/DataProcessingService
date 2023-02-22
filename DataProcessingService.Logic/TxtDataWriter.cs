using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper.TypeConversion;
using DataProcessingService.Contracts;
using DataProcessingService.Entities;
using Microsoft.VisualBasic;

namespace DataProcessingService.Logic;

public class TxtDataWriter : IDataWriter
{
    public async Task WriteDataAsync(ParseOutput parse, string outputPath, int fileNumber)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new DateOnlyConverter());
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.WriteIndented = true;

        string jsonString = JsonSerializer.Serialize(parse.Payments, options);

        using var writer = new StreamWriter(outputPath + $"/output{fileNumber}.json");

        await writer.WriteAsync(jsonString);
    }
}
