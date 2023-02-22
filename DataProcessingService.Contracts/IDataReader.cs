using DataProcessingService.Entities;

namespace DataProcessingService.Contracts;

public interface IDataReader
{
    Task<ParseOutput> ParsePaymentTransactionsAsync(string path);
}
