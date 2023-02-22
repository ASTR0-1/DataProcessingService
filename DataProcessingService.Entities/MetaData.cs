using System.Text.Json.Serialization;

namespace DataProcessingService.Entities;

public class MetaData
{
    [JsonPropertyName("parsed_files")]
    public int ParsedFiles { get; set; }

    [JsonPropertyName("parsed_lines")]
    public int ParsedLines { get; set; }

    [JsonPropertyName("found_errors")]
    public int ParsedErrors { get; set; }

    [JsonPropertyName("invalid_files")]
    public List<string>? InvalidFilePaths { get; set; }
}
