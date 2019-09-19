namespace LostTech.TextProcessing {
    using System;
    using System.Collections.Generic;

    public struct TokenPair<T>: IEquatable<TokenPair<T>>
        where T: IEquatable<T>
    {
        public T Token1 { get; }
        public T Token2 { get; }

        public TokenPair(T token1, T token2) {
            this.Token1 = token1;
            this.Token2 = token2;
        }

        public bool Equals(TokenPair<T> other)
            => EqualityComparer<T>.Default.Equals(this.Token1, other.Token1)
               && EqualityComparer<T>.Default.Equals(this.Token2, other.Token2);

        public override bool Equals(object obj)
            => obj is TokenPair<T> other && this.Equals(other);

        public override int GetHashCode() {
            unchecked {
                return (EqualityComparer<T>.Default.GetHashCode(this.Token1) * 397) ^ EqualityComparer<T>.Default.GetHashCode(this.Token2);
            }
        }

        public void Deconstruct(out T token1, out T token2) {
            token1 = this.Token1;
            token2 = this.Token2;
        }
    }

    public static class TokenPair
    {
        public static TokenPair<T> Create<T>(T token1, T token2) where T : IEquatable<T>
            => new TokenPair<T>(token1, token2);
    }
}
