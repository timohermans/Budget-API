using Budget.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Budget.Infrastructure.Database.TypeConfigurations;

public class TransactionTypeConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.Property(t => t.Iban).HasMaxLength(34);
        builder.Property(t => t.Currency).HasMaxLength(5);
        builder.Property(t => t.NameOtherParty).HasMaxLength(255);
        builder.Property(t => t.IbanOtherParty).HasMaxLength(34);
        builder.Property(t => t.AuthorizationCode).HasMaxLength(255);
        builder.Property(t => t.Description).HasMaxLength(255);
        builder.Property(t => t.Amount).HasPrecision(12, 2);
        builder.Property(t => t.BalanceAfterTransaction).HasPrecision(12, 2);
        builder.HasIndex(t => new { t.FollowNumber, t.Iban }).IsUnique();
    }
}