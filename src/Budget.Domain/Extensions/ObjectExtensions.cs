using System.Text.Json;
using System.Text.Json.Serialization;

namespace Budget.Domain.Extensions;

public static class ObjectExtensions
{
    public static string Dump(this object obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });
    }
}