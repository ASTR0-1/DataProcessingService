namespace DataProcessingService.Entities;

public class ParseOutput
{
    public List<PaymentTransaction> Payments { get; } = new List<PaymentTransaction>();

    public int ParsedLines { get; }

	public int ParsedErrors { get; }

	public string FilePath { get; }

    public bool IsValid { get; }

	public ParseOutput(List<PaymentTransaction> payments, int parsedLines, int parsedErrors, string filePath, bool isValid)
    {
        Payments = payments;
        ParsedLines = parsedLines;
        ParsedErrors = parsedErrors;
        FilePath = filePath;
        IsValid = isValid;
    }
}
