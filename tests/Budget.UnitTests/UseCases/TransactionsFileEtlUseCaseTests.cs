using System.Collections;
using Budget.Application.UseCases.TransactionsFileEtl;
using Budget.Domain.Contracts;
using Budget.Domain.Entities;
using Budget.Domain.Repositories;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Budget.UnitTests.UseCases;

public class TransactionsFileEtlUseCaseTests
{
    [Fact]
    public async Task HandleAsync_SingleTransaction_Success()
    {
        // Arrange
        var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteLineAsync(
            "\"IBAN/BBAN\",\"Munt\",\"BIC\",\"Volgnr\",\"Datum\",\"Rentedatum\",\"Bedrag\",\"Saldo na trn\",\"Tegenrekening IBAN/BBAN\",\"Naam tegenpartij\",\"Naam uiteindelijke partij\",\"Naam initi\ufffdrende partij\",\"BIC tegenpartij\",\"Code\",\"Batch ID\",\"Transactiereferentie\",Machtigingskenmerk,\"Incassant ID\",\"Betalingskenmerk\",\"Omschrijving-1\",\"Omschrijving-2\",\"Omschrijving-3\",\"Reden retour\",\"Oorspr bedrag\",\"Oorspr munt\",\"Koers\"");
        await writer.WriteLineAsync(
            "\"NL11RABO0104946666\",\"EUR\",\"RABONL2U\",\"000000000000012107\",\"2023-11-20\",\"2023-11-20\",\"+4000,00\",\"+4000,00\",\"NL11INGB00022222\",\"Werkgever 1\",,,\"INGBNL2A\",\"cb\",,\"COAXX024818544202311151030147423687\",,,,\"Salaris 1\",\" \",,,,,\n");
        await writer.FlushAsync();
        stream.Position = 0;

        IEnumerable<Transaction> transactions = [];
        var repo = Substitute.For<ITransactionRepository>();
        repo.GetIdsBetweenAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>())
            .Returns(new List<TransactionIdDto>());
        repo.When(r => r.AddRangeAsync(Arg.Any<IEnumerable<Transaction>>()))
            .Do(t => transactions = t.Arg<IEnumerable<Transaction>>());
        var logger = Substitute.For<ILogger<TransactionsFileEtlUseCase>>();

        var useCase = new TransactionsFileEtlUseCase(repo, logger);

        // Act
        var result = await useCase.HandleAsync(stream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(transactions);
        var transaction = transactions.FirstOrDefault();
        Assert.NotNull(transaction);
        Assert.Equal(12107, transaction.FollowNumber);
        Assert.Equal("NL11RABO0104946666", transaction.Iban);
        Assert.Equal(new DateOnly(2023, 11, 20), transaction.DateTransaction);
        Assert.Equal("EUR", transaction.Currency);
        Assert.Equal(4000, transaction.Amount);
        Assert.Equal(4000, transaction.BalanceAfterTransaction);
        Assert.Equal("NL11INGB00022222", transaction.IbanOtherParty);
        Assert.Equal("Werkgever 1", transaction.NameOtherParty);
        Assert.NotNull(transaction.AuthorizationCode);
        Assert.Empty(transaction.AuthorizationCode);
        Assert.Equal("Salaris 1", transaction.Description);
    }

    [Fact]
    public async Task HandleAsync_MultipleTransactions_Success()
    {
        // Arrange
        var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteLineAsync(
            "\"IBAN/BBAN\",\"Munt\",\"BIC\",\"Volgnr\",\"Datum\",\"Rentedatum\",\"Bedrag\",\"Saldo na trn\",\"Tegenrekening IBAN/BBAN\",\"Naam tegenpartij\",\"Naam uiteindelijke partij\",\"Naam initi\ufffdrende partij\",\"BIC tegenpartij\",\"Code\",\"Batch ID\",\"Transactiereferentie\",Machtigingskenmerk,\"Incassant ID\",\"Betalingskenmerk\",\"Omschrijving-1\",\"Omschrijving-2\",\"Omschrijving-3\",\"Reden retour\",\"Oorspr bedrag\",\"Oorspr munt\",\"Koers\"");
        await writer.WriteLineAsync(
            "\"NL11RABO0104946666\",\"EUR\",\"RABONL2U\",\"000000000000012107\",\"2023-11-20\",\"2023-11-20\",\"+4000,00\",\"+4000,00\",\"NL11INGB00022222\",\"Werkgever 1\",,,\"INGBNL2A\",\"cb\",,\"COAXX024818544202311151030147423687\",,,,\"Salaris 1\",\" \",,,,,\n");
        await writer.WriteLineAsync(
            "\"NL11RABO0104946666\",\"EUR\",\"RABONL2U\",\"000000000000012108\",\"2023-11-21\",\"2023-11-21\",\"-2000,00\",\"+4000,00\",\"NL11INGB00033333\",\"Werkgever 2\",,,\"INGBNL2A\",\"cb\",,\"COAXX024818544202311151030147423688\",,,,\"Salaris 2\",\" \",,,,,\n");
        await writer.FlushAsync();
        stream.Position = 0;

        IEnumerable<Transaction> transactions = [];
        var repo = Substitute.For<ITransactionRepository>();
        repo.GetIdsBetweenAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>())
            .Returns(new List<TransactionIdDto>());
        repo.When(r => r.AddRangeAsync(Arg.Any<IEnumerable<Transaction>>()))
            .Do(t => transactions = t.Arg<IEnumerable<Transaction>>());
        var logger = Substitute.For<ILogger<TransactionsFileEtlUseCase>>();

