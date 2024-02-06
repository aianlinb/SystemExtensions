﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace SystemExtensions.Collections {
	public static class CollectionExtensions {
		public static int IndexOf<T>(this IEnumerable<T> source, T value) {
			switch (source) {
				case IList<T> listt: // includes array & List<T>
					return listt.IndexOf(value);
				case IList list:
					return list.IndexOf(value);
				default:
					var i = 0;
					if (value is not null)
						foreach (var item in source) {
							if (EqualityComparer<T>.Default.Equals(item, value))
								return i;
							++i;
						}
					else
						foreach (var item in source) {
							if (item is null)
								return i;
							++i;
						}
					return -1;
			}
		}

		public static int IndexOf<T>(this IEnumerable<T> source, Predicate<T> match, out T? value) {
			var i = 0;
			foreach (var item in source) {
				if (match(item)) {
					value = item;
					return i;
				}
				++i;
			}
			value = default;
			return -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IReadOnlyList<T> AsIReadOnly<T>(this IList<T> list) {
			return list is IReadOnlyList<T> irl ? irl : new ReadOnlyListWrapper<T>(list);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IReadOnlyCollection<T> AsIReadOnly<T>(this ICollection<T> collection) {
			return collection is IReadOnlyCollection<T> irc ? irc : new ReadOnlyCollectionWrapper<T>(collection);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IReadOnlySet<T> AsIReadOnly<T>(this ISet<T> set) {
			return set is IReadOnlySet<T> irs ? irs : new ReadOnlySetWrapper<T>(set);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IReadOnlyDictionary<TKey, TValue> AsIReadOnly<TKey, TValue>(this IDictionary<TKey, TValue> dict) {
			return dict is IReadOnlyDictionary<TKey, TValue> ird ? ird : new ReadOnlDictionaryWrapper<TKey, TValue>(dict);
		}

		#region Wrappers
		[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
		private sealed class ReadOnlyListWrapper<T>(IList<T> list) : IReadOnlyList<T> {
			public T this[int index] => list[index];
			public int Count => list.Count;
			public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
		private sealed class ReadOnlyCollectionWrapper<T>(ICollection<T> collection) : IReadOnlyCollection<T> {
			public int Count => collection.Count;
			public IEnumerator<T> GetEnumerator() => collection.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
		private sealed class ReadOnlySetWrapper<T>(ISet<T> set) : IReadOnlySet<T> {
			public int Count => set.Count;
			public bool Contains(T item) => set.Contains(item);
			public IEnumerator<T> GetEnumerator() => set.GetEnumerator();
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			public bool IsProperSubsetOf(IEnumerable<T> other) => set.IsProperSubsetOf(other);
			public bool IsProperSupersetOf(IEnumerable<T> other) => set.IsProperSupersetOf(other);
			public bool IsSubsetOf(IEnumerable<T> other) => set.IsSubsetOf(other);
			public bool IsSupersetOf(IEnumerable<T> other) => set.IsSupersetOf(other);
			public bool Overlaps(IEnumerable<T> other) => set.Overlaps(other);
			public bool SetEquals(IEnumerable<T> other) => set.SetEquals(other);
		}

		[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
		private sealed class ReadOnlDictionaryWrapper<TKey, TValue>(IDictionary<TKey, TValue> dict) : IReadOnlyDictionary<TKey, TValue> {
			public TValue this[TKey key] => dict[key];
			public IEnumerable<TKey> Keys => dict.Keys;
			public IEnumerable<TValue> Values => dict.Values;
			public int Count => dict.Count;
			public bool ContainsKey(TKey key) => dict.ContainsKey(key);
			public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dict.GetEnumerator();
			public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => dict.TryGetValue(key, out value);
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
		#endregion Wrappers
	}
}