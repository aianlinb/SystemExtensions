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
	}
}