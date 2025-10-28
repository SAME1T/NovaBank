using NovaBank.Core.Services;

namespace NovaBank.Infrastructure.Services;

public class IbanGenerator : IIbanGenerator
{
    private static readonly Random _random = new();
    private static readonly string[] _bankCodes = { "0001", "0002", "0003", "0004", "0005" };
    
    public string GenerateIban()
    {
        // TR + 2 digit check + 4 digit bank code + 20 digit account number
        var bankCode = _bankCodes[_random.Next(_bankCodes.Length)];
        var accountNumber = GenerateAccountNumber();
        var checkDigits = CalculateCheckDigits($"TR00{bankCode}{accountNumber}");
        
        return $"TR{checkDigits}{bankCode}{accountNumber}";
    }
    
    private static string GenerateAccountNumber()
    {
        // 20 digit account number
        var accountNumber = "";
        for (int i = 0; i < 20; i++)
        {
            accountNumber += _random.Next(0, 10);
        }
        return accountNumber;
    }
    
    private static string CalculateCheckDigits(string iban)
    {
        // Move first 4 characters to end
        var rearranged = iban.Substring(4) + iban.Substring(0, 4);
        
        // Convert letters to numbers (A=10, B=11, etc.)
        var numericString = "";
        foreach (char c in rearranged)
        {
            if (char.IsLetter(c))
            {
                numericString += (c - 'A' + 10).ToString();
            }
            else
            {
                numericString += c;
            }
        }
        
        // Calculate mod 97
        var remainder = 0;
        foreach (char c in numericString)
        {
            remainder = (remainder * 10 + (c - '0')) % 97;
        }
        
        var checkDigits = (98 - remainder).ToString("D2");
        return checkDigits;
    }
}
