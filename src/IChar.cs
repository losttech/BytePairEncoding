namespace LostTech.TextProcessing {
    using System;

    public interface IChar<T> where T: IEquatable<T>
    {
        T Append(T other);
        int IndexOf(T other, int startIndex);
        TokenPair<T> GetPair(int index);
        T this[int index] { get; }
        int Count { get; }
    }
}
