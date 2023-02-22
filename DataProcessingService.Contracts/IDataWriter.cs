using DataProcessingService.Entities;

namespace DataProcessingService.Contracts;

public interface IDataWriter
{
    Task WriteDataAsync(ParseOutput parse, string outputPath, int fileNumber);
}
