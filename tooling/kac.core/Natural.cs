namespace kac.core;

// Alphabetical as a reader means it, used by the `list-order` check. Digit runs compare as numbers,
// so `A.8.7` precedes `A.8.29` and `7` precedes `108`, where a byte-wise comparison inverts both.
// Letters compare case-insensitively, with an ordinal tie-break so the order stays total — two
// entries differing only in case still have a defined winner rather than being read as equal.
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
        return string.CompareOrdinal(a, b); // equal but for case or leading zeros
    }
}
