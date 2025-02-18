using Budget.Domain.Entities;
using Budget.Domain.Enums;

namespace Budget.Domain;

public class TransactionsFileJob
{
    public Guid Id { get; set; }
    public required string OriginalFileName { get; set; }
    public required string StoredFilePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending; // e.g., "Pending", "Processing", "Completed", "Failed"
    public string? ErrorMessage { get; set; }
}