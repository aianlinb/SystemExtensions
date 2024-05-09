using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using SystemExtensions.Spans;

namespace SystemExtensions.Streams {
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
			/*T value;
			stream.ReadExactly(new Span<byte>(&value, sizeof(T)));
			return value;*/
			Read(stream, out T value);
			return value;
		}
		/// <exception cref="EndOfStreamException"/>
		[SkipLocalsInit]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void Read<T>(this Stream stream, out T value) where T : unmanaged {
			/*fixed (T* p = &value)
				stream.ReadExactly(new Span<byte>(p, sizeof(T)));*/
			Unsafe.SkipInit(out value);
			ReadExactlyCore(stream, ref value, sizeof(T));
		}
		/// <exception cref="EndOfStreamException"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void Read<T>(this Stream stream, T[] array) where T : unmanaged { // T is not byte
																							   //Read(stream, array.AsSpan());
			ReadExactlyCore(stream, ref MemoryMarshal.GetArrayDataReference(array), checked(sizeof(T) * array.Length));
		}
		/// <exception cref="EndOfStreamException"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void Read<T>(this Stream stream, scoped Span<T> span) where T : unmanaged { // T is not byte
																										 //stream.ReadExactly(MemoryMarshal.AsBytes(span));
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

		public static unsafe byte[] ReadToEnd(this Stream stream) {
			if (!stream.CanSeek)
				return ReadToEndUnseekable(stream);
			var buffer = GC.AllocateUninitializedArray<byte>(checked((int)(stream.Length - stream.Position)));
			stream.ReadExactly(buffer);
			return buffer;
		}
		private static unsafe byte[] ReadToEndUnseekable(Stream stream) {
			var buffer = ArrayPool<byte>.Shared.Rent(4096);
			try {
				var l = 0;
				int lastRead;
				do {
					if (buffer.Length == l) {
						byte[] toReturn = buffer;
						buffer = ArrayPool<byte>.Shared.Rent(buffer.Length + 1);
						new ReadOnlySpan<byte>(toReturn).CopyToUnchecked(ref MemoryMarshal.GetArrayDataReference(buffer));
						ArrayPool<byte>.Shared.Return(toReturn);
					}

					lastRead = stream.Read(buffer, l, buffer.Length - l);
					l += lastRead;
				} while (lastRead > 0);
				return buffer[..l];
			} finally {
				ArrayPool<byte>.Shared.Return(buffer);
			}
		}

		/// <summary>
		/// Write <paramref name="str"/> as an UTF-16 string to the <paramref name="stream"/>
		/// </summary>
		/// <exception cref="EndOfStreamException"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write(this Stream stream, string str) {
			/*fixed (char* p = str)
				stream.Write(new ReadOnlySpan<byte>(p, str.Length * 2));*/
			WriteCore(stream, ref Unsafe.AsRef(in str.GetPinnableReference()), checked(str.Length << 1));
		}
		/// <exception cref="EndOfStreamException"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void Write<T>(this Stream stream, scoped in T value) where T : unmanaged {
			/*fixed (T* p = &value)
				stream.Write(new ReadOnlySpan<byte>(p, sizeof(T)));*/
			WriteCore(stream, ref Unsafe.AsRef(in value), sizeof(T));
		}
		/// <exception cref="EndOfStreamException"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void Write<T>(this Stream stream, T[] array) where T : unmanaged { // T is not byte
			/*fixed (T* p = array)
				stream.Write(new ReadOnlySpan<byte>(p, array.Length * sizeof(T)));*/
			WriteCore(stream, ref MemoryMarshal.GetArrayDataReference(array), checked(sizeof(T) * array.Length));
		}
		/// <exception cref="EndOfStreamException"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe void Write<T>(this Stream stream, scoped ReadOnlySpan<T> span) where T : unmanaged { // T is not byte
			/*fixed (T* p = span)
				stream.Write(new ReadOnlySpan<byte>(p, span.Length * sizeof(T)));*/
			WriteCore(stream, ref MemoryMarshal.GetReference(span), checked(sizeof(T) * span.Length));
		}
		/// <exception cref="EndOfStreamException"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Write<T>(this Stream stream, List<T> list, int offset, int length) where T : unmanaged {
			Write(stream, CollectionsMarshal.AsSpan(list).Slice(offset, length).AsReadOnly());
		}

		/// <summary>
		/// See <see cref="SubStream"/>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SubStream Slice(this Stream stream, long offset, long length) => new(stream, offset, length);
		/// <summary>
		/// See <see cref="SubStream"/>
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SubStream Slice(this Stream stream, Range range) => new(stream, range);
	}
}