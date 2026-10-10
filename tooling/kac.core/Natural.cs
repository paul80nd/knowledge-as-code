namespace kac.core;

/// <summary>
/// Alphabetical as a reader means it, for <c>list-order</c>. Digit runs compare as numbers, so <c>A.8.7</c> precedes
/// <c>A.8.29</c>. Letters compare case-insensitively, with an ordinal tie-break so the order stays total.
/// </summary>
public static class Natural
{
    public static int Compare(string a, string b)
    {
        int i = 0, j = 0;
        while (i < a.Length && j < b.Length)
        {
            if (char.IsAsciiDigit(a[i]) && char.IsAsciiDigit(b[j]))
            {
                int si = i, sj = j;
                while (i < a.Length && char.IsAsciiDigit(a[i])) i++;
                while (j < b.Length && char.IsAsciiDigit(b[j])) j++;
                // Leading zeros carry no magnitude, so trim them and let the digit count decide first.
                var na = a.AsSpan(si, i - si).TrimStart('0');
                var nb = b.AsSpan(sj, j - sj).TrimStart('0');
                if (na.Length != nb.Length) return na.Length - nb.Length;
                var digits = na.SequenceCompareTo(nb);
                if (digits != 0) return digits;
                continue;
            }

            var ca = char.ToLowerInvariant(a[i]);
            var cb = char.ToLowerInvariant(b[j]);
            if (ca != cb) return ca - cb;
            i++;
            j++;
        }

        if (i < a.Length) return 1;
        if (j < b.Length) return -1;
        // Equal but for case or leading zeros.
        return string.CompareOrdinal(a, b);
    }
}
