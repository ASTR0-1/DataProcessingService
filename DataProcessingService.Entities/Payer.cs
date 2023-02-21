using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataProcessingService.Entities;

public class Payer
{
    public string? Name { get; set; }

    public decimal? Payment { get; set; }

    public DateOnly? Date { get; set; }

    [JsonPropertyName("account_number")]
    public long? AccountNumber { get; set; }
}
