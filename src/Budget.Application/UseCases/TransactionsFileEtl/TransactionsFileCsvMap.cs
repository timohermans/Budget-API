using Budget.Domain.Entities;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace Budget.Application.UseCases.TransactionsFileEtl;

public class TransactionsFileCsvMap
{
    [Index(0)]
    public string? Iban { get; set; }
    [Index(1)]
    public string? Currency { get; set; }
    [Index(3)]
    public int FollowNumber { get; set; }
    [Index(4)]
    public DateTime Date { get; set; }
    [Index(6)]
    public string? Amount { get; set; }
    [Index(7)]
    public string? BalanceAfter { get; set; }
    [Index(8)]
    public string? IbanOtherParty { get; set; }
    [Index(9)]
    public string? NameOtherParty { get; set; }
    [Index(16)]
    public string? AuthorizationCode { get; set; }
    [Index(19)]
    public string? Description1 { get; set; }
    [Index(20)]
    public string? Description2 { get; set; }
    [Index(21)]
    public string? Description3 { get; set; }
}