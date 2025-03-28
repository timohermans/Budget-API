using CsvHelper.Configuration.Attributes;

namespace Budget.Application.UseCases.TransactionsFileEtl;

public class TransactionsFileCsvMap
{
    [Name("IBAN/BBAN")]
    public string? Iban { get; set; }
    [Name("Munt")]
    public string? Currency { get; set; }
    [Name("Volgnr")]
    public int FollowNumber { get; set; }
    [Name("Datum")]
    public DateOnly Date { get; set; }
    [Name("Bedrag")]
    public decimal? Amount { get; set; }
    [Name("Saldo na trn")]
    public decimal? BalanceAfter { get; set; }
    [Name("Tegenrekening IBAN/BBAN")]
    public string? IbanOtherParty { get; set; }
    [Name("Naam tegenpartij")]
    public string? NameOtherParty { get; set; }
    [Name("Machtigingskenmerk")]
    public string? AuthorizationCode { get; set; }
    [Name("Omschrijving-1")]
    public string? Description1 { get; set; }
    [Name("Omschrijving-2")]
    public string? Description2 { get; set; }
    [Name("Omschrijving-3")]
    public string? Description3 { get; set; }
}