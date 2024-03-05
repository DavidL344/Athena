namespace Athena.Core.Model.DataStructures;

public static class UniqueListExtensions
{
    public static UniqueList<TSource> ToUniqueList<TSource>(this IEnumerable<TSource> source)
    {
        return new UniqueList<TSource>(source);
    }
}
