namespace DataProcessingService.Entities;

public class ParseOutput
{
    public List<PaymentTransaction> Payments { get; }

    public List<int> ParsedLines { get; }

	public List<int> ParsedErrors { get; }

	public string FilePath { get; }

	public ParseOutput(List<PaymentTransaction> payments, List<int> parsedLines, List<int> parsedErrors, string filePath)
    {
        Payments = payments;
        ParsedLines = parsedLines;
        ParsedErrors = parsedErrors;
        FilePath = filePath;
    }
}
