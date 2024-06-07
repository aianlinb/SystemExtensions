extern alias corelib;

using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SystemExtensions.Spans;
/// <remarks>
/// Some of the methods in this class have the same signature as the methods in <see cref="MemoryExtensions"/> without the <see cref="IEquatable{T}"/> limitation.<br />
/// Use those in <see cref="MemoryExtensions"/> for T that implements <see cref="IEquatable{T}"/> instead, for better performance.
/// </remarks>
public static class SpanExtensions {
	public static ReadOnlySpan<T> AsReadOnly<T>(this Span<T> span) => span;
	public static ReadOnlyMemory<T> AsReadOnly<T>(this Memory<T> memory) => memory;
	public static ReadOnlySpan<T> AsReadOnlySpan<T>(this T[] array) => array;
	public static ReadOnlyMemory<T> AsReadOnlyMemory<T>(this T[] array) => array;

	/// <summary>
	/// Returns a <see cref="Span{T}"/> that represents the memory of the by reference parameter <paramref name="value"/>.
	/// </summary>
	public static unsafe Span<byte> AsSpan<T>(this ref T value) where T : unmanaged
		=> MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref value), sizeof(T));

	/// <summary>
	/// Reads a value of type <typeparamref name="T"/> from the <paramref name="source"/>
	/// and advances the <paramref name="source"/> by <see langword="sizeof"/>(<typeparamref name="T"/>).
	/// </summary>
	public static unsafe T ReadAndSlice<T>(this scoped ref ReadOnlySpan<byte> source) where T : unmanaged {
		ref var p = ref MemoryMarshal.GetReference(source);
		source = source.Slice(sizeof(T)); // range check here
		return Unsafe.As<byte, T>(ref p);
	}
	/// <summary>
	/// Writes a <paramref name="value"/> to the <paramref name="source"/>
	/// and advances the <paramref name="source"/> by <see langword="sizeof"/>(<typeparamref name="T"/>).
	/// </summary>
	public static unsafe void WriteAndSlice<T>(this scoped ref Span<byte> source, T value) where T : unmanaged {
		ref var p2 = ref MemoryMarshal.GetReference(source);
		source = source.Slice(sizeof(T)); // range check here
		Unsafe.As<byte, T>(ref p2) = value;
	}
	/// <summary>
	/// Copies <paramref name="length"/> bytes from the <paramref name="source"/> to the <paramref name="destination"/>
	/// and advances both two spans by <paramref name="length"/>.
	/// </summary>
	public static unsafe void CopyToAndSlice<T>(this scoped ref ReadOnlySpan<T> source, scoped ref Span<T> destination, int length) where T : unmanaged {
		ref var p = ref MemoryMarshal.GetReference(source);
		ref var p2 = ref MemoryMarshal.GetReference(destination);
		source = source.Slice(length); // range check here
		destination = destination.Slice(length); // and here
		MemoryMarshal.CreateReadOnlySpan(ref p, length).CopyToUnchecked(ref p2);
	}
	/// <summary>
	/// Copies <paramref name="source"/> to the <paramref name="destination"/>
	/// and advances the <paramref name="destination"/> by <paramref name="source"/>.Length.
	/// </summary>
	public static unsafe void CopyToAndSliceDest<T>(this scoped ReadOnlySpan<T> source, scoped ref Span<T> destination) where T : unmanaged {
		ref var p2 = ref MemoryMarshal.GetReference(destination);
		destination = destination.Slice(source.Length); // range check here
		source.CopyToUnchecked(ref p2);
	}

	/// <summary>
	/// Determines whether the first element of the <paramref name="span"/> equals to the <paramref name="value"/>.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe bool StartsWith<T>(this scoped ReadOnlySpan<T> span, T value) {
		return !span.IsEmpty && EqualityComparer<T>.Default.Equals(value, MemoryMarshal.GetReference(span));
	}
	/// <summary>
	/// Determines whether the last element of the <paramref name="span"/> equals to the <paramref name="value"/>.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe bool EndsWith<T>(this scoped ReadOnlySpan<T> span, T value) {
		return !span.IsEmpty && EqualityComparer<T>.Default.Equals(value, Unsafe.Add(ref MemoryMarshal.GetReference(span), (nint)(uint)(span.Length - 1) /* force zero-extension */));
	}

	public static bool Any<T>(this scoped ReadOnlySpan<T> source, Predicate<T> predicate) {
		foreach (var item in source)
			if (predicate(item))
				return true;
		return false;
	}

	public static bool All<T>(this scoped ReadOnlySpan<T> source, Predicate<T> predicate) {
		foreach (var item in source)
			if (!predicate(item))
				return false;
		return true;
	}

	#region Unsafe
	/// <remarks>
	/// <para>This method is not always safe, please make sure the target memory is suitable for <typeparamref name="T"/>.</para>
	/// For example, if the <paramref name="span"/> is created from a <see cref="string"/>[], and the <typeparamref name="T"/> is <see cref="object"/>.<br />
	/// The returned <see cref="Span{T}"/> will accept writing any object like <see cref="List{T}"/> or <see cref="Stream"/> to the source <see cref="string"/>[].<br />
	/// And it will produce undefined behavior
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static Span<T> AsWritable<T>(this ReadOnlySpan<T> span) => MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(span), span.Length);
	/// <remarks>
	/// <para>This method is not always safe, please make sure the target memory is suitable for <typeparamref name="T"/>.</para>
	/// For example, if the <paramref name="memory"/> is created from a <see cref="string"/>[], and the <typeparamref name="T"/> is <see cref="object"/>.<br />
	/// The returned <see cref="Span{T}"/> will accept writing any object like <see cref="List{T}"/> or <see cref="Stream"/> to the source <see cref="string"/>[].<br />
	/// And it will produce undefined behavior
	/// </remarks>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public static Memory<T> AsWritable<T>(this ReadOnlyMemory<T> memory) => MemoryMarshal.AsMemory(memory);

	/// <summary>
	/// <see cref="Span{T}.Slice(int)"/> without bounds checking
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> SliceUnchecked<T>(this Span<T> source, int start) {
		return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(source), (nint)(uint)start /* force zero-extension */), source.Length - start);
	}
	/// <summary>
	/// <see cref="Span{T}.Slice(int, int)"/> without bounds checking
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> SliceUnchecked<T>(this Span<T> source, int start, int length) {
		return MemoryMarshal.CreateSpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(source), (nint)(uint)start /* force zero-extension */), length);
	}
	/// <summary>
	/// <see cref="Span{T}"/>[<see cref="Range"/>] without bounds checking
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<T> SliceUnchecked<T>(this Span<T> source, in Range range) {
		var offset = range.Start.GetOffset(source.Length);
		return MemoryMarshal.CreateSpan(
			ref Unsafe.Add(ref MemoryMarshal.GetReference(source), (nint)(uint)offset /* force zero-extension */),
			range.End.GetOffset(source.Length) - offset
		);
	}
	/// <summary>
	/// <see cref="ReadOnlySpan{T}.Slice(int)"/> without bounds checking
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> SliceUnchecked<T>(this ReadOnlySpan<T> source, int start) {
		return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(source), (nint)(uint)start /* force zero-extension */), source.Length - start);
	}
	/// <summary>
	/// <see cref="ReadOnlySpan{T}.Slice(int, int)"/> without bounds checking
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> SliceUnchecked<T>(this ReadOnlySpan<T> source, int start, int length) {
		return MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref MemoryMarshal.GetReference(source), (nint)(uint)start /* force zero-extension */), length);
	}
	/// <summary>
	/// <see cref="ReadOnlySpan{T}"/>[<see cref="Range"/>] without bounds checking
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<T> SliceUnchecked<T>(this ReadOnlySpan<T> source, in Range range) {
		var offset = range.Start.GetOffset(source.Length);
		return MemoryMarshal.CreateReadOnlySpan(
			ref Unsafe.Add(ref MemoryMarshal.GetReference(source), (nint)(uint)offset /* force zero-extension */),
			range.End.GetOffset(source.Length) - offset
		);
	}

	/// <summary>
	/// <see cref="ReadOnlySpan{T}.CopyTo(Span{T})"/> without bounds checking
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CopyToUnchecked<T>(this scoped ReadOnlySpan<T> source, scoped ref T destination) {
		corelib::System.Buffer.Memmove(ref destination, ref MemoryMarshal.GetReference(source), (nuint)source.Length);
	}
	/// <summary>
	/// <see cref="Span{T}.CopyTo(Span{T})"/> without bounds checking
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void CopyToUnchecked<T>(this scoped Span<T> source, scoped ref T destination) {
		corelib::System.Buffer.Memmove(ref destination, ref MemoryMarshal.GetReference(source), (nuint)source.Length);
	}
	#endregion

	/// <summary>
	/// See <see cref="SplitEnumerator"/>
	/// </summary>
	public static SplitEnumerator Split(this ReadOnlySpan<char> source, in char separator, StringSplitOptions options = StringSplitOptions.None) => Split(source, new ReadOnlySpan<char>(in separator), options);
	/// <summary>
	/// See <see cref="SplitEnumerator"/>
	/// </summary>
	public static SplitEnumerator Split(this ReadOnlySpan<char> source, ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None) => new(source, separator, false, options);
	/// <summary>
	/// See <see cref="SplitEnumerator"/>
	/// </summary>
	public static SplitEnumerator SplitAny(this ReadOnlySpan<char> source, ReadOnlySpan<char> separator, StringSplitOptions options = StringSplitOptions.None) => new(source, separator, true, options);
	/// <summary>
	/// Free allocation implementation of <see cref="string.Split(char, StringSplitOptions)"/>
	/// </summary>
	/// <param name="source">The source span to parse.</param>
	/// <param name="separator">A character(s) that delimits the regions in this instance.</param>
	/// <param name="options">A bitwise combination of the enumeration values that specifies whether to trim whitespace and include empty ranges.</param>
	/// <param name="isAny">
	/// Whether to split on any of the characters in <paramref name="separator"/>.<br />
	/// <see langword="true"/> if the <paramref name="separator"/> is a set; <see langword="false"/> if <paramref name="separator"/> should be treated as a single separator.
	/// </param>
	/// <remarks>
	/// This underlying uses the <see cref="MemoryExtensions.Split(ReadOnlySpan{char}, Span{Range}, ReadOnlySpan{char}, StringSplitOptions)"/> or <see cref="MemoryExtensions.SplitAny(ReadOnlySpan{char}, Span{Range}, ReadOnlySpan{char}, StringSplitOptions)"/> but handles the <see cref="Range"/> things for you.<br />
	/// Note that each instance of <see cref="SplitEnumerator"/> can be enumerated only once.<br />
	/// Do not modify the <paramref name="source"/> and <paramref name="separator"/> during the lifetime of this instance.
	/// </remarks>
	[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref struct SplitEnumerator(ReadOnlySpan<char> source, ReadOnlySpan<char> separator, bool isAny, StringSplitOptions options = StringSplitOptions.None) {
		private ReadOnlySpan<char> source = source;
		private readonly ReadOnlySpan<char> separator = separator;
		private readonly StringSplitOptions options = options;
		private readonly bool isAny = isAny || separator.Length == 1; // MemoryExtensions.Split pass true to isAny of SplitCore for single char separator parameter

		/// <remarks><see href="https://github.com/dotnet/runtime/issues/96579"/></remarks>
		private readonly bool specialCase = options == StringSplitOptions.TrimEntries && (isAny ? separator.Any(c => char.IsWhiteSpace(c)) : separator.IsWhiteSpace());

		/// <inheritdoc cref="Ranges"/>
		private Ranges ranges;
		private int index = -1;
		private int count; // assumed never negative

		public bool MoveNext() {
			var i = index + 1;

			if (i == 0) // First element
				goto Spliting;
			else if ((uint)i >= (uint)count) // Reached end or constructor not called
				return false;
			else if (count == stackAllocationLength && i == count - 1 /*Last element*/) { // More spliting needed
				if (specialCase) { // https://github.com/dotnet/runtime/issues/96579
					source = source.SliceUnchecked(ranges.UnsafeAt(i - 1).End.GetOffset(source.Length));
					source = source.SliceUnchecked(
						isAny ?
							MemoryExtensions.IndexOfAny(source, separator) + 1
							: MemoryExtensions.IndexOf(source, separator) + separator.Length
					);
				} else
					source = source.SliceUnchecked(in ranges.UnsafeAt(i));
				goto Spliting;
			}

			// Other elements
			index = i;
			return true;

		Spliting:
			index = 0;
			count = isAny ?
				MemoryExtensions.SplitAny(source, ranges.AsSpan(), separator, options) :
				MemoryExtensions.Split(source, ranges.AsSpan(), separator, options);
			return count != 0;
		}

		public readonly ReadOnlySpan<char> Current {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get {
				if ((uint)index >= (uint)count)
					ThrowHelper.Throw<IndexOutOfRangeException>();
				return source.SliceUnchecked(in ranges.UnsafeAt(index));
			}
		}

		/// <returns><see langword="this"/></returns>
		public readonly SplitEnumerator GetEnumerator() => this;

		private const int stackAllocationLength = 8; // Question: what's the best number here?
		/// <summary>
		/// Inline array of <see cref="Range"/> with <see cref="stackAllocationLength"/> elements.
		/// </summary>
		[InlineArray(stackAllocationLength)]
		private struct Ranges {
			private Range firstRange;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public Span<Range> AsSpan() => MemoryMarshal.CreateSpan(ref firstRange, stackAllocationLength);

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public ref Range UnsafeAt(int index) => ref Unsafe.Add(ref Unsafe.AsRef(ref firstRange), (nint)(uint)index);
		}
	}

	public static MemoryEnumerable<T> AsEnumerable<T>(this ReadOnlyMemory<T> memory) => new(memory);
	public static MemoryEnumerable<T> AsEnumerable<T>(this Memory<T> memory) => new(memory);
	/// <summary>
	/// <see cref="IEnumerable{T}"/> implementation of <see cref="ReadOnlyMemory{T}"/>
	/// </summary>
	/// <remarks>
	/// Use <see cref="ReadOnlySpan{T}.Enumerator"/> from <see cref="ReadOnlyMemory{T}.Span"/> instead for better performance
	/// </remarks>
	public sealed class MemoryEnumerable<T>(ReadOnlyMemory<T> memory) : IEnumerable<T>, IEnumerator<T> {
		private int index = -1;

		public IEnumerator<T> GetEnumerator() => this;
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public T Current => memory.Span[index];
		object? IEnumerator.Current => Current;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext() {
			var i = index + 1;
			if (i < memory.Length) {
				index = i;
				return true;
			}
			return false;
		}
		public void Reset() {
			index = -1;
		}
		public void Dispose() {
			memory = default;
		}
	}
}