        var useCase = new TransactionsFileEtlUseCase(repo, logger);

        // Act
        var result = await useCase.HandleAsync(stream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, transactions.Count());
        var transaction1 = transactions.FirstOrDefault(t => t.FollowNumber == 12107);
        var transaction2 = transactions.FirstOrDefault(t => t.FollowNumber == 12108);
        Assert.NotNull(transaction1);
        Assert.NotNull(transaction2);
        Assert.Equal("NL11RABO0104946666", transaction1.Iban);
        Assert.Equal(new DateOnly(2023, 11, 20), transaction1.DateTransaction);
        Assert.Equal("EUR", transaction1.Currency);
        Assert.Equal(4000, transaction1.Amount);
        Assert.Equal(4000, transaction1.BalanceAfterTransaction);
        Assert.Equal("NL11INGB00022222", transaction1.IbanOtherParty);
        Assert.Equal("Werkgever 1", transaction1.NameOtherParty);
        Assert.NotNull(transaction1.AuthorizationCode);
        Assert.Empty(transaction1.AuthorizationCode);
        Assert.Equal("Salaris 1", transaction1.Description);

        Assert.Equal("NL11RABO0104946666", transaction2.Iban);
        Assert.Equal(new DateOnly(2023, 11, 21), transaction2.DateTransaction);
        Assert.Equal("EUR", transaction2.Currency);
        Assert.Equal(-2000, transaction2.Amount);
        Assert.Equal(4000, transaction2.BalanceAfterTransaction);
        Assert.Equal("NL11INGB00033333", transaction2.IbanOtherParty);
        Assert.Equal("Werkgever 2", transaction2.NameOtherParty);
        Assert.NotNull(transaction2.AuthorizationCode);
        Assert.Empty(transaction2.AuthorizationCode);
        Assert.Equal("Salaris 2", transaction2.Description);
    }

    [Fact]
    public async Task HandleAsync_SkipExistingTransaction_InsertNewTransaction_Success()
    {
        // Arrange
        var stream = new MemoryStream();
        await using var writer = new StreamWriter(stream);
        await writer.WriteLineAsync(
            "\"IBAN/BBAN\",\"Munt\",\"BIC\",\"Volgnr\",\"Datum\",\"Rentedatum\",\"Bedrag\",\"Saldo na trn\",\"Tegenrekening IBAN/BBAN\",\"Naam tegenpartij\",\"Naam uiteindelijke partij\",\"Naam initi\ufffdrende partij\",\"BIC tegenpartij\",\"Code\",\"Batch ID\",\"Transactiereferentie\",Machtigingskenmerk,\"Incassant ID\",\"Betalingskenmerk\",\"Omschrijving-1\",\"Omschrijving-2\",\"Omschrijving-3\",\"Reden retour\",\"Oorspr bedrag\",\"Oorspr munt\",\"Koers\"");
        await writer.WriteLineAsync(
            "\"NL11RABO0104946666\",\"EUR\",\"RABONL2U\",\"000000000000012107\",\"2023-11-20\",\"2023-11-20\",\"+4000,00\",\"+4000,00\",\"NL11INGB00022222\",\"Werkgever 1\",,,\"INGBNL2A\",\"cb\",,\"COAXX024818544202311151030147423687\",,,,\"Salaris 1\",\" \",,,,,\n");
        await writer.WriteLineAsync(
            "\"NL11RABO0104946666\",\"EUR\",\"RABONL2U\",\"000000000000012108\",\"2023-11-21\",\"2023-11-21\",\"-2000,00\",\"+2000,00\",\"NL11INGB00033333\",\"Werkgever 2\",,,\"INGBNL2A\",\"cb\",,\"COAXX024818544202311151030147423688\",,,,\"Salaris 2\",\" \",,,,,\n");
        await writer.FlushAsync();
        stream.Position = 0;

        IEnumerable<Transaction> transactions = [];
        var repo = Substitute.For<ITransactionRepository>();
        repo.GetIdsBetweenAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>())
            .Returns(new List<TransactionIdDto>
            {
                new TransactionIdDto { Id= 1, FollowNumber = 12107, Iban = "NL11RABO0104946666" }
            });
        repo.When(r => r.AddRangeAsync(Arg.Any<IEnumerable<Transaction>>()))
            .Do(t => transactions = t.Arg<IEnumerable<Transaction>>());
        var logger = Substitute.For<ILogger<TransactionsFileEtlUseCase>>();

        var useCase = new TransactionsFileEtlUseCase(repo, logger);

        // Act
        var result = await useCase.HandleAsync(stream);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(transactions);
        var transaction = transactions.FirstOrDefault();
        Assert.NotNull(transaction);
        Assert.Equal(12108, transaction.FollowNumber);
        Assert.Equal("NL11RABO0104946666", transaction.Iban);
        Assert.Equal(new DateOnly(2023, 11, 21), transaction.DateTransaction);
        Assert.Equal("EUR", transaction.Currency);
        Assert.Equal(-2000, transaction.Amount);
        Assert.Equal(2000, transaction.BalanceAfterTransaction);
        Assert.Equal("NL11INGB00033333", transaction.IbanOtherParty);
        Assert.Equal("Werkgever 2", transaction.NameOtherParty);
        Assert.NotNull(transaction.AuthorizationCode);
        Assert.Empty(transaction.AuthorizationCode);
        Assert.Equal("Salaris 2", transaction.Description);
    }
}