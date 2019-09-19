namespace LostTech.TextProcessing {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using MoreLinq;

    public static class BytePairEncodingTraining<T> where T: struct, IChar<T>, IEquatable<T>
    {
        public static void Learn(IReadOnlyDictionary<T, long> vocabulary, T endOfWord) {
            throw new NotImplementedException();

            // vocab = dict([(tuple(x[:-1])+(x[-1]+'</w>',) ,y) for (x,y) in vocab.items()])
            // vocabulary = new ReadOnlyDictionary<T, long>(vocabulary.ToDictionary(
            //    keySelector: token => (T)token.Key.Add(endOfWord),
            //    elementSelector: token => token.Value));
        }
        [Obsolete]
        public static IEnumerable<TokenPair<T>> Learn(IReadOnlyDictionary<BytePairToken<T>, long> vocabulary,
                                 int numSymbols, int minFrequency) {
            if (vocabulary == null) throw new ArgumentNullException(nameof(vocabulary));

            var sorted = vocabulary.OrderByDescending(token => token.Value).ToArray();

            var stats = GetPairStatistics(sorted, out var indices);
            var bigStats = Copy(stats);
            double threshold = stats.Values.Max() / 10.0;
            for (int symbolIndex = 0; symbolIndex < numSymbols; symbolIndex++) {
                TokenPair<T> mostFrequent = default;
                if (stats.Count != 0) {
                    mostFrequent = MostFrequent(stats);
                }

                if (stats.Count == 0 || (symbolIndex > 0 && stats[mostFrequent] < threshold)) {
                    PruneStats(stats, bigStats, threshold);
                    stats = Copy(bigStats);
                    mostFrequent = MostFrequent(stats);
                    threshold = checked(stats[mostFrequent] * symbolIndex) / (symbolIndex + 10000.0);
                    PruneStats(stats, bigStats, threshold);
                }

                if (stats[mostFrequent] < minFrequency)
                    throw new ArgumentException("Inconsistent input: no pair has required frequency");

                yield return mostFrequent;

                var changes = ReplacePair(mostFrequent, sorted, indices);
                UpdatePairStatistics(mostFrequent, changes, stats, indices);
                stats[mostFrequent] = 0;
                if (symbolIndex % 100 == 99)
                    PruneStats(stats, bigStats, threshold);
            }
        }

        static void UpdatePairStatistics(TokenPair<T> pair, IEnumerable<ValueTuple<int,T,T,int>> changed, Dictionary<TokenPair<T>, long> stats, Dictionary<TokenPair<T>, Dictionary<int, long>> indices) {
            stats[pair] = 0;
            indices[pair] = new Dictionary<int, long>();
            var (first, second) = pair;
            var newPair = first.Append(second);

            foreach (var (j, word, oldWord, freq) in changed) {
                int i = 0;
                while (true) {
                    i = oldWord.IndexOf(first, startIndex: i);
                    if (i < 0) break;

                    if (i < oldWord.Count - 1 && oldWord[i + 1].Equals(second)) {
                        if (i > 0) {
                            var prev = oldWord.GetPair(i-1);
                            stats[prev] -= freq;
                            indices[prev][j]--;
                        }

                        if (i < oldWord.Count - 2) {
//assuming a symbol sequence "A B C B", if "B C" is merged, reduce the frequency of "C B".
//however, skip this if the sequence is A B C B C, because the frequency of "C B" will be reduced by the previous code block
                            if (!oldWord[i + 2].Equals(first) || i >= oldWord.Count - 3
                                                        || !oldWord[i + 3].Equals(second)) {
                                var nex = oldWord.GetPair(i+1);
                                stats[nex] -= freq;
                                indices[nex][j]--;
                            }
                        }

                        i += 2;
                    } else {
                        i++;
                    }
                }

                i = 0;
                while (true) {
                    i = word.IndexOf(newPair, i);
                    if (i < 0) break;

                    if (i > 0) {
                        var prev = word.GetPair(i-1);
                        stats[prev] += freq;
                        indices[prev][j]++;
                    }

                    if (i < word.Count - 1 && !word[i + 1].Equals(newPair)) {
                        var nex = word.GetPair(i);
                        stats[nex] += freq;
                        indices[nex][j]++;
                    }

                    i++;
                }
            }
        }

        static IEnumerable<ValueTuple<int, T, T, int>> ReplacePair(TokenPair<T> pair,
            KeyValuePair<BytePairToken<T>, long>[] sorted,
            Dictionary<TokenPair<T>, Dictionary<int, long>> indices) {
            var (first, second) = pair;
            throw new NotImplementedException();
        }

        private static void PruneStats(Dictionary<TokenPair<T>, long> stats,
            Dictionary<TokenPair<T>, long> bigStats, double threshold) {
            foreach (var item in stats.ToArray()) {
                long freq = item.Value;
                if (freq < threshold) {
                    stats.Remove(item.Key);

                    if (freq < 0) {
                        bigStats.TryGetValue(item.Key, out long oldFreq);
                        bigStats[item.Key] = oldFreq + freq;
                    } else {
                        bigStats[item.Key] = freq;
                    }
                }
            }
        }
        static TKey MostFrequent<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> items)
            // TODO probably different semantics in Python
            => items.MaxBy(selector: pair => pair.Value).First().Key;

        static Dictionary<TKey, TValue> Copy<TKey, TValue>(IDictionary<TKey, TValue> dict)
            => dict.ToDictionary(entry => entry.Key, entry => entry.Value);

        static Dictionary<TokenPair<T>, long> GetPairStatistics<TList>(KeyValuePair<TList, long>[] sortedWordCounts,
            out Dictionary<TokenPair<T>, Dictionary<int, long>> indices)
            where TList: IList<T>
        {
            var stats = new Dictionary<TokenPair<T>, long>();
            indices = new Dictionary<TokenPair<T>, Dictionary<int,long>>();

            for (int i = 0; i < sortedWordCounts.Length; i++) {
                var word = sortedWordCounts[i].Key;
                var prevChar = word[0];
                foreach (var @char in word.Skip(1)) {
                    var pair = TokenPair.Create(prevChar, @char);
                    stats.TryGetValue(pair, out long freq);
                    stats[pair] = freq + sortedWordCounts[i].Value;
                    if (indices.TryGetValue(pair, out var index)) {
                        index.TryGetValue(i, out long indexCounter);
                        index[i] = indexCounter + 1;
                    } else {
                        indices[pair] = new Dictionary<int, long> {[i] = 1};
                    }

                    prevChar = @char;
                }
            }

            return stats;
        }
    }
}

// adopted from https://github.com/rsennrich/subword-nmt/blob/master/subword_nmt/learn_bpe.py
