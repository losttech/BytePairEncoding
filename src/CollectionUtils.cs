namespace LostTech.TextProcessing {
    using System;
    using System.Collections.Generic;

    static class CollectionUtils {
        public static int HashSequence<T>(this IEnumerable<T> sequence, ref int seed)
            where T: struct
        {
            if (sequence == null) throw new ArgumentNullException(nameof(sequence));

            int result = 0;
            foreach (T item in sequence) {
                result ^= unchecked(item.GetHashCode() * seed);
                seed = unchecked(seed * 17);
            }

            return result;
        }
    }
}
