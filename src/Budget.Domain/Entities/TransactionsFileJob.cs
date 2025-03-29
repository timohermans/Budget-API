using Budget.Domain.Enums;

namespace Budget.Domain.Entities;

public class TransactionsFileJob
{
    public Guid Id { get; set; }
    public required string OriginalFileName { get; set; }
    public required byte[] FileContent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Pending; // e.g., "Pending", "Processing", "Completed", "Failed"
    public string? ErrorMessage { get; set; }
}