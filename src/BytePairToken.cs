namespace LostTech.TextProcessing {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class BytePairToken<T>: List<T>, IEquatable<BytePairToken<T>>
        where T: struct, IChar<T>, IEquatable<T>
    {
        public bool Equals(BytePairToken<T> other) => other?.SequenceEqual(this) == true;

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && this.Equals((BytePairToken<T>)obj);
        }

        public override int GetHashCode() {
            int seed = 17;
            return this.HashSequence(ref seed);
        }
    }
}
