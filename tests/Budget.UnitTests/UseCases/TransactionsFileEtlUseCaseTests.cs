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
}