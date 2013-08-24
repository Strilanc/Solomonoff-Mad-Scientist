using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

public static class CollectionUtil {
    public static IEnumerable<BigInteger> Naturals() {
        for (var i = BigInteger.Zero; ; i++)
            yield return i;
    }
    public static IEnumerable<BigInteger> Range(this BigInteger length) {
        for (var i = BigInteger.Zero; i < length; i++)
            yield return i;
    }

    public static IEnumerable<T> Prepend<T>(this IEnumerable<T> sequence, T appendedItem) {
        return new[] {appendedItem}.Concat(sequence);
    }
    public static IEnumerable<Tuple<T1, T2>> Cross<T1, T2>(this IEnumerable<T1> sequence1, IEnumerable<T2> sequence2) {
        if (sequence1 == null) throw new ArgumentNullException("sequence1");
        if (sequence2 == null) throw new ArgumentNullException("sequence2");
        return from item1 in sequence1
               from item2 in sequence2
               select Tuple.Create(item1, item2);
    }

    /// <summary>
    /// Enumerates all of the ways that it's possible one item from each collection in a sequence.
    /// For example, the choice combinations of [[1,2],[3,4,5]] are (in some order): {[1,3],[1,4],[1,5],[2,3],[2,4],[2,5]}.
    /// </summary>
    public static IEnumerable<IReadOnlyList<T>> AllChoiceCombinations<T>(this IEnumerable<IEnumerable<T>> sequenceOfChoices) {
        using (var e = sequenceOfChoices.GetEnumerator().AllChoiceCombinationsOfRemainder()) {
            while (e.MoveNext()) {
                yield return e.Current;
            }
        }
    }
    private static IEnumerator<IImmutableList<T>> AllChoiceCombinationsOfRemainder<T>(this IEnumerator<IEnumerable<T>> sequenceOfChoices) {
        if (!sequenceOfChoices.MoveNext()) {
            yield return ImmutableList.Create<T>();
            yield break;
        }

        var headChoices = sequenceOfChoices.Current;
        var tailChoices = sequenceOfChoices.AllChoiceCombinationsOfRemainder();
        using (var e = tailChoices) {
            while (e.MoveNext()) {
                var tailChoice = e.Current;
                foreach (var headChoice in headChoices) {
                    yield return tailChoice.Insert(0, headChoice);
                }
            }
        }
    }
}