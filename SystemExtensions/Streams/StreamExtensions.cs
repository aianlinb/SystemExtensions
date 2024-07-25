using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using SystemExtensions.Collections;
using SystemExtensions.Spans;
using SystemExtensions.Tasks;

namespace SystemExtensions.Streams;
/// <summary>
/// High performance methods for read/write structs/arrays from/to <see cref="Stream"/>
/// </summary>
/// <remarks>
/// All methods here do not process endianness, and are not thread-safe.
/// </remarks>
public static class StreamExtensions {
	/// <summary>
	/// Dangerous read <typeparamref name="T"/> with <paramref name="lengthInBytes"/> in bytes from <paramref name="stream"/> to the memory start from <paramref name="reference"/>
	/// </summary>
	/// <remarks>
	/// The caller must ensure that the memory access to the <paramref name="reference"/> with <paramref name="lengthInBytes"/> is legal
	/// </remarks>
	/// <exception cref="EndOfStreamException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void ReadExactlyCore<T>(Stream stream, scoped ref T reference, int lengthInBytes) where T : unmanaged {
		stream.ReadExactly(MemoryMarshal.CreateSpan(ref Unsafe.As<T, byte>(ref reference), lengthInBytes));
	}

	/// <summary>
	/// Dangerous write <typeparamref name="T"/> with <paramref name="lengthInBytes"/> in bytes from the memory start from <paramref name="reference"/> to <paramref name="stream"/>
	/// </summary>
	/// <remarks>
	/// The caller must ensure that the memory access to the <paramref name="reference"/> with <paramref name="lengthInBytes"/> is legal
	/// </remarks>
	/// <exception cref="EndOfStreamException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void WriteCore<T>(Stream stream, scoped ref readonly T reference, int lengthInBytes) where T : unmanaged {
		stream.Write(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in reference)), lengthInBytes));
	}

	private static readonly SpanAction<char, Stream> createStringAction = static (Span<char> span, Stream stream) => Read(stream, span);
	/// <summary>
	/// Read an UTF-16 <see cref="string"/> with the length of <paramref name="charCount"/> from the <paramref name="stream"/>
	/// </summary>
	/// <exception cref="EndOfStreamException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string ReadString(this Stream stream, int charCount) => string.Create(charCount, stream, createStringAction);

	/// <exception cref="EndOfStreamException"/>
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Read<T>(this Stream stream) where T : unmanaged {
		Read(stream, out T value);
		return value;
	}
	/// <exception cref="EndOfStreamException"/>
	[SkipLocalsInit]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe void Read<T>(this Stream stream, out T value) where T : unmanaged {
		Unsafe.SkipInit(out value);
		ReadExactlyCore(stream, ref value, sizeof(T));
	}
	/// <exception cref="EndOfStreamException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe void Read<T>(this Stream stream, T[] array) where T : unmanaged { // T is not byte
		ReadExactlyCore(stream, ref MemoryMarshal.GetArrayDataReference(array), checked(sizeof(T) * array.Length));
	}
	/// <exception cref="EndOfStreamException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe void Read<T>(this Stream stream, scoped Span<T> span) where T : unmanaged { // T is not byte
		ReadExactlyCore(stream, ref MemoryMarshal.GetReference(span), checked(sizeof(T) * span.Length));
	}
	/// <exception cref="EndOfStreamException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Read<T>(this Stream stream, List<T> list, int offset, int length) where T : unmanaged {
		var count = offset + length;
		if (list.Count < count)
			CollectionsMarshal.SetCount(list, count);
		Read(stream, CollectionsMarshal.AsSpan(list).Slice(offset, length));
	}

	public static byte[] ReadToEnd(this Stream stream) {
		if (!stream.CanSeek)
			return ReadToEndUnseekable();
		var buffer = GC.AllocateUninitializedArray<byte>(checked((int)(stream.Length - stream.Position)));
		stream.ReadExactly(buffer);
		return buffer;

		byte[] ReadToEndUnseekable() {
			using var renter = new ArrayPoolRenter<byte>(4096);
			var buffer = renter.Array;
			var l = 0;
			int lastRead;
			do {
				if (l == buffer.Length)
					buffer = renter.Resize(buffer.Length + 1, buffer.Length);
				lastRead = stream.Read(buffer, l, buffer.Length - l);
				l += lastRead;
			} while (lastRead > 0);
			return buffer[..l];
		}
	}
	public static Task<byte[]> ReadToEndAsync(this Stream stream) {
		if (!stream.CanSeek)
			return ReadToEndUnseekableAsync();
		var buffer = GC.AllocateUninitializedArray<byte>(checked((int)(stream.Length - stream.Position)));
		return stream.ReadExactlyAsync(buffer).ContinueWith(_ => buffer).AsTask();

		async Task<byte[]> ReadToEndUnseekableAsync() {
			using var renter = new ArrayPoolRenter<byte>(4096);
			var memory = new Memory<byte>(renter.Array);
			int lastRead;
			do {
				if (memory.IsEmpty) {
					var length = renter.Array.Length;
					memory = new Memory<byte>(renter.Resize(length + 1, length)).Slice(length);
				}
				lastRead = await stream.ReadAsync(memory).ConfigureAwait(false);
				memory = memory.Slice(lastRead);
			} while (lastRead > 0);
			return renter.Array[..(renter.Array.Length - memory.Length)];
		}
	}

	/// <summary>
	/// Write <paramref name="str"/> as an UTF-16 string to the <paramref name="stream"/>
	/// </summary>
	/// <exception cref="EndOfStreamException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Write(this Stream stream, string str) {
		WriteCore(stream, ref Unsafe.AsRef(in str.GetPinnableReference()), checked(str.Length * 2));
	}
	/// <exception cref="EndOfStreamException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe void Write<T>(this Stream stream, scoped in T value) where T : unmanaged {
		WriteCore(stream, ref Unsafe.AsRef(in value), sizeof(T));
	}
	/// <exception cref="EndOfStreamException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe void Write<T>(this Stream stream, T[] array) where T : unmanaged { // T is not byte
		WriteCore(stream, ref MemoryMarshal.GetArrayDataReference(array), checked(sizeof(T) * array.Length));
	}
	/// <exception cref="EndOfStreamException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe void Write<T>(this Stream stream, scoped ReadOnlySpan<T> span) where T : unmanaged { // T is not byte
		WriteCore(stream, ref MemoryMarshal.GetReference(span), checked(sizeof(T) * span.Length));
	}
	/// <exception cref="EndOfStreamException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Write<T>(this Stream stream, List<T> list, int offset, int length) where T : unmanaged {
		Write(stream, CollectionsMarshal.AsSpan(list).Slice(offset, length).AsReadOnly());
	}


	/// <summary>
	/// Write <paramref name="str"/> as an UTF-16 string to the <paramref name="writer"/>
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Write(this IBufferWriter<byte> writer, string str) {
		Write(writer, str.AsSpan());
	}
	/// <exception cref="ArgumentOutOfRangeException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Write<T>(this IBufferWriter<byte> writer, scoped in T value) where T : unmanaged {
		BuffersExtensions.Write(writer, SpanExtensions.AsReadOnlySpan(in value));
	}
	/// <exception cref="ArgumentOutOfRangeException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe void Write<T>(this IBufferWriter<byte> writer, T[] array) where T : unmanaged { // T is not byte
		Write(writer, new ReadOnlySpan<T>(array));
	}
	/// <exception cref="ArgumentOutOfRangeException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe void Write<T>(this IBufferWriter<byte> writer, scoped ReadOnlySpan<T> span) where T : unmanaged { // T is not byte
		BuffersExtensions.Write(writer, MemoryMarshal.AsBytes(span));
	}
	/// <exception cref="ArgumentOutOfRangeException"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Write<T>(this IBufferWriter<byte> writer, List<T> list, int offset, int length) where T : unmanaged {
		Write(writer, CollectionsMarshal.AsSpan(list).Slice(offset, length).AsReadOnly());
	}

	/// <summary>
	/// See <see cref="SubStream"/>
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SubStream Slice(this Stream stream, long offset, long length = -1) => new(stream, offset, length);
	/// <summary>
	/// See <see cref="SubStream"/>
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static SubStream Slice(this Stream stream, LongRange range) => new(stream, range);
	/// <summary>
	/// See <see cref="BufferWriterWrapper"/>
	/// </summary>
	/// <returns><see cref="IBufferWriter{T}"/> of <see cref="byte"/></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static BufferWriterWrapper AsIBufferWriter(this MemoryStream stream) => new(stream);
	/// <summary>
	/// See <see cref="BufferWriterStream(IBufferWriter{byte})"/>
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static BufferWriterStream AsStream(this IBufferWriter<byte> writer) => new(writer);
}