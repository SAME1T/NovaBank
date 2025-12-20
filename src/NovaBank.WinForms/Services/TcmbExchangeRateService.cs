using System.Globalization;
using System.Net.Http;
using System.Xml.Linq;
using NovaBank.WinForms.Dto;

namespace NovaBank.WinForms.Services;

public class TcmbExchangeRateService
{
    private readonly HttpClient _httpClient;
    private const string TcmbUrl = "https://www.tcmb.gov.tr/kurlar/today.xml";

    public TcmbExchangeRateService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<(DateTime date, List<DovizKurDto> rates)> GetTodayAsync(CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(TcmbUrl, ct);
            var doc = XDocument.Parse(response);
            
            // Tarih parse et
            var root = doc.Root;
            if (root == null)
                return (DateTime.Today, new List<DovizKurDto>());

            var tarihAttr = root.Attribute("Tarih");
            DateTime date = DateTime.Today;
            
            if (tarihAttr != null && !string.IsNullOrWhiteSpace(tarihAttr.Value))
            {
                // dd.MM.yyyy formatından parse et
                if (DateTime.TryParseExact(tarihAttr.Value, "dd.MM.yyyy", 
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                {
                    date = parsedDate;
                }
            }

            var rates = new List<DovizKurDto>();
            
            // Currency elementlerini parse et
            var currencyElements = root.Elements("Currency");
            foreach (var currency in currencyElements)
            {
                var codeAttr = currency.Attribute("CurrencyCode");
                if (codeAttr == null) continue;

                var dto = new DovizKurDto
                {
                    CurrencyCode = codeAttr.Value
                };

                // CurrencyName
                var nameEl = currency.Element("CurrencyName");
                if (nameEl != null)
                    dto.CurrencyName = nameEl.Value;

                // Unit
                var unitEl = currency.Element("Unit");
                if (unitEl != null && int.TryParse(unitEl.Value, out var unit))
                    dto.Unit = unit;

                // ForexBuying
                var forexBuyEl = currency.Element("ForexBuying");
                if (forexBuyEl != null && !string.IsNullOrWhiteSpace(forexBuyEl.Value))
                {
                    dto.ForexBuying = TryParseDecimal(forexBuyEl.Value);
                }

                // ForexSelling
                var forexSellEl = currency.Element("ForexSelling");
                if (forexSellEl != null && !string.IsNullOrWhiteSpace(forexSellEl.Value))
                {
                    dto.ForexSelling = TryParseDecimal(forexSellEl.Value);
                }

                // BanknoteBuying
                var banknoteBuyEl = currency.Element("BanknoteBuying");
                if (banknoteBuyEl != null && !string.IsNullOrWhiteSpace(banknoteBuyEl.Value))
                {
                    dto.BanknoteBuying = TryParseDecimal(banknoteBuyEl.Value);
                }

                // BanknoteSelling
                var banknoteSellEl = currency.Element("BanknoteSelling");
                if (banknoteSellEl != null && !string.IsNullOrWhiteSpace(banknoteSellEl.Value))
                {
                    dto.BanknoteSelling = TryParseDecimal(banknoteSellEl.Value);
                }

                rates.Add(dto);
            }

            return (date, rates);
        }
        catch (Exception ex)
        {
            // Hata durumunda boş liste döndür, üst katmanda gösterilecek
            System.Diagnostics.Debug.WriteLine($"TCMB kur çekme hatası: {ex.Message}");
            throw; // Hata mesajını üst katmana ilet
        }
    }

    private static decimal? TryParseDecimal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        // Önce InvariantCulture ile dene
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        // Sonra tr-TR ile dene
        if (decimal.TryParse(value, NumberStyles.Any, new CultureInfo("tr-TR"), out result))
            return result;

        return null;
    }
}

