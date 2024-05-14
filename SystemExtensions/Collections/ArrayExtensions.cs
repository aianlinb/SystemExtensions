using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using SystemExtensions.Spans;

namespace SystemExtensions.Collections {
	public static class ArrayExtensions {
		public static T[] Add<T>(this T[] array, T item) => Insert(array, array.Length, item);
		public static T[] Insert<T>(this T[] array, int index, T item) {
			var result = GC.AllocateUninitializedArray<T>(array.Length + 1);
			var span = new Span<T>(result); // cache to avoid duplicate type checks
			if (index != 0)
				new ReadOnlySpan<T>(array, 0, index).CopyToUnchecked(ref MemoryMarshal.GetReference(span)); // index check here
			Unsafe.Add(ref MemoryMarshal.GetReference(span), index) = item;
			if (index != array.Length)
				new ReadOnlySpan<T>(array, index, array.Length - index).CopyToUnchecked(
					ref Unsafe.Add(ref MemoryMarshal.GetReference(span), index + 1));
			return result;
		}

		public static T[] RemoveAt<T>(this T[] array, int index) {
			var result = GC.AllocateUninitializedArray<T>(array.Length - 1);
			var span = new Span<T>(result); // cache to avoid duplicate type checks
			if (index != 0)
				new ReadOnlySpan<T>(array, 0, index).CopyToUnchecked(ref MemoryMarshal.GetReference(span)); // index check here
			if (index != result.Length)
				new ReadOnlySpan<T>(array, index + 1, result.Length - index).CopyToUnchecked(
					ref Unsafe.Add(ref MemoryMarshal.GetReference(span), index));
			return result;
		}

		private sealed class ShadawList {
#pragma warning disable CS8618 // Used as fields layout only
			internal Array _items;
			internal int _size;
#pragma warning restore CS8618
		}
		/// <inheritdoc cref="AsList{T}(T[], int)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<T> AsList<T>(this T[] array) => AsList(array, array.Length);
		/// <summary>
		/// Returns a <see cref="List{T}"/> that use <paramref name="array"/> as its base array.
		/// </summary>
		/// <param name="array">Base array of the list</param>
		/// <param name="count">
		/// Element count of the list. (&lt;= <paramref name="array"/>.Length)<br />
		/// Don't store instances in <paramref name="array"/> outside of this range if <typeparamref name="T"/> is a reference type,
		/// otherwise they won't be collected by GC until they are overwritten.
		/// </param>
		/// <remarks>
		/// Note that the base array will be changed when <see cref="List{T}.Capacity"/> is changed.
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static List<T> AsList<T>(this T[] array, int count) {
			ArgumentNullException.ThrowIfNull(array);
			ArgumentOutOfRangeException.ThrowIfGreaterThan(unchecked((uint)count), (uint)array.Length, nameof(count));

			var result = Unsafe.As<ShadawList>(RuntimeHelpers.GetUninitializedObject(typeof(List<T>)));
			result._items = array;
			result._size = count;
			return Unsafe.As<List<T>>(result);
		}
	}
}