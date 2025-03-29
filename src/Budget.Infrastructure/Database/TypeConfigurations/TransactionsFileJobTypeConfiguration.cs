using Budget.Domain;
using Budget.Domain.Entities;
using Budget.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Budget.Infrastructure.Database.TypeConfigurations;

public class TransactionsFileJobTypeConfiguration : IEntityTypeConfiguration<TransactionsFileJob>
{
    public void Configure(EntityTypeBuilder<TransactionsFileJob> builder)
    {
        builder.Property(j => j.Status)
            .HasConversion(new EnumToStringConverter<JobStatus>());
    }
}