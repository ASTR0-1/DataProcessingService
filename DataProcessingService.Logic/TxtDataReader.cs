using System.Globalization;
using System.Text.RegularExpressions;
using DataProcessingService.Contracts;
using DataProcessingService.Entities;

namespace DataProcessingService.Logic;

public class TxtDataReader : IDataReader
{
    public async Task<ParseOutput> ParsePaymentTransactionsAsync(string path)
    {
        List<PaymentTransaction> transactions = new List<PaymentTransaction>();
        int parsedLines = 0;
        int parsedErrors = 0;

        // Depending on this tests: https://cc.davelozinski.com/c-sharp/fastest-way-to-read-text-files
        // It is the fastest way to read files
        using StreamReader reader = File.OpenText(path);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            try
            {
                var regex = new Regex("“(.*?)”");
                var match = regex.Match(line);

                string parsedAddress = match.Success
                    ? match.Groups[1].Value
                    : throw new("Error parsing address");

                string city = parsedAddress.Split(',')[0];

                line = line.Replace($"“{parsedAddress}”,", "");
                string[] values = line
                    .Replace(" ", "")
                    .Split(',');

                string firstName = values[(int)ParseFormat.FirstName];
                string lastName = values[(int)ParseFormat.LastName];
                decimal payment = decimal.Parse(values[(int)ParseFormat.Payment], CultureInfo.InvariantCulture);
                DateOnly date = DateOnly.ParseExact(values[(int)ParseFormat.Date], "yyyy-dd-MM",
                    CultureInfo.InvariantCulture);
                string service = values[(int)ParseFormat.Service];

                match = regex.Match(line);
                string quotedAccountNumber = match.Success
                    ? match.Groups[1].Value
                    : "";
                long accountNumber = (!string.IsNullOrEmpty(quotedAccountNumber))
                    ? long.Parse(quotedAccountNumber, CultureInfo.InvariantCulture)
                    : long.Parse(values[(int)ParseFormat.AccountNumber], CultureInfo.InvariantCulture);

                var searchedCityTransaction = transactions
                    .FirstOrDefault(t => t.City == city);
                var searchedService = searchedCityTransaction?.Services?
                    .FirstOrDefault(s => s.Name == service);

                if (searchedService != null)
                {
                    searchedService.Payers.Add(new Payer
                    {
                        Name = string.Concat(firstName, " ", lastName),
                        Payment = payment,
                        Date = date,
                        AccountNumber = accountNumber
                    });

                    searchedService.Total += payment;
                    searchedCityTransaction.Total += payment;
                }
                else if (searchedCityTransaction != null)
                {
                    searchedCityTransaction.Services.Add(new Service
                    {
                        Name = service,
                        Payers = new List<Payer>
                        {
                            new Payer
                            {
                                Name = string.Concat(firstName, " ", lastName),
                                Payment = payment,
                                Date = date,
                                AccountNumber = accountNumber
                            }
                        },
                        Total = payment
                    });

                    searchedCityTransaction.Total += payment;
                }
                else
                {
                    transactions.Add(new PaymentTransaction
                    {
                        City = city,
                        Services = new List<Service>
                        {
                            new Service
                            {
                                Name = service,
                                Total = payment,
                                Payers = new List<Payer>
                                {
                                    new Payer
                                    {
                                        Name = string.Concat(firstName, " ", lastName),
                                        Payment = payment,
                                        Date = date,
                                        AccountNumber = accountNumber
                                    }
                                }
                            }
                        },
                        Total = payment
                    });
                }

                parsedLines++;
            }
            catch
            {
                parsedErrors++;
            }
        }

        return new ParseOutput(transactions, parsedLines,
            parsedErrors, path, parsedErrors < 0);
    }
}