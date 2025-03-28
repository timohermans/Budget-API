namespace Budget.Application.Settings;

public class FileStorageSettings
{
    public long MaxSizeMb { get; set; }
    public string? BasePath { get; set; }

    public bool IsValid => MaxSizeMb > 0 && !string.IsNullOrEmpty(BasePath);
}