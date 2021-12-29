namespace TradeBot;

public class GeneralUtils
{
    
}

internal static class ExtensionMethodes
{
    /// <summary>
    /// Helper methods for the lists.
    /// </summary>
    public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize) 
    {
        return source
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }
}